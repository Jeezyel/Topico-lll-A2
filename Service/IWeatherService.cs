using A2.Models;

namespace A2.Service
{
    public interface IWeatherService
    {
        Task<AlertaClimatico?> VerificarClimaAsync(decimal latitude, decimal longitude);
    }
}
