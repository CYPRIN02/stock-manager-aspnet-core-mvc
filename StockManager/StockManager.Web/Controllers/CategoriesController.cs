using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockManager.Web.Data;
using StockManager.Web.Models;

namespace StockManager.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ApplicationDbContext context,
        ILogger<CategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? search, string sortOrder = "name_asc", int pageSize = 25)
    {
        var query = _context.Categories
            .Include(c => c.Products)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search));
        }

        query = sortOrder switch
        {
            "name_desc" => query.OrderByDescending(c => c.Name),
            "products_asc" => query.OrderBy(c => c.Products.Count),
            "products_desc" => query.OrderByDescending(c => c.Products.Count),
            _ => query.OrderBy(c => c.Name)
        };

        pageSize = pageSize == 50 ? 50 : 25;

        ViewBag.Search = search;
        ViewBag.SortOrder = sortOrder;
        ViewBag.PageSize = pageSize;

        var categories = await query.Take(pageSize).ToListAsync();
        
        _logger.LogInformation(
            "Liste catégories consultée | Recherche={Search} | Tri={SortOrder} | PageSize={PageSize} | Résultats={ResultCount} | Utilisateur={UserName}",
            search,
            sortOrder,
            pageSize,
            categories.Count,
            User.Identity?.Name);

        return View(categories);
    }

    public async Task<IActionResult> Details(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .ThenInclude(p => p.Supplier)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            _logger.LogWarning(
                "Catégorie introuvable lors de la consultation détail | CategoryId={CategoryId} | Utilisateur={UserName}",
                id,
                User.Identity?.Name);
        
            return NotFound();
        }

        _logger.LogInformation(
           "Détail catégorie consulté | CategoryId={CategoryId} | Nom={CategoryName} | Produits={ProductCount} | Utilisateur={UserName}",
           category.Id,
           category.Name,
           category.Products.Count,
           User.Identity?.Name);

        return View(category);
    }

    public IActionResult Create()
    {
        _logger.LogDebug("Ouverture formulaire création catégorie | Utilisateur={UserName}", User.Identity?.Name);
        return View(new Category());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        var exists = await _context.Categories
            .AnyAsync(c => c.Name == category.Name);

        if (exists)
        {
            _logger.LogWarning(
                "Création catégorie refusée : doublon | Nom={CategoryName} | Utilisateur={UserName}",
                category.Name,
                User.Identity?.Name); 
            
            ModelState.AddModelError("Name", "Cette catégorie existe déjà.");
            TempData["ErrorMessage"] = "Impossible d'ajouter : catégorie déjà existante.";
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "Création catégorie invalide | Nom={CategoryName} | Utilisateur={UserName}",
                category.Name,
                User.Identity?.Name);
            return View(category);
        }

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
           "Catégorie créée | CategoryId={CategoryId} | Nom={CategoryName} | CrééePar={UserName}",
           category.Id,
           category.Name,
           User.Identity?.Name);

        TempData["SuccessMessage"] = "Catégorie ajoutée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            _logger.LogWarning(
                "Catégorie introuvable lors de l'ouverture modification | CategoryId={CategoryId} | Utilisateur={UserName}",
                id,
                User.Identity?.Name);
            return NotFound();
        }

        _logger.LogDebug(
            "Ouverture formulaire modification catégorie | CategoryId={CategoryId} | Utilisateur={UserName}",
            id,
            User.Identity?.Name);

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id)
        {
            _logger.LogWarning(
                "Modification catégorie refusée : identifiant incohérent | RouteId={RouteId} | ModelId={ModelId} | Utilisateur={UserName}",
                id,
                category.Id,
                User.Identity?.Name);
            return BadRequest();
        }

        var exists = await _context.Categories
            .AnyAsync(c => c.Name == category.Name && c.Id != category.Id);

        if (exists)
        {
            _logger.LogWarning(
                "Modification catégorie refusée : doublon | CategoryId={CategoryId} | Nom={CategoryName} | Utilisateur={UserName}",
                category.Id,
                category.Name,
                User.Identity?.Name);

            ModelState.AddModelError("Name", "Ce nom de catégorie est déjà utilisé.");
            TempData["ErrorMessage"] = "Modification impossible : catégorie déjà existante.";
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "Modification catégorie invalide | CategoryId={CategoryId} | Nom={CategoryName} | Utilisateur={UserName}",
                category.Id,
                category.Name,
                User.Identity?.Name);
            return View(category);
        }

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Catégorie modifiée | CategoryId={CategoryId} | Nom={CategoryName} | ModifiéePar={UserName}",
            category.Id,
            category.Name,
            User.Identity?.Name);

        TempData["SuccessMessage"] = "Catégorie modifiée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            _logger.LogWarning(
                "Catégorie introuvable lors de l'ouverture suppression | CategoryId={CategoryId} | Utilisateur={UserName}",
                id,
                User.Identity?.Name);
            return NotFound();
        }

        _logger.LogDebug(
            "Ouverture confirmation suppression catégorie | CategoryId={CategoryId} | ProduitsLiés={ProductCount} | Utilisateur={UserName}",
            category.Id,
            category.Products.Count,
            User.Identity?.Name);

        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            _logger.LogWarning(
                "Suppression catégorie impossible : catégorie introuvable | CategoryId={CategoryId} | Utilisateur={UserName}",
                id,
                User.Identity?.Name);
            return NotFound();
        }

        if (category.Products.Any())
        {
            _logger.LogWarning(
                "Suppression catégorie bloquée : produits liés | CategoryId={CategoryId} | ProduitsLiés={ProductCount} | Utilisateur={UserName}",
                category.Id,
                category.Products.Count,
                User.Identity?.Name);

            TempData["ErrorMessage"] = "Impossible de supprimer cette catégorie car elle contient des produits.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Catégorie supprimée | CategoryId={CategoryId} | Nom={CategoryName} | SuppriméePar={UserName}",
            category.Id,
            category.Name,
            User.Identity?.Name);

        TempData["SuccessMessage"] = "Catégorie supprimée avec succès.";
        return RedirectToAction(nameof(Index));
    }
}
