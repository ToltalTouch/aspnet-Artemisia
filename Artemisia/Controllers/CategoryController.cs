using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.log;
using System.Threading.Tasks;

namespace Artemisia.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public CategoryController(ApplicationDbContext context) => _context = context;

        [Route("categoria/{categorySlug}/{subCategorySlug?}")]
        public async Task<IActionResult> Index(string categorySlug, string? subCategorySlug, int page = 1)
        {
            if (string.IsNullOrEmpty(categorySlug)) return NotFound();

            var query = _context.Produtos
                                .Include(p => p.Categoria)
                                .AsQueryable()
                                .Where(p => p.Categoria.Slug == categorySlug);

            if(!string.IsNullOrEmpty(subCategorySlug))
                query = query.Where(pp => pp.SubCategoria != null && pp.SubCategoria.Slug == subCategorySlug);

            var produtos = await query.OrderBy(p => p.Nome)
                                        .Skip((page - 1) * PageSize)
                                        .Take(PageSize)
                                        .ToListAsync();

            ViewData["CategorySlug"] = categorySlug;
            ViewData["SubCategorySlug"] = subCategorySlug;
            ViewData["Page"] = page;

            return View(produtos);
        }

        [HttpGet("categoria/products")]
        public async Task<IActionResult> ProductsFragment(string categorySlug, string? subCategorySlug, int pages = 1)
        {
            if (string.IsNullOrEmpty(categorySlug)) return BadRequest();

            var query = _context.Produtos
                                .Include(p => p.Categoria)
                                .AsQueryable()
                                .Where(p => p.Categoria.Slug == categorySlug);

            if (!string.IsNullOrEmpty(subCategorySlug))
                query = query.Where(p => p.SubCategoria != null && p.SubCategoria.Slug == subCategorySlug);

            var produtos = await query.OrderBy(p => p.Nome)
                                        .Skip((pages - 1) * PageSize)
                                        .Take(PageSize)
                                        .ToListAsync();

            return PartialView("_ProductsFragment", produtos);
        }
        
    }
}