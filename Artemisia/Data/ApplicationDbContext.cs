using Microsoft.EntityFrameworkCore;
using Artemisia.Models;

namespace Artemisia.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

		public DbSet<Produto> Produtos { get; set; }
		public DbSet<Categoria> Categorias { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Ensure decimal precision for Preco to avoid truncation warnings
			modelBuilder.Entity<Produto>()
				.Property(p => p.Preco)
				.HasColumnType("decimal(18,2)");

			base.OnModelCreating(modelBuilder);
		}
	}
}
