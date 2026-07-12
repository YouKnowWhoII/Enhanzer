using Enhanzer.Api.DTOs;
using Enhanzer.Api.Models;

namespace Enhanzer.Api.Repositories;

public interface ILocationRepository
{
    Task<IReadOnlyCollection<LocationDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LocationDto>> UpsertAsync(IEnumerable<ExternalLocation> locations, CancellationToken cancellationToken);
}
