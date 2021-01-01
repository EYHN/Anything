namespace OwnHub.Utils.Color
{
    public static class ColorUtils
    {
        public static int Red(int color) => color >> 16 & byte.MaxValue;

        public static int Green(int color) => color >> 8 & byte.MaxValue;

        public static int Blue(int color) => color & byte.MaxValue;
    }
}