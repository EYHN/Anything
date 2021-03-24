#pragma warning disable IDE0007 // use 'var' instead of explicit type

using System;
using System.Diagnostics.CodeAnalysis;

namespace OwnHub.Utils.Color
{
    public struct HslColor
    {
        /// <summary>
        /// from 0.0 to 360.0
        /// </summary>
        public float H { get; set; }
        
        /// <summary>
        /// from 0.0 to 1.0
        /// </summary>
        public float S { get; set; }
        
        /// <summary>
        /// from 0.0 to 1.0
        /// </summary>
        public float L { get; set; }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public static HslColor FromRgb(int rgbColor)
        {
            float[] outHsl = new float[3];
            float rf = ColorUtils.Red(rgbColor) / 255f;
            float gf = ColorUtils.Green(rgbColor) / 255f;
            float bf = ColorUtils.Blue(rgbColor) / 255f;
            float max = Math.Max(rf, Math.Max(gf, bf));
            float min = Math.Min(rf, Math.Min(gf, bf));
            float deltaMaxMin = max - min;
            float h, s;
            float l = (max + min) / 2f;
            if (max == min)
            {
                // Monochromatic
                h = s = 0f;
            }
            else
            {
                if (max == rf)
                {
                    h = ((gf - bf) / deltaMaxMin) % 6f;
                }
                else if (max == gf)
                {
                    h = ((bf - rf) / deltaMaxMin) + 2f;
                }
                else
                {
                    h = ((rf - gf) / deltaMaxMin) + 4f;
                }
                s = deltaMaxMin / (1f - Math.Abs(2f * l - 1f));
            }
            h = h * 60f % 360f;
            if (h < 0)
            {
                h += 360f;
            }
            outHsl[0] = Constrain(h, 0f, 360f);
            outHsl[1] = Constrain(s, 0f, 1f);
            outHsl[2] = Constrain(l, 0f, 1f);
            return new HslColor()
            {
                H = h,
                S = s,
                L = l
            };
        }
        
        private static float Constrain(float amount, float low, float high)
        {
            return amount < low ? low : (amount > high ? high : amount);
        }
    }
}