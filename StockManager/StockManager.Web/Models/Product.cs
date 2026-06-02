using StockManager.Web.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockManager.Web.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Reference { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public int AlertThreshold { get; set; } = 10;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}