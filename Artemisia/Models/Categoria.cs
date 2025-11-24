using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Artemisia.Models
{
	public class Categoria
	{
		public int Id { get; set; }

	[Required]
	[StringLength(100)]
	public string Nome { get; set; } = string.Empty;

		// Navigation property
		public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
	}
}
