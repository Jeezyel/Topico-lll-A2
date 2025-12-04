using A2.DTO;
using A2.Models;

namespace A2.Service
{
    public interface IWeatherService
    {
        Task<WeatherForecastDto?> VerificarClimaAsync(decimal latitude, decimal longitude);
    }
}
