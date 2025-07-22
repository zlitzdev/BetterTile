using UnityEngine;

namespace Zlitz.Extra2D.BetterTile
{
    internal static class ColorExtensions
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
