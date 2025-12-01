using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using Artemisia.Data;
using Artemisia.Models;

namespace Artemisia.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public ProdutoController(ApplicationDbContext db, IWebHostEnvironment env, IConfiguration config)
        {
            _db = db;
            _env = env;
            _config = config;
        }

        private bool IsAdmin()
        {
            // Prefer cookie/claims-based admin (set after login). Keep header fallback for API/tools.
            var adminKey = _config["AdminKey"];
            try
            {
                if (User?.Identity?.IsAuthenticated == true && User.IsInRole("Admin")) return true;
            }
            catch
            {
                // ignore if User is not available
            }

            if (string.IsNullOrEmpty(adminKey)) return false;
            if (!Request.Headers.TryGetValue("X-Admin-Key", out var provided)) return false;
            return string.Equals(provided.ToString(), adminKey, StringComparison.Ordinal);
        }

        public async Task<IActionResult> Create()
        {
            if (!IsAdmin()) return Forbid();
            // Load root categories with their subcategories for the dropdowns
            ViewBag.Categorias = await _db.Categorias
                .Include(c => c.SubCategorias)
                .Where(c => c.ParentCategoriaId == null)
                .OrderBy(c => c.Nome)
                .ToListAsync();
            
            // Also load for menu
            ViewData["Categories"] = await _db.Categorias
                .Include(c => c.SubCategorias)
                .Where(c => c.ParentCategoriaId == null)
                .OrderBy(c => c.Nome)
                .ToListAsync();
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Produto model, IFormFile imagem)
        {
            if (!IsAdmin()) return Forbid();
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await _db.Categorias
                    .Include(c => c.SubCategorias)
                    .Where(c => c.ParentCategoriaId == null)
                    .OrderBy(c => c.Nome)
                    .AsNoTracking()
                    .ToListAsync();
                ViewData["Categories"] = ViewBag.Categorias;
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
                    ViewBag.Categorias = await _db.Categorias
                        .Include(c => c.SubCategorias)
                        .Where(c => c.ParentCategoriaId == null)
                        .OrderBy(c => c.Nome)
                        .AsNoTracking()
                        .ToListAsync();
                    ViewData["Categories"] = ViewBag.Categorias;
                    return View(model);
                }

                const long maxBytes = 2 * 1024 * 1024; // 2MB
                if (imagem.Length > maxBytes)
                {
                    ModelState.AddModelError("imagem", "A imagem deve ter no máximo 2MB.");
                    ViewBag.Categorias = await _db.Categorias
                        .Include(c => c.SubCategorias)
                        .Where(c => c.ParentCategoriaId == null)
                        .OrderBy(c => c.Nome)
                        .AsNoTracking()
                        .ToListAsync();
                    ViewData["Categories"] = ViewBag.Categorias;
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
            try
            {
                await _db.SaveChangesAsync();
                TempData["Success"] = "Produto salvo com sucesso.";
            }
            catch (Exception ex)
            {
                // Log and show friendly message
                Console.WriteLine("Save product failed: " + ex.Message);
                TempData["Error"] = "Ocorreu um erro ao salvar o produto.";
                ViewBag.Categorias = await _db.Categorias
                    .Include(c => c.SubCategorias)
                    .Where(c => c.ParentCategoriaId == null)
                    .OrderBy(c => c.Nome)
                    .AsNoTracking()
                    .ToListAsync();
                ViewData["Categories"] = ViewBag.Categorias;
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Categories"] = await _db.Categorias
                .Include(c => c.SubCategorias)
                .Where(c => c.ParentCategoriaId == null)
                .OrderBy(c => c.Nome)
                .ToListAsync();
            var produtos = await _db.Produtos.Include(p => p.Categoria).ToListAsync();
            return View(produtos);
        }
    }
}