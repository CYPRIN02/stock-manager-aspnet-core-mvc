using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockManager.Web.Models;
namespace StockManager.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager);
        await CreateUserAsync(
        userManager,
        "manager@amadagoit.com",
        "Manager123!",
        "Manager");

        await CreateUserAsync(
            userManager,
            "employee@amadagoit.com",
            "Employee123!",
            "Employee");

        await SeedDataAsync(context);
    }

    private static async Task CreateUserAsync(
    UserManager<IdentityUser> userManager,
    string email,
    string password,
    string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
        else if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "Manager", "Employee" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminAsync(UserManager<IdentityUser> userManager)
    {
        const string adminEmail = "admin@amadagoit.com";
        const string adminPassword = "Admin123!";

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
        else if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }

    private static async Task SeedDataAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync() ||
            await context.Suppliers.AnyAsync() ||
            await context.Products.AnyAsync() ||
            await context.StockMovements.AnyAsync())
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
                AlertQuantity = 5,
                CategoryId = categories[0].Id,
                SupplierId = suppliers[0].Id
            },
            new()
            {
                Name = "Clavier Logitech K120",
                Reference = "PROD-002",
                Quantity = 4,
                UnitPrice = 19.99m,
                AlertQuantity = 10,
                CategoryId = categories[3].Id,
                SupplierId = suppliers[2].Id
            },
            new()
            {
                Name = "Souris Logitech M185",
                Reference = "PROD-003",
                Quantity = 25,
                UnitPrice = 14.99m,
                AlertQuantity = 8,
                CategoryId = categories[3].Id,
                SupplierId = suppliers[2].Id
            },
            new()
            {
                Name = "Écran HP 24 pouces",
                Reference = "PROD-004",
                Quantity = 8,
                UnitPrice = 159.99m,
                AlertQuantity = 5,
                CategoryId = categories[0].Id,
                SupplierId = suppliers[1].Id
            },
            new()
            {
                Name = "Switch Cisco 24 ports",
                Reference = "PROD-005",
                Quantity = 2,
                UnitPrice = 349.99m,
                AlertQuantity = 3,
                CategoryId = categories[2].Id,
                SupplierId = suppliers[3].Id
            },
            new()
            {
                Name = "Disque SSD Samsung 1To",
                Reference = "PROD-006",
                Quantity = 12,
                UnitPrice = 89.99m,
                AlertQuantity = 5,
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