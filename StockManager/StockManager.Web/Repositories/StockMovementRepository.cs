using Microsoft.EntityFrameworkCore;
using StockManager.Web.Data;
using StockManager.Web.Models;
using StockManager.Web.Repositories.Interfaces;

namespace StockManager.Web.Repositories;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly ApplicationDbContext _context;

    public StockMovementRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<StockMovement>> GetLatestMovementsAsync(int count = 5)
    {
        return _context.StockMovements
            .Include(m => m.Product)
            .OrderByDescending(m => m.MovementDate)
            .Take(count)
            .ToListAsync();
    }
}