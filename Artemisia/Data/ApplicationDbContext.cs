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
				.HasOne(p => p.Categoria)
				.WithMany(c => c.Produtos)
				.HasForeignKey(p => p.CategoriaId)
				.OnDelete(DeleteBehavior.Restrict);

			// Produto -> SubCategoria (opcional): sem coleção inversa
			modelBuilder.Entity<Produto>()
				.HasOne(p => p.SubCategoria)
				.WithMany() // sem navigation inverse
				.HasForeignKey(p => p.SubCategoriaId)
				.OnDelete(DeleteBehavior.Restrict);
					base.OnModelCreating(modelBuilder);
				}
	}
}
