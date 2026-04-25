using IpBlockerApi.Models;

namespace IpBlockerApi.Services
{
    public interface IGeolocationService
    {
        Task<GeoLocationResult?> LookupAsync(string ipAddress);
    }
}
