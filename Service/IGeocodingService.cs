using A2.Models;

namespace A2.Service
{
    public interface IGeocodingService
    {
        Task<(decimal Latitude, decimal Longitude)?> ObterCoordenadasAsync(EnderecoCliente endereco);
    }
}
