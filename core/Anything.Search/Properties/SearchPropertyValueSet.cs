using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Anything.Search.Properties;

public record SearchPropertyValueSet
    : IEnumerable<SearchPropertyValue>
{
    private readonly ImmutableArray<SearchPropertyValue> _data;

    public SearchPropertyValueSet(IEnumerable<SearchPropertyValue> data)
    {
        _data = data.ToImmutableArray();
    }

    public IEnumerator<SearchPropertyValue> GetEnumerator()
    {
        return (_data as IEnumerable<SearchPropertyValue>).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public virtual bool Equals(SearchPropertyValueSet? other)
    {
        if (other == null)
        {
            return false;
        }

        return !other.Except(this).Any();
    }

    public override int GetHashCode()
    {
        return _data.GetHashCode();
    }

    public static SearchPropertyValueSet Merge(params SearchPropertyValueSet[] sets)
    {
        var list = new List<SearchPropertyValue>();

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
