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

    public async Task<IActionResult> Index()
    {
        try
        {
            var suppliers = await _context.Suppliers
                .Include(s => s.Products)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(suppliers);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des fournisseurs.";
            return View(new List<Supplier>());
        }
    }

    public IActionResult Create()
    {
        return View(new Supplier());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supplier supplier)
    {
        if (!ModelState.IsValid)
            return View(supplier);

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

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

        if (!ModelState.IsValid)
            return View(supplier);

        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync();

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
            ModelState.AddModelError("", "Impossible de supprimer ce fournisseur car il est lié à des produits.");
            return View(supplier);
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}