using Microsoft.EntityFrameworkCore;
using Artemisia.Data;
using Artemisia.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
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

// Configure DbContext (uses DefaultConnection from appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Server=(localdb)\\mssqllocaldb;Database=ArtemisiaDb;Trusted_Connection=True;MultipleActiveResultSets=true";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

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
            var parents = new[] {
                new Categoria { Nome = "PLANNERS" },
                new Categoria { Nome = "AGENDAS" },
                new Categoria { Nome = "FESTAS" },
                new Categoria { Nome = "SACOLAS Personalizadas" },
                new Categoria { Nome = "CANECAS" },
                new Categoria { Nome = "LANÇAMENTOS" },
                new Categoria { Nome = "OFERTAS" }
            };
            db.Categorias.AddRange(parents);
            db.SaveChanges();

            // Add a couple of sample subcategories for the first two parents
            var p1 = parents[0];
            var p2 = parents[1];

            db.Categorias.AddRange(
                new Categoria { Nome = "Agendas 2026", ParentCategoriaId = p1.Id },
                new Categoria { Nome = "Planners Diários", ParentCategoriaId = p1.Id },
                new Categoria { Nome = "Sacolas Tamanho P", ParentCategoriaId = p2.Id },
                new Categoria { Nome = "Sacolas Tamanho G", ParentCategoriaId = p2.Id }
            );
            db.SaveChanges();

            // Log seed result so startup logs make it obvious whether seeding occurred
            try
            {
                var total = db.Categorias.Count();
                var seedMsg = $"Database seed check complete. Categorias count = {total}";
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
    }
    catch (Exception ex)
    {
        // log full exception to console and file to aid debugging (stack trace included)
        var msg = "Database migrate/seed failed: " + ex.ToString();
        Console.WriteLine(msg);
        AppendLog(msg);
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
