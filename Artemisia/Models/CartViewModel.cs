using System.Collections.Generic;
using System.Linq;

namespace Artemisia.Models
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal Total => Items?.Sum(item => item.Preco * item.Quantidade) ?? 0m;
    }

    public class CartItemViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
    }
}