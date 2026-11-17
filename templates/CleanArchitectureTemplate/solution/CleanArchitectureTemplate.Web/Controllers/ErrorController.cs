using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureTemplate.Web.Controllers;

public class ErrorController : Controller
{
    private const string SharedErrorViewPath = "~/Views/Shared/Error.cshtml";
    private readonly ILogger<ErrorController> logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        this.logger = logger;
    }

    [Route("Error")]
    [AllowAnonymous]
    public IActionResult UnhandledException()
    {
        var exceptionData = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        ViewBag.ErrorTitle = "Server error";
        ViewBag.Subtitle = "Internal Server Error";
        ViewBag.ErrorMessage = "An error occurred on the server.";
        ViewBag.ErrorDetail = "Please try again later.";
        ViewBag.ErrorId = HttpContext.TraceIdentifier;

        logger.LogError(
            exceptionData?.Error,
            "Unhandled exception while processing {Path}. Error ID: {ErrorId}",
            exceptionData?.Path ?? HttpContext.Request.Path,
            HttpContext.TraceIdentifier
        );

        return View(SharedErrorViewPath);
    }

    [Route("Error/{statusCode}")]
    [AllowAnonymous]
    public IActionResult HttpStatusCodeHandler(int statusCode, string message, string errorId)
    {
        var statusCodeData = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

        ViewBag.OriginalPath = statusCodeData?.OriginalPath;
        ViewBag.OriginalQueryString = statusCodeData?.OriginalQueryString;

        switch (statusCode)
        {
            case 400:
                // EXPLANATION :
                // A 400 status code, also known as "Bad Request", indicates that the server cannot process the
                // request because it is malformed, invalid, or contains invalid parameters.
                ViewBag.ErrorTitle = "Bad request";
                ViewBag.Subtitle = "Bad Request";
                ViewBag.ErrorMessage = "The request is not valid. Check the data and try again.";
                ViewBag.ErrorDetail = message;
                ViewBag.ErrorId = errorId;
                return View(SharedErrorViewPath);
            case 403:
                // EXPLANATION :
                // A 403 status code, also known as "Forbidden", indicates that the server understood the request
                // but refuses to authorize access to the requested resource.
                ViewBag.ErrorTitle = "Access forbidden";
                ViewBag.Subtitle = "Forbidden";
                ViewBag.ErrorMessage = "You do not have permission to access this resource.";
                ViewBag.ErrorDetail = message;
                ViewBag.ErrorId = errorId;
                return View(SharedErrorViewPath);
            case 404:
                // EXPLANATION :
                // A 404 status code, also known as "Not Found", indicates that the requested resource does not
                // exist or is no longer available at the requested URL.
                ViewBag.ErrorTitle = "Not found";
                ViewBag.Subtitle = "Not Found";
                ViewBag.ErrorMessage = "The requested resource was not found.";
                ViewBag.ErrorDetail = message;
                ViewBag.ErrorId = errorId;
                logger.LogWarning(
                    "{StatusCode} Error occurred. Path = {Path} and QueryString = {QueryString}",
                    statusCode,
                    statusCodeData?.OriginalPath ?? HttpContext.Request.Path,
                    statusCodeData?.OriginalQueryString ?? HttpContext.Request.QueryString.Value ?? "no-query-string"
                );
                break;
            case 405:
                // EXPLANATION :
                // A 405 status code, also known as "Method Not Allowed", is an HTTP response code that a server
                // sends when a client requests a method that the resource doesn't support.
                ViewBag.ErrorTitle = "Method not allowed";
                ViewBag.Subtitle = "Method Not Allowed";
                ViewBag.ErrorMessage = "The method used to access this resource is not allowed.";
                ViewBag.ErrorDetail = message;
                ViewBag.ErrorId = errorId;
                logger.LogWarning(
                    "405 Error occurred. Path = {Path} and QueryString = {QueryString}",
                    statusCodeData?.OriginalPath ?? HttpContext.Request.Path,
                    statusCodeData?.OriginalQueryString ?? HttpContext.Request.QueryString.Value ?? "no-query-string"
                );
                break;
            case 500:
                // EXPLANATION :
                // A 500 status code, also known as "Internal Server Error", indicates that an unexpected server
                // error prevented the request from being completed.
                ViewBag.ErrorTitle = "Server error";
                ViewBag.Subtitle = "Internal Server Error";
                ViewBag.ErrorMessage = "An error occurred on the server.";
                ViewBag.ErrorDetail = message;
                ViewBag.ErrorId = errorId;
                return View(SharedErrorViewPath);
            default:
                // EXPLANATION :
                // This branch handles HTTP status codes without a specific presentation defined above.
                ViewBag.ErrorTitle = $"Error: {statusCode}";
                ViewBag.Subtitle = "HTTP Error";
                ViewBag.ErrorMessage = "An error occurred while processing the request.";
                ViewBag.ErrorDetail = message;
                ViewBag.ErrorId = errorId;
                return View(SharedErrorViewPath);
        }

        return View("NotFound");
    }
}
