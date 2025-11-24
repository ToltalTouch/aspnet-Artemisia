using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artemisia.Models
{
    public class Produto
    {
        // Chave prim�ria
        public int Id { get; set; }

        //Nome do produto com valida��o
    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome do produto não pode exceder 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

        // Descri��o do produto com valida��o
    [Required(ErrorMessage = "A descrição do produto é obrigatória.")]
    [Display(Name = "Descrição Completa do Produto")]
    public string Descricao { get; set; } = string.Empty;

        // Pre�o do produto
        [Required(ErrorMessage = "O pre�o do produto � obrigat�rio.")]
        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal Preco { get; set; }

        // URL da imagem do produto
    [Display(Name = "URL da Imagem do Produto")]
    public string ImagemUrl { get; set; } = string.Empty;

        // Estoque
        public int QuantidadeEmEstoque { get; set; }

        //Chave estrangeira para Categoria
        [Display(Name = "Categoria")]
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    }
}