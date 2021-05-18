using System;
using System.Collections;
using System.Collections.Generic;

namespace StagingBox.Database.Orm
{
    public class OrmSnapshot : IEnumerable<KeyValuePair<OrmPropertyInfo, object?>>
    {
        private Dictionary<OrmPropertyInfo, object?> _data;

        public object? this[OrmPropertyInfo key] => _data[key];

        public int Count => _data.Count;

        public static IEnumerable<DiffResult> Diff(OrmSnapshot beforeSnapshot, OrmSnapshot afterSnapshot)
        {
            var remainingAfter = new Dictionary<OrmPropertyInfo, object?>(afterSnapshot);

            var changed = new List<DiffResult>();

            foreach (var pair in beforeSnapshot)
            {
                var beforeValue = pair.Value;
                if (afterSnapshot.TryGetValue(pair.Key, out var afterValue))
                {
                    if (beforeValue == afterValue)
                    {
                        // no change
                        remainingAfter.Remove(pair.Key);
                    }
                    else
                    {
                        // property changed
                        changed.Add(new DiffResult { PropertyInfo = pair.Key, Old = beforeValue, New = afterValue });
                        remainingAfter.Remove(pair.Key);
                    }
                }
                else
                {
                    // property removed
                    changed.Add(new DiffResult { PropertyInfo = pair.Key, Old = beforeValue, New = null });
                }
            }

            foreach (var pair in remainingAfter)
            {
                changed.Add(new DiffResult { PropertyInfo = pair.Key, Old = null, New = pair.Value });
            }

            return changed;
        }

        public OrmSnapshot(Dictionary<OrmPropertyInfo, object?>? data = null)
        {
            _data = data ?? new();
        }

        public object? GetValueOrDefault(OrmPropertyInfo propertyInfo)
        {
            return _data.GetValueOrDefault(propertyInfo);
        }

        public bool TryGetValue(OrmPropertyInfo propertyInfo, out object? value)
        {
            return _data.TryGetValue(propertyInfo, out value);
        }

        public void Add(OrmPropertyInfo propertyInfo, object? value)
        {
            if (!propertyInfo.IsAssignableFrom(value))
            {
                throw new ArgumentException($"Value type error. Expect {propertyInfo.ValueTypeInfo.Name} but get {value?.GetType().Name ?? "NULL"}");
            }

            _data.Add(propertyInfo, value);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool ContainsKey(OrmPropertyInfo key)
        {
            return _data.ContainsKey(key);
        }

        public bool Remove(OrmPropertyInfo key)
        {
            return _data.Remove(key);
        }

        public IEnumerator<KeyValuePair<OrmPropertyInfo, object?>> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public record DiffResult
        {
            public OrmPropertyInfo PropertyInfo { get; init; }

            public object? Old { get; init; }

            public object? New { get; init; }
        }
    }
}
