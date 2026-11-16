using CleanArchitectureTemplate.Web.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureTemplate.Web.Controllers;

public sealed class ServiceStatusController : Controller
{
    [AllowAnonymous]
    [HttpGet(ServiceStatusResponse.DatabaseUnavailableDiagnosticPath)]
    public async Task<IActionResult> DatabaseUnavailableTest()
    {
        await ServiceStatusResponse.WriteDatabaseUnavailableDiagnosticAsync(
            HttpContext,
            HttpContext.RequestAborted
        );

        return new EmptyResult();
    }

    [AllowAnonymous]
    [HttpGet(AppAvailabilityMiddleware.OutOfServicePath)]
    public IActionResult OutOfService()
    {
        Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        Response.Headers.CacheControl = "no-store, no-cache";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.RetryAfter = ServiceStatusResponse.RetryAfterSeconds;

        return View("~/Views/Shared/OutOfService.cshtml");
    }
}
