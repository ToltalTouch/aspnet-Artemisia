using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Artemisia.Data
{
    // This factory is used at design-time by the EF tools (dotnet ef) to create
    // an instance of ApplicationDbContext without relying on the application's
    // configuration (avoids issues when the project path contains special chars).
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            // Keep the same default connection string used in Program.cs for local development
            var connectionString = "Server=(localdb)\\mssqllocaldb;Database=ArtemisiaDb;Trusted_Connection=True;MultipleActiveResultSets=true";
            optionsBuilder.UseSqlServer(connectionString);
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
