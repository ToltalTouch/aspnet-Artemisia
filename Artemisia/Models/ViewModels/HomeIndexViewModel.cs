using System.Collections.Generic;
using Artemisia.Models;

namespace Artemisia.Models.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Produto> Produtos { get; set; } = new List<Produto>();
    }
}
