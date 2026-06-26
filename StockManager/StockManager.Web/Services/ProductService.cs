using Microsoft.EntityFrameworkCore;
using StockManager.Web.Data;
using StockManager.Web.Models;
using StockManager.Web.Services.Interfaces;

namespace StockManager.Web.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        ApplicationDbContext context,
        ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        var products = await _context.Products
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        _logger.LogDebug("Produits récupérés via ProductService | Résultats={ProductCount}", products.Count);
        return products;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            _logger.LogWarning("Produit introuvable via ProductService | ProductId={ProductId}", id);
        }

        return product;
    }

    public async Task CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Produit créé via ProductService | ProductId={ProductId} | Référence={Reference} | Nom={ProductName}",
            product.Id,
            product.Reference,
            product.Name);
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Produit modifié via ProductService | ProductId={ProductId} | Référence={Reference} | Nom={ProductName}",
            product.Id,
            product.Reference,
            product.Name);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await GetByIdAsync(id);

        if (product is null)
        {
            _logger.LogWarning("Suppression ignorée via ProductService : produit introuvable | ProductId={ProductId}", id);
            return;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Produit supprimé via ProductService | ProductId={ProductId} | Référence={Reference} | Nom={ProductName}",
            product.Id,
            product.Reference,
            product.Name);
    }
}
