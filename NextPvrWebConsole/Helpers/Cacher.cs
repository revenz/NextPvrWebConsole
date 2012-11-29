using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace NextPvrWebConsole.Helpers
{
    public class Cacher
    {
        class CachedObject
        {
            public DateTime CreatedUtc { get; set; }
            public TimeSpan Expires { get; set; }
            public object Value { get; set; }

            public bool IsExpired
            {
                get
                {
                    return DateTime.UtcNow > CreatedUtc.Add(Expires);
                }
            }
        }

        private static Dictionary<string, CachedObject> _Cache = new Dictionary<string, CachedObject>();

        public static void FlushCache(Regex KeyMatch)
        {
            var keys = _Cache.Where(x => KeyMatch.IsMatch(x.Key)).Select(x => x.Key).ToArray();
            foreach (var k in keys)
                _Cache.Remove(k);
        }

        public static void Store(string Key, object Item, TimeSpan? Expires = null)
        {
            if (Expires == null)
                Expires = new TimeSpan(0, 5, 0);
            lock (_Cache)
            {
                if (_Cache.ContainsKey(Key))
                {
                    // expire it
                    _Cache.Remove(Key);
                }
                _Cache.Add(Key, new CachedObject() { Expires = Expires.Value, CreatedUtc = DateTime.UtcNow, Value = Item });
            }
        }

        public static T Retrieve<T>(string Key, T DefaultValue)
        {
            lock (_Cache)
            {
                if (!_Cache.ContainsKey(Key))
                    return DefaultValue;

                var cached = _Cache[Key];
                if (cached.IsExpired)
                {
                    _Cache.Remove(Key);
                    return DefaultValue;
                }

                return (T)_Cache[Key].Value;
            }
        }

        public static T RetrieveOrStore<T>(string Key, TimeSpan Expires, Func<T> RetrieveFunction)
        {
            lock (_Cache)
            {
                if (_Cache.ContainsKey(Key) && !_Cache[Key].IsExpired)
                    return (T)_Cache[Key].Value;

                if (_Cache.ContainsKey(Key))
                    _Cache.Remove(Key);

                T value = RetrieveFunction();
                _Cache.Add(Key, new CachedObject() { Value = value, CreatedUtc = DateTime.UtcNow, Expires = Expires });
                return value;
            }
        }
    }
}