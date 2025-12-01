using Microsoft.EntityFrameworkCore;
using Artemisia.Data;
using Artemisia.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure cookie-based authentication so we can sign in an admin and use Forbid/Authorize safely
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Rotate cookie name to invalidate previously issued cookies (forces re-login)
// Change the cookie name when you want to force all clients to sign in again.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "ArtemisiaAuth_v2";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Configure DbContext (uses DefaultConnection from appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Server=(localdb)\\mssqllocaldb;Database=ArtemisiaDb;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // if DefaultConnection looks like a sqlite file connection, use Sqlite provider
    if (connectionString.TrimStart().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

var app = builder.Build();

// simple file logger to persist diagnostics across runs (helpful if process is killed)
var _logPath = Path.Combine(builder.Environment.ContentRootPath ?? Directory.GetCurrentDirectory(), "runtime-diagnostics.log");
object _logLock = new();
void AppendLog(string message)
{
    try
    {
        var line = $"[{DateTime.UtcNow:O}] {message}{Environment.NewLine}";
        lock (_logLock)
        {
            File.AppendAllText(_logPath, line);
        }
    }
    catch
    {
        // best-effort logging, swallow errors so we don't mask original issue
    }
}

// Register application lifetime events to help diagnose unexpected shutdowns
var lifetime = app.Lifetime;
lifetime.ApplicationStopping.Register(() => { Console.WriteLine("ApplicationLifetime: ApplicationStopping called."); AppendLog("ApplicationLifetime: ApplicationStopping called."); });
lifetime.ApplicationStopped.Register(() => { Console.WriteLine("ApplicationLifetime: ApplicationStopped called."); AppendLog("ApplicationLifetime: ApplicationStopped called."); });

// Global handlers to catch otherwise-silent failures and unobserved exceptions
AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
    var msg = "AppDomain UnhandledException: " + (e.ExceptionObject?.ToString() ?? "<null>");
    Console.WriteLine(msg);
    AppendLog(msg);
};
TaskScheduler.UnobservedTaskException += (s, e) =>
{
    var msg = "TaskScheduler UnobservedTaskException: " + e.Exception?.ToString();
    Console.WriteLine(msg);
    AppendLog(msg);
};
Console.CancelKeyPress += (s, e) =>
{
    var msg = "Console.CancelKeyPress received (ctrl+c)";
    Console.WriteLine(msg);
    AppendLog(msg);
};

// Improve ApplicationStopping hook: dump a little more runtime context
lifetime.ApplicationStopping.Register(() =>
{
    try
    {
        var proc = System.Diagnostics.Process.GetCurrentProcess();
        var msg = $"ApplicationLifetime: ApplicationStopping called. Process Id={proc.Id} Threads={proc.Threads.Count} WorkingSet={proc.WorkingSet64}";
        Console.WriteLine(msg);
        AppendLog(msg);

        var st = "Environment.StackTrace: " + Environment.StackTrace;
        Console.WriteLine(st);
        AppendLog(st);
    }
    catch (Exception ex)
    {
        var em = "Error while logging stopping info: " + ex.ToString();
        Console.WriteLine(em);
        AppendLog(em);
    }
});


// Apply migrations and seed default data on startup (lightweight)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Ensure database exists and apply any pending migrations
        db.Database.Migrate();

        // Seed few categories if none exist
        if (!db.Categorias.Any())
        {
            var ParentNames = new[] { "PLANNERS",
                                    "AGENDAS",
                                    "FESTAS",
                                    "SACOLAS Personalizadas",
                                    "CANECAS",
                                    "LANÇAMENTOS",
                                    "OFERTAS"
                                    };
            var parents = ParentNames.Select(m => new Categoria { Nome = m }).ToArray();
            db.Categorias.AddRange(parents);
            db.SaveChanges();

            // Seed subcategorias
            var subs = new[] {
                new { Nome = "Planners 2026", ParentName = "PLANNERS" },
                new { Nome = "Planners Personalizados", ParentName = "PLANNERS" },
                new { Nome = "Agendas 2026", ParentName = "AGENDAS" },
                new { Nome = "Agendas Personalizados", ParentName = "AGENDAS" },
                new { Nome = "Decoração de Festas", ParentName = "FESTAS" },
                new { Nome = "Topos de Bolo", ParentName = "FESTAS" },
                new { Nome = "Festa Personalizada", ParentName = "FESTAS" },
                new { Nome = "Convites Personalizados", ParentName = "FESTAS" },
                new { Nome = "Sacolas Personalizadas", ParentName = "SACOLAS Personalizadas" },
                new { Nome = "Sacolas para Presentes", ParentName = "SACOLAS Personalizadas" },
                new { Nome = "Sacolas para sua Empresa", ParentName = "SACOLAS Personalizadas" },
                new { Nome = "Canecas de Cerâmica", ParentName = "CANECAS" },
                new { Nome = "Canecas Personalizadas", ParentName = "CANECAS" },
                new { Nome = "Novos Produtos", ParentName = "LANÇAMENTOS" },
                new { Nome = "Lançamentos Exclusivos", ParentName = "LANÇAMENTOS" },
                new { Nome = "Descontos Especiais", ParentName = "OFERTAS" },
                new { Nome = "Promoções Relâmpago", ParentName = "OFERTAS" }
            };

            foreach (var s in subs)
            {
                var parent = parents.FirstOrDefault(p => string.Equals(p.Nome, s.ParentName, StringComparison.OrdinalIgnoreCase));
                if (parent != null)
                {
                    db.Categorias.Add(new Categoria { Nome = s.Nome, ParentCategoriaId = parent.Id });
                }
            }
  
            db.SaveChanges();

            // Log seed result so startup logs make it obvious whether seeding occurred
            try
            {
                var total = db.Categorias.Count();
                var totalRoot = db.Categorias.Count(c => c.ParentCategoriaId == null);
                var totalSub = db.Categorias.Count(c => c.ParentCategoriaId != null);
                var seedMsg = $"Database seed check complete. Categorias count = {total} (Root: {totalRoot}, Sub: {totalSub})";
                Console.WriteLine(seedMsg);
                AppendLog(seedMsg);
            }
            catch (Exception ex)
            {
                var em = "Error while logging seed result: " + ex.ToString();
                Console.WriteLine(em);
                AppendLog(em);
            }
        }
        else
        {
            // Categories exist but check if subcategories are missing
            var hasSubcategories = db.Categorias.Any(c => c.ParentCategoriaId != null);
            if (!hasSubcategories)
            {
                Console.WriteLine("Categorias raiz existem mas subcategorias ausentes. Adicionando subcategorias...");
                
                var existingParents = db.Categorias.Where(c => c.ParentCategoriaId == null).ToList();
                
                var subs = new[] {
                    new { Nome = "Planners 2026", ParentName = "PLANNERS" },
                    new { Nome = "Planners Personalizados", ParentName = "PLANNERS" },
                    new { Nome = "Agendas 2026", ParentName = "AGENDAS" },
                    new { Nome = "Agendas Personalizados", ParentName = "AGENDAS" },
                    new { Nome = "Decoração de Festas", ParentName = "FESTAS" },
                    new { Nome = "Topos de Bolo", ParentName = "FESTAS" },
                    new { Nome = "Festa Personalizada", ParentName = "FESTAS" },
                    new { Nome = "Convites Personalizados", ParentName = "FESTAS" },
                    new { Nome = "Sacolas Personalizadas", ParentName = "SACOLAS Personalizadas" },
                    new { Nome = "Sacolas para Presentes", ParentName = "SACOLAS Personalizadas" },
                    new { Nome = "Sacolas para sua Empresa", ParentName = "SACOLAS Personalizadas" },
                    new { Nome = "Canecas de Cerâmica", ParentName = "CANECAS" },
                    new { Nome = "Canecas Personalizadas", ParentName = "CANECAS" },
                    new { Nome = "Novos Produtos", ParentName = "LANÇAMENTOS" },
                    new { Nome = "Lançamentos Exclusivos", ParentName = "LANÇAMENTOS" },
                    new { Nome = "Descontos Especiais", ParentName = "OFERTAS" },
                    new { Nome = "Promoções Relâmpago", ParentName = "OFERTAS" }
                };

                foreach (var s in subs)
                {
                    var parent = existingParents.FirstOrDefault(p => string.Equals(p.Nome, s.ParentName, StringComparison.OrdinalIgnoreCase));
                    if (parent != null)
                    {
                        // Check if sub doesn't already exist
                        if (!db.Categorias.Any(c => c.Nome == s.Nome && c.ParentCategoriaId == parent.Id))
                        {
                            db.Categorias.Add(new Categoria { Nome = s.Nome, ParentCategoriaId = parent.Id });
                        }
                    }
                }
                
                db.SaveChanges();
                Console.WriteLine("Subcategorias adicionadas com sucesso.");
            }
            
            // Log current state
            try
            {
                var total = db.Categorias.Count();
                var totalRoot = db.Categorias.Count(c => c.ParentCategoriaId == null);
                var totalSub = db.Categorias.Count(c => c.ParentCategoriaId != null);
                var msg = $"Database already seeded. Categorias count = {total} (Root: {totalRoot}, Sub: {totalSub})";
                Console.WriteLine(msg);
                AppendLog(msg);
            }
            catch (Exception ex)
            {
                var em = "Error while logging existing categories: " + ex.ToString();
                Console.WriteLine(em);
                AppendLog(em);
            }
        }
    }
    catch (Exception ex)
    {
        // log full exception to console and file to aid debugging (stack trace included)
        var msg = "Database migrate/seed failed: " + ex.ToString();
        Console.WriteLine(msg);
        AppendLog(msg);
    }

    // Debug: always print total categories count after migration attempt to help verify seeding
    using (var scope2 = app.Services.CreateScope())
    {
        try
        {
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var total = db2.Categorias.Count();
            var msg = $"DEBUG: total categorias (post-migrate) = {total}";
            Console.WriteLine(msg);
            AppendLog(msg);
        }
        catch (Exception ex)
        {
            var em = "DEBUG: failed to read categorias count: " + ex.ToString();
            Console.WriteLine(em);
            AppendLog(em);
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Running without HTTPS. Remove automatic HTTPS redirection so the app uses HTTP only
// (this removes the "Failed to determine the https port for redirect" warning when https is not configured)
// Note: if you later want HTTPS, re-enable this line and ensure an https endpoint is configured in launchSettings or Kestrel.
// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// se MapStaticAssets/WithStaticAssets são extensões do projeto, mantenha-as.
// caso contrário remova essas linhas.
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Produto}/{action=Index}/{id?}")
    .WithStaticAssets();

try
{
    var startupMsg = $"Host starting. Process Id={System.Diagnostics.Process.GetCurrentProcess().Id}. Listening on environment: {builder.Environment.EnvironmentName}";
    Console.WriteLine(startupMsg);
    AppendLog(startupMsg);

    app.Run();
}
catch (Exception ex)
{
    // Log fatal host exception with stack trace to console and file so it's visible in logs
    var msg = "Host terminated with exception: " + ex.ToString();
    Console.WriteLine(msg);
    AppendLog(msg);
    throw;
}
