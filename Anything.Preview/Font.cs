using System;
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
            private CharacterMatcher()
            {
            }

            public SKTypeface? MatchCharacter(
                string familyName,
                int weight,
                int width,
                SKFontStyleSlant slant,
                string[] bcp47,
                int character)
            {
                foreach (SKTypeface typeFace in _typeFaces)
                {
                    if (typeFace.ContainsGlyph(character))
                    {
                        return typeFace;
                    }
                }

                return null;
            }

            private static SKTypeface[] _typeFaces =
            {
                SKTypeface.FromStream(
                    Resources.ReadEmbeddedFile(typeof(CharacterMatcher).Assembly, "Resources/Fonts/UbuntuMono-R.ttf")),
                SKTypeface.FromStream(
                    Resources.ReadEmbeddedFile(typeof(CharacterMatcher).Assembly, "Resources/Fonts/NotoSansCJKsc-Regular.otf"))
            };

            public static CharacterMatcher Instance { get; } = new();

            public static void Initialize()
            {
                FontFallback.CharacterMatcher = Instance;
            }
        }
    }
}
