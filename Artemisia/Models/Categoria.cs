using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Artemisia.Models
{
	public class Categoria
	{
		public int Id { get; set; }
		public string Nome { get; set; } = string.Empty;
		public ICollection<Produto> Produtos { get; set;} = new List<Produto>();
	}
}
