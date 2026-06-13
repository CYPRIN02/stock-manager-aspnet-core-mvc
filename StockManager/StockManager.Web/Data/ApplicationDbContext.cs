using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockManager.Web.Models;
using System.Reflection.Emit;

namespace StockManager.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }

    //protected override void OnModelCreating(ModelBuilder builder)
    //{
    //    base.OnModelCreating(builder);

    //    builder.Entity<Product>()
    //        .HasIndex(p => p.Reference)
    //        .IsUnique()
    //        .HasFilter("[Reference] IS NOT NULL");
    //    builder.Entity<Product>()
    //        .Property(p => p.UnitPrice)
    //        .HasPrecision(18, 2);
    //}
}