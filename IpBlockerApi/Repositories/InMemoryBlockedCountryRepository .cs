using IpBlockerApi.interfaces;
using IpBlockerApi.Models;
using System.Collections.Concurrent;

namespace IpBlockerApi.Repositories
{
    public class InMemoryBlockedCountryRepository: IBlockedCountryRepository
    {
        private readonly ConcurrentDictionary<string, BlockedCountry> _store = new();

        public bool TryAdd(BlockedCountry country)
        {
            // TryAdd returns false if the key already exists → duplicate prevention
            return _store.TryAdd(country.CountryCode.ToUpper(), country);
        }

        public bool TryRemove(string countryCode)
        {
            return _store.TryRemove(countryCode.ToUpper(), out _);
        }

        public bool Exists(string countryCode)
        {
            return _store.ContainsKey(countryCode.ToUpper());
        }

        public IEnumerable<BlockedCountry> GetAll()
        {
            // Return a snapshot — callers get a consistent view even if the dict changes
            return _store.Values.ToList();
        }
    }
}
