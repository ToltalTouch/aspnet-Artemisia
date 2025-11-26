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
            // Validate uploaded image (optional) - allow common image types and max 2MB
            if (imagem != null && imagem.Length > 0)
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(imagem.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("imagem", "Formato de imagem inválido. Use JPG, PNG, GIF ou WEBP.");
                    ViewBag.Categorias = await _db.Categorias.AsNoTracking().ToListAsync();
                    return View(model);
                }

                const long maxBytes = 2 * 1024 * 1024; // 2MB
                if (imagem.Length > maxBytes)
                {
                    ModelState.AddModelError("imagem", "A imagem deve ter no máximo 2MB.");
                    ViewBag.Categorias = await _db.Categorias.AsNoTracking().ToListAsync();
                    return View(model);
                }

                var imagesPath = Path.Combine(_env.WebRootPath, "images", "products");
                if (!Directory.Exists(imagesPath)) Directory.CreateDirectory(imagesPath);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(imagesPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagem.CopyToAsync(stream);
                }

                model.ImagemUrl = $"/images/products/{fileName}";
            }
            else
            {
                model.ImagemUrl ??= string.Empty;
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