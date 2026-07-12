using Enhanzer.Api.DTOs;

namespace Enhanzer.Api.Repositories;

public interface IPurchaseBillRepository
{
    Task<PurchaseBillSaveResponse> SaveAsync(PurchaseBillSaveRequest request, string createdByEmail, CancellationToken cancellationToken);
}
