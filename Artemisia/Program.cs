using Microsoft.EntityFrameworkCore;
using Artemisia.Data;
using Artemisia.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext (uses DefaultConnection from appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Server=(localdb)\\mssqllocaldb;Database=ArtemisiaDb;Trusted_Connection=True;MultipleActiveResultSets=true";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

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
            db.Categorias.AddRange(
                new Categoria { Nome = "Planners/Agendas" },
                new Categoria { Nome = "SACOLAS Personalizadas" },
                new Categoria { Nome = "CANECAS" },
                new Categoria { Nome = "LANÇAMENTOS" }
            );
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        // log to console — keep startup resilient
        Console.WriteLine("Database migrate/seed failed: " + ex.Message);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
