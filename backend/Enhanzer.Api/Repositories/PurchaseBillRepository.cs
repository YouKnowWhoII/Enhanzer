using Enhanzer.Api.Data;
using Enhanzer.Api.DTOs;
using Enhanzer.Api.Entities;

namespace Enhanzer.Api.Repositories;

public sealed class PurchaseBillRepository(AppDbContext dbContext) : IPurchaseBillRepository
{
    public async Task<PurchaseBillSaveResponse> SaveAsync(PurchaseBillSaveRequest request, string createdByEmail, CancellationToken cancellationToken)
    {
        var items = request.Items.Select(ToEntity).ToList();
        var bill = new PurchaseBill
        {
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByEmail = createdByEmail,
            TotalItems = items.Count,
            TotalQuantity = items.Sum(item => item.Quantity),
            TotalCost = items.Sum(item => item.TotalCost),
            TotalSelling = items.Sum(item => item.TotalSelling),
            Items = items
        };

        dbContext.PurchaseBills.Add(bill);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PurchaseBillSaveResponse
        {
            Id = bill.Id,
            CreatedAtUtc = bill.CreatedAtUtc,
            TotalItems = bill.TotalItems,
            TotalQuantity = bill.TotalQuantity,
            TotalCost = bill.TotalCost,
            TotalSelling = bill.TotalSelling
        };
    }

    private static PurchaseBillItem ToEntity(PurchaseBillItemRequest item)
    {
        // Totals are recalculated server-side so persisted values cannot be tampered with by the client.
        var grossCost = item.StandardCost * item.Quantity;
        var totalCost = grossCost - grossCost * (item.Discount / 100);
        var totalSelling = item.StandardPrice * item.Quantity;

        return new PurchaseBillItem
        {
            Item = item.Item.Trim(),
            Batch = item.Batch.Trim(),
            StandardCost = item.StandardCost,
            StandardPrice = item.StandardPrice,
            Quantity = item.Quantity,
            Discount = item.Discount,
            TotalCost = totalCost,
            TotalSelling = totalSelling
        };
    }
}
