using UnityEngine;

namespace Useful.Extensions
{
    public static class ColorExtensions
    {
        public static Color WithR(this Color c, float r)
        {
            c.r = r;
            return c;
        }

        public static Color WithG(this Color c, float g)
        {
            c.g = g;
            return c;
        }

        public static Color WithB(this Color c, float b)
        {
            c.b = b;
            return c;
        }

        public static Color WithA(this Color c, float alpha)
        {
            c.a = alpha;
            return c;
        }

        public static Color WithRGB(this Color c, Color rgb)
        {
            c.r = rgb.r;
            c.g = rgb.g;
            c.b = rgb.b;
            return c;
        }

        public static Color MultiplyRGB(this Color c, float m) => c.WithRGB(c * m);
        public static Color MultiplyA(this Color c, float m) => c.WithA(c.a * m);

        public static Vector3 ToVector3(this Color c) => new(c.r, c.g, c.b);
        public static Vector4 ToVector4(this Color c) => new(c.r, c.g, c.b, c.a);
    }
}