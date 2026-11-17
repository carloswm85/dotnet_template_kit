using CleanArchitectureTemplate.Web.Models.ViewModels.WeatherForecast;

namespace CleanArchitectureTemplate.Web.Services.Interfaces
{
    public interface IWeatherForecastService
    {
        Task<List<WeatherForecast>?> GetWeatherForecastExample();
    }
}
