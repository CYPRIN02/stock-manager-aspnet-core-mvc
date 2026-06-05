using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockManager.Web.Data;
using StockManager.Web.Models;

namespace StockManager.Web.Controllers;

public class ProductsController : Controller
{
    private readonly StockManagerDbContext _context;

    public ProductsController(StockManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        string? search,
        int? categoryId,
        int? supplierId,
        string? status,
        string? sortOrder,
        int pageSize = 25,
        int page = 1)
    {
        if (pageSize != 25 && pageSize != 50)
            pageSize = 25;

        if (page < 1)
            page = 1;

        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Name.Contains(search) ||
                p.Reference.Contains(search));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (supplierId.HasValue)
            query = query.Where(p => p.SupplierId == supplierId.Value);

        if (status == "low")
            query = query.Where(p => p.Quantity <= p.AlertThreshold);
        else if (status == "ok")
            query = query.Where(p => p.Quantity > p.AlertThreshold);

        query = sortOrder switch
        {
            "name_desc" => query.OrderByDescending(p => p.Name),
            "reference_asc" => query.OrderBy(p => p.Reference),
            "reference_desc" => query.OrderByDescending(p => p.Reference),
            "quantity_asc" => query.OrderBy(p => p.Quantity),
            "quantity_desc" => query.OrderByDescending(p => p.Quantity),
            _ => query.OrderBy(p => p.Name)
        };

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
         
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.SupplierId = supplierId;
        ViewBag.Status = status;
        ViewBag.SortOrder = sortOrder;
        ViewBag.PageSize = pageSize;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
        ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "Id", "Name");

        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.StockMovements.OrderByDescending(m => m.MovementDate))
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        return View(product);
    }

    public async Task<IActionResult> Create()
    {
        await LoadDropdownsAsync();
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        var referenceExists = await _context.Products
            .AnyAsync(p => p.Reference == product.Reference);

        if (referenceExists)
        {
            ModelState.AddModelError("Reference", "Cette référence existe déjà. Veuillez choisir une autre référence.");
            TempData["ErrorMessage"] = "Impossible d'ajouter ce produit : référence déjà utilisée.";
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(product);
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Produit ajouté avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        await LoadDropdownsAsync();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id)
            return BadRequest();

        var referenceExists = await _context.Products
            .AnyAsync(p => p.Reference == product.Reference && p.Id != product.Id);

        if (referenceExists)
        {
            ModelState.AddModelError("Reference", "Cette référence est déjà utilisée par un autre produit.");
            TempData["ErrorMessage"] = "Modification impossible : référence déjà utilisée.";
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(product);
        }

        _context.Products.Update(product);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Produit modifié avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products
            .Include(p => p.StockMovements)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        if (product.StockMovements.Any())
        {
            TempData["ErrorMessage"] = "Impossible de supprimer ce produit car il possède des mouvements de stock.";
            return RedirectToAction(nameof(Index));
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Produit supprimé avec succès.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDropdownsAsync()
    {
        ViewBag.Categories = new SelectList(
            await _context.Categories.OrderBy(c => c.Name).ToListAsync(),
            "Id",
            "Name");

        ViewBag.Suppliers = new SelectList(
            await _context.Suppliers.OrderBy(s => s.Name).ToListAsync(),
            "Id",
            "Name");
    }
}