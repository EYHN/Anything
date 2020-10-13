using SkiaSharp;
using Topten.RichTextKit;

namespace OwnHub.Preview
{
    public static class Font
    {
        static Font()
        {
            CharacterMatcher.Initialize();
        }

        public class TextBlock : Topten.RichTextKit.TextBlock
        {

        }


        public class CharacterMatcher : ICharacterMatcher
        {
            SKTypeface[] TypeFaces;

            static CharacterMatcher()
            {
                SKTypeface[] TypeFaces = new SKTypeface[]
                {
                    SKTypeface.FromStream(Utils.Utils.ReadEmbeddedFile("Resources/Fonts/UbuntuMono-R.ttf")),
                    SKTypeface.FromStream(Utils.Utils.ReadEmbeddedFile("Resources/Fonts/NotoSansCJKsc-Regular.otf"))
                };
                FontFallback.CharacterMatcher = new CharacterMatcher(TypeFaces);
            }

            public static void Initialize()
            {
            }

            public CharacterMatcher(SKTypeface[] Typefaces)
            {
                this.TypeFaces = Typefaces;
            }

            public SKTypeface MatchCharacter(string familyName, int weight, int width, SKFontStyleSlant slant, string[] bcp47, int character)
            {
                foreach (SKTypeface TypeFace in this.TypeFaces)
                {
                    if (TypeFace.ContainsGlyph(character))
                    {
                        return TypeFace;
                    }
                }
                return null;
            }
        }
    }
    
}
