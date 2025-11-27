using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Artemisia.Models
{
	public class Categoria
	{
		public int Id { get; set; }
		public string Nome { get; set; } = string.Empty;

		// Self-referencing hierarchy: optional parent and collection of subcategories
		// ParentCategoriaId is optional for root categories
		public int? ParentCategoriaId { get; set; }
		public Categoria? ParentCategoria { get; set; }

		public ICollection<Categoria> SubCategorias { get; set; } = new List<Categoria>();

		public ICollection<Produto> Produtos { get; set;} = new List<Produto>();
	}
}
