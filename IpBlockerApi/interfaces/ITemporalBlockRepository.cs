using IpBlockerApi.Models;

namespace IpBlockerApi.interfaces
{
    public interface ITemporalBlockRepository
    {
        bool TryAdd(TemporalBlock block);
        bool TryRemove(string countryCode);
        bool Exists(string countryCode);
        bool IsCountryBlocked(string countryCode);   // checks existence AND not expired
        IEnumerable<TemporalBlock> GetAll();
        IEnumerable<TemporalBlock> GetExpired();
    }
}
