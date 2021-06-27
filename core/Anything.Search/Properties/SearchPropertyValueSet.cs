using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Anything.Search.Properties
{
    public record SearchPropertyValueSet
        : IEnumerable<(SearchProperty Property, object Data)>
    {
        private readonly ImmutableArray<(SearchProperty Property, object Data)> _data;

        public SearchPropertyValueSet(IEnumerable<(SearchProperty Property, object Data)> data)
        {
            _data = data.ToImmutableArray();
        }

        public IEnumerator<(SearchProperty Property, object Data)> GetEnumerator()
        {
            return (_data as IEnumerable<(SearchProperty Property, object Data)>).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static SearchPropertyValueSet Merge(params SearchPropertyValueSet[] sets)
        {
            var list = new List<(SearchProperty Property, object Data)>();

            foreach (var set in sets)
            {
                foreach (var item in set)
                {
                    list.Add(item);
                }
            }

            return new SearchPropertyValueSet(list);
        }
    }
}
