using System;
using System.Text.RegularExpressions;

namespace Anything.Database.Triples
{
    internal enum TriplesTypeCategory
    {
        Value = 'V',
        Object = 'O',
    }

    internal readonly struct TriplesTypeDesc
    {
        public readonly TriplesTypeCategory Category;
        public readonly string? Name;

        public TriplesTypeDesc(char category, string? name = null)
        {
            Category = (TriplesTypeCategory)category;
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
            Regex regex = new(@"(\w)(\(([\w\d]+)\))?$");
            var match = regex.Match(typeDescText);

            if (!match.Success)
            {
                throw new InvalidOperationException("Can't parse type description: " + typeDescText);
            }

            return new TriplesTypeDesc(match.Groups[1].Value[0], match.Groups[3].Value);
        }

        public string ToTypeDescText()
        {
            return Name != null ? $"{(char)Category}({Name})" : Category.ToString();
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
