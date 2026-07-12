namespace Enhanzer.Api.DTOs;

public sealed class PurchaseBillSaveResponse
{
    public int Id { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public int TotalItems { get; init; }
    public decimal TotalQuantity { get; init; }
    public decimal TotalCost { get; init; }
    public decimal TotalSelling { get; init; }
}
