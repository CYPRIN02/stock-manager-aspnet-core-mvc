using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockManager.Web.Data;
using StockManager.Web.Models;

namespace StockManager.Web.Controllers;

public class CategoriesController : Controller
{
    private readonly StockManagerDbContext _context;

    public CategoriesController(StockManagerDbContext context)
    {
        _context = context;
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

        return View(categories);
    }

    public async Task<IActionResult> Details(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .ThenInclude(p => p.Supplier)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound();

        return View(category);
    }

    public IActionResult Create()
    {
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
            ModelState.AddModelError("Name", "Cette catégorie existe déjà.");
            TempData["ErrorMessage"] = "Impossible d'ajouter : catégorie déjà existante.";
        }

        if (!ModelState.IsValid)
            return View(category);

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Catégorie ajoutée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound();

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id)
            return BadRequest();

        var exists = await _context.Categories
            .AnyAsync(c => c.Name == category.Name && c.Id != category.Id);

        if (exists)
        {
            ModelState.AddModelError("Name", "Ce nom de catégorie est déjà utilisé.");
            TempData["ErrorMessage"] = "Modification impossible : catégorie déjà existante.";
        }

        if (!ModelState.IsValid)
            return View(category);

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Catégorie modifiée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound();

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
            return NotFound();

        if (category.Products.Any())
        {
            TempData["ErrorMessage"] = "Impossible de supprimer cette catégorie car elle contient des produits.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Catégorie supprimée avec succès.";
        return RedirectToAction(nameof(Index));
    }
}