using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artemisia.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
        // Subcategoria é opcional
        public int? SubCategoriaId { get; set; }
        public Categoria? SubCategoria { get; set;}
        public string Descricao { get; set; } = string.Empty;
        public string ImagemUrl { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int QuantidadeEmEstoque { get; set; }
    }
}