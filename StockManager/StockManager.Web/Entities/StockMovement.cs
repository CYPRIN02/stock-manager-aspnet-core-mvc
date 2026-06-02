using System.ComponentModel.DataAnnotations;

namespace StockManager.Web.Entities;

public class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public StockMovementType Type { get; set; }

    public int Quantity { get; set; }

    [MaxLength(255)]
    public string? Reason { get; set; }

    public DateTime MovementDate { get; set; } = DateTime.Now;
}