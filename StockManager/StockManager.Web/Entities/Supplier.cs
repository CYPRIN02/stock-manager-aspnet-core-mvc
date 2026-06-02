using System.ComponentModel.DataAnnotations;

namespace StockManager.Web.Entities;

public class Supplier
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}