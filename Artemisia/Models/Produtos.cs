using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artemisia.Models
{
    public class Produto
    {
        // Chave primária
        public int Id { get; set; }

        //Nome do produto com validação
        [Required(ErrorMessage = "O nome do produto é obrigatório.")]
        [StringLenght(100, ErroMessage = "O nome do produto não pode exceder 100 caracteres.")]
        public string Nome { get; set; }

        // Descrição do produto com validação
        [Required(ErrorMessage = "A descrição do produto é obrigatória.")]
        [Display(namespace = "Descrição Completa do Produto")]

        // Preço do produto
        [Required(ErrorMessage = "O preço do produto é obrigatório.")]
        [Column(TypeName = "decimal(18, 2")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatIntEditMode = false)]
        public decimal Preco { get; set; }

        // URL da imagem do produto
        [Display(Name = "URL da Imagem do Produto")]
        public string ImagemUrl { get; set; }

        // Estoque
        public int QuantidadeEmEstoque { get; set;}

        //Chave estrangeira para Categoria
        [Display(namespace = "Categoria")]
        public interface CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
    }
}