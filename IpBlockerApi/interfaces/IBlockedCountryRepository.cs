using IpBlockerApi.Models;

namespace IpBlockerApi.interfaces
{
    public interface IBlockedCountryRepository
    {
        bool TryAdd(BlockedCountry country);
        bool TryRemove(string countryCode);
        bool Exists(string countryCode);
        IEnumerable<BlockedCountry> GetAll();
    }
}
