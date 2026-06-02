using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace StockManager.Web.Entities;

public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Reference { get; set; }

    public int Quantity { get; set; }

    public int AlertQuantity { get; set; }

    [Precision(18, 2)]
    public decimal UnitPrice { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}