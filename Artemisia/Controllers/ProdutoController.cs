using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Artemisia.Data;
using Artemisia.Models;

namespace Artemisia.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProdutoController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categorias = await _db.Categorias.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Produto model, IFormFile imagem)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await _db.Categorias.AsNoTracking().ToListAsync();
                return View(model);
            }

            if (imagem != null && imagem.Length > 0)
            {
                var imagesPath = Path.Combine(_env.WebRootPath, "images", "products");
                if (!Directory.Exists(imagesPath)) Directory.CreateDirectory(imagesPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imagem.FileName)}";
                var filePath = Path.Combine(imagesPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagem.CopyToAsync(stream);
                }

                model.ImagemUrl = $"/images/products/{fileName}";
            }
            _db.Produtos.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {
            var produtos = await _db.Produtos.Include(p => p.Categoria).ToListAsync();
            return View(produtos);
        }
    }
}