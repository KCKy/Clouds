using System;
using UnityEngine;

namespace Useful.Math
{
    /// <summary>
    /// A procedural cosine palette.
    /// </summary>
    /// <remarks>
    /// Used formula: <code>A+B*cos(tau*(C*t+D))</code>.
    /// Adapted from https://iquilezles.org/articles/palettes
    /// Useful tool: http://dev.thi.ng/gradients
    /// </remarks>
    public readonly struct Palette
    {
        public readonly Vector3 A;
        public readonly Vector3 B;
        public readonly Vector3 C;
        public readonly Vector3 D;

        public Palette(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public static Palette Rainbow => new(Vector3.one * 0.5f, Vector3.one * 0.5f, Vector3.one, new(0, 1f / 3, 2f / 3));

        public static Palette OrangeBlue => new(Vector3.one * 0.5f, Vector3.one * 0.5f, Vector3.one, new(0, 1f / 10, 1f / 5));

        public static Palette CyanPink => new(Vector3.one * 0.5f, Vector3.one * 0.5f, Vector3.one, new(0, 1f / 10, 1f / 5));

        static float Formula(float a, float b, float c, float d, float t)
        {
            return a + b * MathF.Cos(MathUtils.TAU * (c * t + d));
        }

        /// <summary>
        /// Sample the palette.
        /// </summary>
        /// <param name="t">The point to sample.</param>
        /// <returns>A vector of RGB values.</returns>
        public Color this[float t]
        {
            get
            {
                t %= 1;
                float r = Formula(A.x, B.x, C.x, D.x, t);
                float g = Formula(A.y, B.y, C.y, D.y, t);
                float b = Formula(A.z, B.z, C.z, D.z, t);
                return new(r, g, b);
            }
        }
    }
}
