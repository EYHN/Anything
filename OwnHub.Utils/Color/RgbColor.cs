namespace OwnHub.Utils.Color
{
    public struct RgbColor
    {
        /// <summary>
        /// from 0 to 255
        /// </summary>
        public int R { get; set; }
        
        /// <summary>
        /// from 0 to 255
        /// </summary>
        public int G { get; set; }
        
        /// <summary>
        /// from 0 to 255
        /// </summary>
        public int B { get; set; }

        public RgbColor(int color)
        {
            R = ColorUtils.Red(color);
            G = ColorUtils.Green(color);
            B = ColorUtils.Blue(color);
        }
    }
}