using Asp.Versioning;
using CleanArchitectureTemplate.API.ApiModels;
using CleanArchitectureTemplate.API.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureTemplate.API.Controllers.Example;

/// <summary>
/// Provides sample weather forecast data.
/// </summary>
/// <remarks>
/// This controller demonstrates a basic public endpoint returning
/// randomly generated weather forecast information.
///
/// Authorization:
/// - Allows anonymous access.
///
/// CORS:
/// - Uses the <see cref="PolicyNames.AllowSpecificOrigin"/> policy.
/// </remarks>
[ApiController]
[AllowAnonymous]
[ApiVersionNeutral]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableCors(PolicyNames.AllowSpecificOrigin)]
public class WeatherForecastsController : ControllerBase
{
    /// <summary>
    /// Predefined weather condition summaries.
    /// </summary>
    private static readonly string[] Summaries = new[]
    {
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching",
    };

    private readonly ILogger<WeatherForecastsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherForecastsController"/> class.
    /// </summary>
    /// <param name="logger">
    /// The logger used for diagnostic and operational logging.
    /// </param>
    public WeatherForecastsController(ILogger<WeatherForecastsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a collection of weather forecast data.
    /// </summary>
    /// <remarks>
    /// Generates five random weather forecast entries starting from tomorrow.
    ///
    /// Route:
    /// GET /api/v1/WeatherForecasts/weatherList
    /// </remarks>
    /// <returns>
    /// A collection of <see cref="WeatherForecast"/> objects
    /// containing date, temperature, and summary information.
    /// </returns>
    /// <response code="200">
    /// Returns the generated weather forecast data.
    /// </response>
    [HttpGet("weatherList", Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        var forecast = Enumerable
            .Range(1, 5)
            .Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)],
            })
            .ToArray();

        return forecast;
    }

    [HttpGet("weatherItem", Name = "GetWeatherForecastOne")]
    public WeatherForecast? GetOne()
    {
        var forecast = Enumerable
            .Range(1, 5)
            .Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)],
            })
            .ToArray()
            .FirstOrDefault();

        return forecast;
    }

    [HttpGet("weatherNull", Name = "GetWeatherForecastNone")]
    public WeatherForecast? GetNone()
    {
        return null;
    }
}
