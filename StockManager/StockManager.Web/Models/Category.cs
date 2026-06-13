using System.ComponentModel.DataAnnotations;

namespace StockManager.Web.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}