using System.ComponentModel.DataAnnotations;

namespace StockManager.Web.Models;

public class Supplier
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Email { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}