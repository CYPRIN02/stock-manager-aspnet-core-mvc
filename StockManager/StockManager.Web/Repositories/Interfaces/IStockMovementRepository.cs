using StockManager.Web.Models;

namespace StockManager.Web.Repositories.Interfaces;

public interface IStockMovementRepository
{
    Task<List<StockMovement>> GetLatestMovementsAsync(int count = 5);
}