using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Artemisia.Data;
using Artemisia.Models;

namespace Artemisia.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 12;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /categoria/{categoryId}
        [Route("categoria/{categoryId:int}")]
        public async Task<IActionResult> Index(int categoryId, int page = 1)
        {
            var categoria = await _context.Categorias
                .Include(c => c.SubCategorias)
                .FirstOrDefaultAsync(c => c.Id == categoryId);
            if (categoria == null) return NotFound();

            var query = _context.Produtos
                                .Include(p => p.Categoria)
                                .Where(p => p.CategoriaId == categoryId)
                                .AsQueryable();

            var produtos = await query.OrderBy(p => p.Nome)
                                      .Skip((page - 1) * PageSize)
                                      .Take(PageSize)
                                      .ToListAsync();

            ViewData["CurrentCategory"] = categoria.Nome;
            ViewData["CurrentCategoryId"] = categoryId;
            ViewData["CurrentPage"] = page;
            // Passa apenas categorias raiz (ParentCategoriaId == null) incluindo SubCategorias
            ViewData["Categories"] = await _context.Categorias
                                                      .Include(c => c.SubCategorias)
                                                      .Where(c => c.ParentCategoriaId == null)
                                                      .OrderBy(c => c.Nome)
                                                      .ToListAsync();

            return View(produtos);
        }

        // GET (AJAX) -> returns partial HTML of product grid
        // /categoria/products?categoryId=1&page=1
        [HttpGet("categoria/products")]
        public async Task<IActionResult> ProductsFragment(int categoryId, int page = 1)
        {
            var query = _context.Produtos
                                .Include(p => p.Categoria)
                                .Where(p => p.CategoriaId == categoryId)
                                .AsQueryable();

            var produtos = await query.OrderBy(p => p.Nome)
                                      .Skip((page - 1) * PageSize)
                                      .Take(PageSize)
                                      .ToListAsync();

            return PartialView("_ProductGrid", produtos);
        }
    }
}
