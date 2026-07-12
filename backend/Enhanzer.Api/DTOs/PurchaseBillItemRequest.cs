using System.ComponentModel.DataAnnotations;

namespace Enhanzer.Api.DTOs;

public sealed class PurchaseBillItemRequest
{
    [Required]
    public string Item { get; init; } = string.Empty;

    [Required]
    public string Batch { get; init; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal StandardCost { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal StandardPrice { get; init; }

    [Range(1, double.MaxValue)]
    public decimal Quantity { get; init; }

    [Range(0, 100)]
    public decimal Discount { get; init; }

    [Range(0, double.MaxValue)]
    public decimal TotalCost { get; init; }

    [Range(0, double.MaxValue)]
    public decimal TotalSelling { get; init; }
}
