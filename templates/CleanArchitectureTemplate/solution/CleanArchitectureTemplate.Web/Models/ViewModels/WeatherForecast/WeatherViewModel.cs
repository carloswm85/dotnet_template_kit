using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureTemplate.Web.Models.ViewModels.WeatherForecast;

public class WeatherViewModel
{
    public List<WeatherForecast>? WeatherList { get; set; } = [];
}

public class WeatherForecast
{
    [Display(Name = "SAMPLE_DATE")]
    public DateTime Date { get; set; }

    [Display(Name = "SAMPLE_TEMPERATURE_C")]
    public int TemperatureC { get; set; }

    [Display(Name = "SAMPLE_TEMPERATURE_F")]
    public int TemperatureF { get; set; }

    [Display(Name = "SAMPLE_SUMMARY")]
    public string? Summary { get; set; }
}
