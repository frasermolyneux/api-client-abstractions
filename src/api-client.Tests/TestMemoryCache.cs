using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace MxIO.ApiClient
{
    /// <summary>
    /// A simple test implementation of IMemoryCache to use in unit tests.
    /// </summary>
    public class TestMemoryCache : IMemoryCache
    {
        private readonly Dictionary<object, object> _cache = new Dictionary<object, object>();

        public ICacheEntry CreateEntry(object key)
        {
            return new TestCacheEntry(key, this);
        }

        public void Dispose()
        {
            _cache.Clear();
        }

        public void Remove(object key)
        {
            _cache.Remove(key);
        }

        public bool TryGetValue(object key, out object value)
        {
            if (_cache.TryGetValue(key, out value))
            {
                return true;
            }

            value = default;
            return false;
        }

        public void Set(object key, object value)
        {
            _cache[key] = value;
        }
    }

    public class TestCacheEntry : ICacheEntry
    {
        private readonly object _key;
        private readonly TestMemoryCache _cache;

        public TestCacheEntry(object key, TestMemoryCache cache)
        {
            _key = key;
            _cache = cache;
            Value = null;
        }

        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        public IList<IChangeToken> ExpirationTokens { get; } = new List<IChangeToken>();
        public object Key => _key;
        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = new List<PostEvictionCallbackRegistration>();
        public CacheItemPriority Priority { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public object Value { get; set; }
        public long? Size { get; set; }

        public void Dispose()
        {
            if (Value != null)
            {
                _cache.Set(_key, Value);
            }
        }
    }
}