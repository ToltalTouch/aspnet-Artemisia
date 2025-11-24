using Microsoft.AspNetCore.Mvc;
using Artemisia.Models;
using Artemisia.Data;
using Microsoft.EntityFrameworkCore;

namespace Artemisia.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProdutoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var produtos = await _context.Produtos
                                .Include(p => p.Categoria)
                                .ToListAsync();

            return View(produtos);
        }

        public async Task<IActionResult> Detalhes(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var produto = await _context.Produtos
                                .Include(p => p.Categoria)
                                .FirstOrDefaultAsync(m => m.Id == id);
            if (produto == null)
            {
                return NotFound();
            }

            return View(produto);
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Nome, Descricao, Preco, ImagemUrl, QuantidadeEmEstoque, CategoriaId")] Produto produto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(produto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(produto);
        }
    }
}