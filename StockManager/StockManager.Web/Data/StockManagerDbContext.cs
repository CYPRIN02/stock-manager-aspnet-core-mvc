using Microsoft.EntityFrameworkCore;
using StockManager.Web.Models;

namespace StockManager.Web.Data;

public class StockManagerDbContext : DbContext
{
    public StockManagerDbContext(DbContextOptions<StockManagerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Reference)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .Property(p => p.UnitPrice)
            .HasPrecision(18, 2);
    }
}