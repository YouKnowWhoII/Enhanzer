using System.Security.Claims;
using Enhanzer.Api.DTOs;
using Enhanzer.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enhanzer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/purchase-bills")]
public sealed class PurchaseBillsController(IPurchaseBillRepository purchaseBillRepository, ILogger<PurchaseBillsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PurchaseBillSaveResponse>> Save(PurchaseBillSaveRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email") ?? "unknown";
        var response = await purchaseBillRepository.SaveAsync(request, email, cancellationToken);

        logger.LogInformation("Purchase bill {PurchaseBillId} saved by {Email} with {ItemCount} items.", response.Id, email, response.TotalItems);
        return Ok(response);
    }
}
