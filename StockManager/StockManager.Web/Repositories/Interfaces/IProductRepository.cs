using StockManager.Web.Models;

namespace StockManager.Web.Repositories.Interfaces;

public interface IProductRepository
{
    Task<int> CountAsync();
    Task<decimal> GetTotalStockValueAsync();
    Task<int> CountLowStockAsync();
    Task<List<Product>> GetLowStockProductsAsync();
}