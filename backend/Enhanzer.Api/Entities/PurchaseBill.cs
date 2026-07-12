namespace Enhanzer.Api.Entities;

public sealed class PurchaseBill
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string CreatedByEmail { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalSelling { get; set; }
    public List<PurchaseBillItem> Items { get; set; } = [];
}
