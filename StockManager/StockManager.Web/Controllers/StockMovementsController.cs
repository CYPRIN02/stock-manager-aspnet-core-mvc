using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockManager.Web.Data;
using StockManager.Web.Models;

namespace StockManager.Web.Controllers;

public class StockMovementsController : Controller
{
    private readonly StockManagerDbContext _context;

    public StockMovementsController(StockManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var movements = await _context.StockMovements
            .Include(m => m.Product)
            .OrderByDescending(m => m.MovementDate)
            .ToListAsync();

        return View(movements);
    }

    public async Task<IActionResult> CreateEntry()
    {
        await LoadProductsAsync();
        return View(new StockMovement { Type = StockMovementType.Entry });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEntry(StockMovement movement)
    {
        movement.Type = StockMovementType.Entry;

        if (!ModelState.IsValid)
        {
            await LoadProductsAsync();
            return View(movement);
        }

        var product = await _context.Products.FindAsync(movement.ProductId);

        if (product == null)
        {
            return NotFound();
        }

        product.Quantity += movement.Quantity;
        movement.MovementDate = DateTime.Now;

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> CreateExit()
    {
        await LoadProductsAsync();
        return View(new StockMovement { Type = StockMovementType.Exit });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateExit(StockMovement movement)
    {
        movement.Type = StockMovementType.Exit;

        if (!ModelState.IsValid)
        {
            await LoadProductsAsync();
            return View(movement);
        }

        var product = await _context.Products.FindAsync(movement.ProductId);

        if (product == null)
        {
            return NotFound();
        }

        if (product.Quantity < movement.Quantity)
        {
            ModelState.AddModelError("Quantity", "Stock insuffisant pour effectuer cette sortie.");
            await LoadProductsAsync();
            return View(movement);
        }

        product.Quantity -= movement.Quantity;
        movement.MovementDate = DateTime.Now;

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadProductsAsync()
    {
        ViewBag.Products = new SelectList(
            await _context.Products.OrderBy(p => p.Name).ToListAsync(),
            "Id",
            "Name");
    }
}