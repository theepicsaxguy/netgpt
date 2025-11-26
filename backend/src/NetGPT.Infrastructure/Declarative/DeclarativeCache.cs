using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NetGPT.Infrastructure.Declarative
{
    public sealed class DeclarativeCache
    {
        private readonly ConcurrentDictionary<string, (object Value, DateTime Expiry)> cache = new();
        private readonly TimeSpan defaultTtl = TimeSpan.FromMinutes(5);

        public void Set(string key, object value, TimeSpan? ttl = null)
        {
            DateTime expiry = DateTime.UtcNow + (ttl ?? defaultTtl);
            cache.AddOrUpdate(key, (value, expiry), (k, v) => (value, expiry));
        }

        public bool TryGet<T>(string key, out T? value)
        {
            value = default;
            if (cache.TryGetValue(key, out var entry))
            {
                if (entry.Expiry > DateTime.UtcNow)
                {
                    if (entry.Value is T t)
                    {
                        value = t;
                        return true;
                    }
                }
                else
                {
                    // expired
                    cache.TryRemove(key, out _);
                }
            }

            return false;
        }

        public void EvictByPrefix(string prefix)
        {
            foreach (var k in cache.Keys)
            {
                if (k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    cache.TryRemove(k, out _);
                }
            }
        }
    }
}
