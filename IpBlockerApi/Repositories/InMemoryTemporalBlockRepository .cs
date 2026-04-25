using IpBlockerApi.interfaces;
using IpBlockerApi.Models;
using System.Collections.Concurrent;

namespace IpBlockerApi.Repositories
{
    public class InMemoryTemporalBlockRepository : ITemporalBlockRepository
    {
        private readonly ConcurrentDictionary<string, TemporalBlock> _store = new();

        public bool TryAdd(TemporalBlock block)
        {
            return _store.TryAdd(block.CountryCode.ToUpper(), block);
        }

        public bool TryRemove(string countryCode)
        {
            return _store.TryRemove(countryCode.ToUpper(), out _);
        }

        public bool Exists(string countryCode)
        {
            return _store.ContainsKey(countryCode.ToUpper());
        }

        /// <summary>
        /// A country is "blocked" only if the entry exists AND hasn't expired yet.
        /// </summary>
        public bool IsCountryBlocked(string countryCode)
        {
            if (_store.TryGetValue(countryCode.ToUpper(), out var block))
                return !block.IsExpired;

            return false;
        }

        public IEnumerable<TemporalBlock> GetAll() => _store.Values.ToList();

        /// <summary>Used by the background cleanup service.</summary>
        public IEnumerable<TemporalBlock> GetExpired()
            => _store.Values.Where(b => b.IsExpired).ToList();
    }
}
