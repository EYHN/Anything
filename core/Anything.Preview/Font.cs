using Anything.Utils;
using SkiaSharp;
using Topten.RichTextKit;

namespace Anything.Preview
{
    public static class Font
    {
        public class TextBlock : Topten.RichTextKit.TextBlock
        {
            public TextBlock()
            {
                CharacterMatcher.Initialize();
            }
        }

        public class CharacterMatcher : ICharacterMatcher
        {
#pragma warning disable IDISP004
            private static readonly SKTypeface[] _typeFaces =
            {
                SKTypeface.FromData(
                    SKData.CreateCopy(Resources.ReadEmbeddedFile(
                        typeof(CharacterMatcher).Assembly,
                        "Resources/Fonts/UbuntuMono-R.ttf"))),
                SKTypeface.FromData(
                    SKData.CreateCopy(Resources.ReadEmbeddedFile(
                        typeof(CharacterMatcher).Assembly,
                        "Resources/Fonts/NotoSansCJKsc-Regular.otf")))
            };
#pragma warning restore IDISP004

            private CharacterMatcher()
            {
            }

            public static CharacterMatcher Instance { get; } = new();

            public SKTypeface? MatchCharacter(
                string familyName,
                int weight,
                int width,
                SKFontStyleSlant slant,
                string[] bcp47,
                int character)
            {
                foreach (var typeFace in _typeFaces)
                {
                    if (typeFace.ContainsGlyph(character))
                    {
                        return typeFace;
                    }
                }

                return null;
            }

            public static void Initialize()
            {
                FontFallback.CharacterMatcher = Instance;
            }
        }
    }
}
