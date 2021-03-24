using System;
using System.Text.RegularExpressions;

namespace OwnHub.Sqlite.Triples
{
    internal enum TriplesTypeCategory
    {
        Value = 1,
        Object = 2,
    }

    internal readonly struct TriplesTypeDesc
    {
        public readonly TriplesTypeCategory Category;
        public readonly string? Name;

        public TriplesTypeDesc(string category, string? name = null)
        {
            Category = Enum.Parse<TriplesTypeCategory>(category);
            Name = name;
        }

        public TriplesTypeDesc(TriplesTypeCategory category, string? name = null)
        {
            Category = category;
            Name = name;
        }

        public bool IsObject => Category != TriplesTypeCategory.Value;

        public bool IsValue => Category == TriplesTypeCategory.Value;

        public static TriplesTypeDesc ParseTypeDescText(string typeDescText)
        {
            Regex regex = new(@"(\w+)(\(([\w\d]+)\))?$");
            var match = regex.Match(typeDescText);

            if (!match.Success)
            {
                throw new InvalidOperationException("Can't parse type description: " + typeDescText);
            }

            return new TriplesTypeDesc(match.Groups[1].Value, match.Groups[3].Value);
        }

        public string ToTypeDescText()
        {
            return Name != null ? $"{Category.ToString()}({Name})" : Category.ToString();
        }

        public override bool Equals(object? ob)
        {
            if (ob is TriplesTypeDesc c)
            {
                return Category == c.Category && Name == c.Name;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Category.GetHashCode() ^ (Name != null ? Name.GetHashCode() : 0);
        }
    }
}
