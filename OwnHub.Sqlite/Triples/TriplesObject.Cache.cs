using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OwnHub.Sqlite.Triples
{
    public abstract partial class TriplesObject
    {
        private Dictionary<string, object> _cache = new();

        /// <summary>
        /// Gets a value indicating whether the internal method is actually do cache or not, depending on
        /// whether the object is currently in the pending state.
        /// </summary>
        protected bool DoCache => Status == ObjectStatus.Pending;

        private void SetPropertyCache(string name, object value, Transaction transaction)
        {
            if (!DoCache)
            {
                throw new InvalidOperationException("Cache is disabled");
            }

            _cache.TryGetValue(name, out var oldValue);
            transaction.RunSideEffect(
                () =>
                {
                    _cache[name] = value;
                },
                () =>
                {
                    if (oldValue != null)
                    {
                        _cache[name] = oldValue;
                    }
                    else
                    {
                        _cache.Remove(name);
                    }
                });
        }

        private bool TryGetPropertyCache(string name, [MaybeNullWhen(false)] out object obj)
        {
            if (!DoCache)
            {
                throw new InvalidOperationException("Cache is disabled");
            }

            return _cache.TryGetValue(name, out obj);
        }

        private void DeletePropertyCache(string name, Transaction transaction)
        {
            if (!DoCache)
            {
                throw new InvalidOperationException("Cache is disabled");
            }

            _cache.TryGetValue(name, out var oldValue);
            transaction.RunSideEffect(
                () =>
                {
                    _cache.Remove(name);
                },
                () =>
                {
                    if (oldValue != null)
                    {
                        _cache[name] = oldValue;
                    }
                });
        }

        private IEnumerable<KeyValuePair<string, object>> GetAllPropertiesCache()
        {
            if (!DoCache)
            {
                throw new InvalidOperationException("Cache is disabled");
            }

            return new Dictionary<string, object>(_cache);
        }

        private void DeleteAllPropertiesCache(Transaction transaction)
        {
            if (!DoCache)
            {
                throw new InvalidOperationException("Cache is disabled");
            }

            var oldCache = _cache;
            transaction.RunSideEffect(
                () =>
                {
                    _cache = new();
                },
                () =>
                {
                    _cache = oldCache;
                });
        }

        private void SetAllPropertiesCache(IEnumerable<KeyValuePair<string, object>> cache, Transaction transaction)
        {
            if (!DoCache)
            {
                throw new InvalidOperationException("Cache is disabled");
            }

            var oldCache = _cache;
            transaction.RunSideEffect(
                () =>
                {
                    _cache = new Dictionary<string, object>(cache);
                },
                () =>
                {
                    _cache = oldCache;
                });
        }
    }
}
