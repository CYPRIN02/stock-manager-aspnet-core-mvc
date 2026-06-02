using StockManager.Web.Models;

namespace StockManager.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(StockManagerDbContext context)
    {
        if (context.Categories.Any() ||
            context.Suppliers.Any() ||
            context.Products.Any() ||
            context.StockMovements.Any())
        {
            return;
        }

        var categories = new List<Category>
        {
            new() { Name = "Informatique" },
            new() { Name = "Bureautique" },
            new() { Name = "Réseau" },
            new() { Name = "Accessoires" },
            new() { Name = "Stockage" }
        };

        await context.Categories.AddRangeAsync(categories);

        var suppliers = new List<Supplier>
        {
            new() { Name = "Dell France", Email = "contact@dell.fr", Phone = "01 44 55 66 77" },
            new() { Name = "HP Store", Email = "support@hp.fr", Phone = "01 22 33 44 55" },
            new() { Name = "Logitech Partner", Email = "sales@logitech.fr", Phone = "01 87 65 43 21" },
            new() { Name = "Cisco Network", Email = "contact@cisco.fr", Phone = "01 98 76 54 32" }
        };

        await context.Suppliers.AddRangeAsync(suppliers);
        await context.SaveChangesAsync();

        var products = new List<Product>
        {
            new()
            {
                Name = "PC Portable Dell Latitude",
                Reference = "PROD-001",
                Quantity = 15,
                UnitPrice = 899.99m,
                AlertThreshold = 5,
                CategoryId = categories[0].Id,
                SupplierId = suppliers[0].Id
            },
            new()
            {
                Name = "Clavier Logitech K120",
                Reference = "PROD-002",
                Quantity = 4,
                UnitPrice = 19.99m,
                AlertThreshold = 10,
                CategoryId = categories[3].Id,
                SupplierId = suppliers[2].Id
            },
            new()
            {
                Name = "Souris Logitech M185",
                Reference = "PROD-003",
                Quantity = 25,
                UnitPrice = 14.99m,
                AlertThreshold = 8,
                CategoryId = categories[3].Id,
                SupplierId = suppliers[2].Id
            },
            new()
            {
                Name = "Écran HP 24 pouces",
                Reference = "PROD-004",
                Quantity = 8,
                UnitPrice = 159.99m,
                AlertThreshold = 5,
                CategoryId = categories[0].Id,
                SupplierId = suppliers[1].Id
            },
            new()
            {
                Name = "Switch Cisco 24 ports",
                Reference = "PROD-005",
                Quantity = 2,
                UnitPrice = 349.99m,
                AlertThreshold = 3,
                CategoryId = categories[2].Id,
                SupplierId = suppliers[3].Id
            },
            new()
            {
                Name = "Disque SSD Samsung 1To",
                Reference = "PROD-006",
                Quantity = 12,
                UnitPrice = 89.99m,
                AlertThreshold = 5,
                CategoryId = categories[4].Id,
                SupplierId = suppliers[0].Id
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        var movements = new List<StockMovement>
        {
            new()
            {
                ProductId = products[0].Id,
                Type = StockMovementType.Entry,
                Quantity = 15,
                Reason = "Stock initial",
                MovementDate = DateTime.Now.AddDays(-5)
            },
            new()
            {
                ProductId = products[1].Id,
                Type = StockMovementType.Entry,
                Quantity = 10,
                Reason = "Réapprovisionnement",
                MovementDate = DateTime.Now.AddDays(-4)
            },
            new()
            {
                ProductId = products[1].Id,
                Type = StockMovementType.Exit,
                Quantity = 6,
                Reason = "Sortie matériel interne",
                MovementDate = DateTime.Now.AddDays(-2)
            },
            new()
            {
                ProductId = products[4].Id,
                Type = StockMovementType.Entry,
                Quantity = 2,
                Reason = "Stock initial réseau",
                MovementDate = DateTime.Now.AddDays(-1)
            }
        };

        await context.StockMovements.AddRangeAsync(movements);
        await context.SaveChangesAsync();
    }
}