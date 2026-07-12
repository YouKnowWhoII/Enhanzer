using System.ComponentModel.DataAnnotations;

namespace Enhanzer.Api.DTOs;

public sealed class PurchaseBillSaveRequest
{
    [Required, MinLength(1)]
    public IReadOnlyCollection<PurchaseBillItemRequest> Items { get; init; } = [];
}
