using StockManager.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace StockManager.Web.Models;

public class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Required]
    public StockMovementType Type { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [StringLength(250)]
    public string? Reason { get; set; }

    public DateTime MovementDate { get; set; } = DateTime.Now;
}