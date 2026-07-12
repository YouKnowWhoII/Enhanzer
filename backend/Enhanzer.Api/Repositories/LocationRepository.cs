using Enhanzer.Api.Data;
using Enhanzer.Api.DTOs;
using Enhanzer.Api.Entities;
using Enhanzer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.Api.Repositories;

public sealed class LocationRepository(AppDbContext dbContext) : ILocationRepository
{
    public async Task<IReadOnlyCollection<LocationDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.LocationDetails
            .AsNoTracking()
            .OrderBy(location => location.LocationName)
            .Select(location => new LocationDto
            {
                Id = location.Id,
                LocationCode = location.LocationCode,
                LocationName = location.LocationName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<LocationDto>> UpsertAsync(IEnumerable<ExternalLocation> locations, CancellationToken cancellationToken)
    {
        var incomingLocations = locations
            .Where(location => !string.IsNullOrWhiteSpace(location.LocationCode) && !string.IsNullOrWhiteSpace(location.LocationName))
            .GroupBy(location => location.LocationCode.Trim())
            .Select(group => group.First())
            .ToList();

        var codes = incomingLocations.Select(location => location.LocationCode.Trim()).ToList();
        var existingLocations = await dbContext.LocationDetails
            .Where(location => codes.Contains(location.LocationCode))
            .ToDictionaryAsync(location => location.LocationCode, cancellationToken);

        foreach (var incomingLocation in incomingLocations)
        {
            var code = incomingLocation.LocationCode.Trim();
            var name = incomingLocation.LocationName.Trim();

            if (existingLocations.TryGetValue(code, out var existingLocation))
            {
                existingLocation.LocationName = name;
                continue;
            }

            dbContext.LocationDetails.Add(new LocationDetail
            {
                LocationCode = code,
                LocationName = name
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetAllAsync(cancellationToken);
    }
}
