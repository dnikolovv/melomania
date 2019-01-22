using System;

namespace Melomania.Utils
{
    public static class Extensions
    {
        public static int RoundToNearestTen(this double number) =>
            ((int)Math.Round(number / 10.0)) * 10;
    }
}
