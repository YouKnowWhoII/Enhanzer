using Enhanzer.Api.DTOs;
using Enhanzer.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enhanzer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class LocationsController(ILocationRepository locationRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<LocationDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await locationRepository.GetAllAsync(cancellationToken));
    }
}
