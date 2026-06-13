using Microsoft.EntityFrameworkCore;
using StockManager.Web.Data;
using StockManager.Web.Models;
using StockManager.Web.Repositories.Interfaces;

namespace StockManager.Web.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<int> CountAsync()
    {
        return _context.Products.CountAsync();
    }

    public Task<decimal> GetTotalStockValueAsync()
    {
        return _context.Products
            .SumAsync(p => p.Quantity * p.UnitPrice);
    }

    public Task<int> CountLowStockAsync()
    {
        return _context.Products
            .CountAsync(p => p.Quantity <= p.AlertQuantity);
    }

    public Task<List<Product>> GetLowStockProductsAsync()
    {
        return _context.Products
            .Include(p => p.Category)
            .Where(p => p.Quantity <= p.AlertQuantity)
            .OrderBy(p => p.Quantity)
            .Take(5)
            .ToListAsync();
    }
}