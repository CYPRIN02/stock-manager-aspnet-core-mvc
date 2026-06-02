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

    public async Task<IActionResult> Index(string? search)
    {
        var products = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            products = products.Where(p =>
                p.Name.Contains(search) ||
                p.Reference.Contains(search));
        }

        ViewBag.Search = search;

        return View(await products.OrderBy(p => p.Name).ToListAsync());
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
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(product);
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

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