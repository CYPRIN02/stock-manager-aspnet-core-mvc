using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockManager.Web.Data;
using StockManager.Web.Models;

namespace StockManager.Web.Controllers;

public class SuppliersController : Controller
{
    private readonly StockManagerDbContext _context;

    public SuppliersController(StockManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, string sortOrder = "name_asc", int pageSize = 25)
    {
        var query = _context.Suppliers
            .Include(s => s.Products)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                s.Name.Contains(search) ||
                (s.Email != null && s.Email.Contains(search)) ||
                (s.Phone != null && s.Phone.Contains(search)));
        }

        query = sortOrder switch
        {
            "name_desc" => query.OrderByDescending(s => s.Name),
            "products_asc" => query.OrderBy(s => s.Products.Count),
            "products_desc" => query.OrderByDescending(s => s.Products.Count),
            _ => query.OrderBy(s => s.Name)
        };

        pageSize = pageSize == 50 ? 50 : 25;

        ViewBag.Search = search;
        ViewBag.SortOrder = sortOrder;
        ViewBag.PageSize = pageSize;

        var suppliers = await query.Take(pageSize).ToListAsync();

        return View(suppliers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Products)
            .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
            return NotFound();

        return View(supplier);
    }

    public IActionResult Create()
    {
        return View(new Supplier());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supplier supplier)
    {
        var exists = await _context.Suppliers
            .AnyAsync(s => s.Name == supplier.Name);

        if (exists)
        {
            ModelState.AddModelError("Name", "Ce fournisseur existe déjà.");
            TempData["ErrorMessage"] = "Impossible d'ajouter : fournisseur déjà existant.";
        }

        if (!ModelState.IsValid)
            return View(supplier);

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Fournisseur ajouté avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);

        if (supplier == null)
            return NotFound();

        return View(supplier);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Supplier supplier)
    {
        if (id != supplier.Id)
            return BadRequest();

        var exists = await _context.Suppliers
            .AnyAsync(s => s.Name == supplier.Name && s.Id != supplier.Id);

        if (exists)
        {
            ModelState.AddModelError("Name", "Ce nom de fournisseur est déjà utilisé.");
            TempData["ErrorMessage"] = "Modification impossible : fournisseur déjà existant.";
        }

        if (!ModelState.IsValid)
            return View(supplier);

        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Fournisseur modifié avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
            return NotFound();

        return View(supplier);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
            return NotFound();

        if (supplier.Products.Any())
        {
            TempData["ErrorMessage"] = "Impossible de supprimer ce fournisseur car il est lié à des produits.";
            return RedirectToAction(nameof(Index));
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Fournisseur supprimé avec succès.";
        return RedirectToAction(nameof(Index));
    }
}