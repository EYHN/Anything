using static MoreLinq.Extensions.MinByExtension;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Topten.RichTextKit;

namespace OwnHub.Preview
{
    /// <summary>
    /// Handles mapping of font family names to SKTypefaces
    /// </summary>
    public class FontMapper : Topten.RichTextKit.FontMapper
    {
        /// <summary>
        /// Loads a private font from a stream
        /// </summary>
        /// <param name="stream">The stream to load from</param>
        /// <param name="familyName">An optional family name to override the font's built in name</param>
        /// <returns>True if the font was successully loaded</returns>
        public static bool LoadPrivateFont(System.IO.Stream stream, string familyName = null)
        {
            var tf = SKTypeface.FromStream(stream);
            if (tf == null)
                return false;

            var qualifiedName = familyName ?? tf.FamilyName;
            if (tf.FontSlant != SKFontStyleSlant.Upright)
            {
                qualifiedName += "-Italic";
            }

            // Get a list of typefaces with this family
            if (!_customFonts.TryGetValue(qualifiedName, out var listFonts))
            {
                listFonts = new List<SKTypeface>();
                _customFonts[qualifiedName] = listFonts;
            }

            // Add to the list
            listFonts.Add(tf);

            return true;
        }

        /// <summary>
        /// Map a RichTextKit style to an SKTypeface
        /// </summary>
        /// <param name="style">The style</param>
        /// <param name="ignoreFontVariants">True to ignore variants (super/subscript)</param>
        /// <returns>The mapped typeface</returns>
        public override SKTypeface TypefaceFromStyle(IStyle style, bool ignoreFontVariants)
        {
            // Work out the qualified name
            var qualifiedName = style.FontFamily;
            if (style.FontItalic)
                qualifiedName += "-Italic";

            // Look up custom fonts
            List<SKTypeface> listFonts;
            if (_customFonts.TryGetValue(qualifiedName, out listFonts))
            {
                // Find closest weight
                return listFonts.MinBy(x => Math.Abs(x.FontWeight - style.FontWeight)).First();
            }

            // Do default mapping
            return base.TypefaceFromStyle(style, ignoreFontVariants);
        }

        static FontMapper()
        {
            // Install self as the default RichTextKit font mapper
            Topten.RichTextKit.FontMapper.Default = new FontMapper();
        }

        // Constructor
        private FontMapper()
        {
        }

        static Dictionary<string, List<SKTypeface>> _customFonts = new Dictionary<string, List<SKTypeface>>();
    }
}
