using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Artemisia.Data;
using Artemisia.Models.ViewModels;

namespace Artemisia.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Categories"] = await _db.Categorias
                .Include(c => c.SubCategorias)
                .Where(c => c.ParentCategoriaId == null)
                .OrderBy(c => c.Nome)
                .ToListAsync();
            
            var produtos = await _db.Produtos.Include(p => p.Categoria).ToListAsync();

            var model = new HomeIndexViewModel
            {
                Produtos = produtos
            };

            return View(model);
        }
    }
}
