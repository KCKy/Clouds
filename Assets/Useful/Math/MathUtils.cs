using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Useful.Math
{
    public static class MathUtils
    {
        public const float TAU = Mathf.PI * 2;

        /// <summary>
        /// Returns the remainder of 'x' divided by 'm'. In particular, returns 'x' - 'd'*'m', where 'd' is the largest integer such that 'd'*'m' is less than or equal to 'x'.
        /// The divisor must be positive.
        /// </summary>
        /// <returns>The remainder in the range [0..m-1].</returns>
        public static int Mod(int x, int m) => (int)((x + ((long)m << 32)) % m);

        /// <summary>
        /// Rounds a number away from zero. The result is equal to ceil(x) for positive x and floor(x) for negative x.
        /// </summary>
        public static int RoundAwayFromZero(float x) => System.Math.Sign(x) * Mathf.CeilToInt(Mathf.Abs(x));

        /// <summary>
        /// Makes the smallest possible step from value towards target, such that it is closer to target by at least 1 / divisor.
        /// </summary>
        public static void StepTowards(ref int value, int target, int divisor) => value += RoundAwayFromZero((target - value) / (float)divisor);

        /// <summary>
        /// Maps space [0, 1]^3 uniformly into a space of unit quaternions.
        /// </summary>
        public static Quaternion QuaternionFromVector(Vector3 x)
        {
            return new(Mathf.Sqrt(1 - x.x) * Mathf.Sin(x.y * TAU),
                Mathf.Sqrt(1 - x.x) * Mathf.Cos(x.y * TAU),
                Mathf.Sqrt(x.x) * Mathf.Sin(x.z * TAU),
                Mathf.Sqrt(x.x) * Mathf.Cos(x.z * TAU));
        }

        public static Quaternion PerlinNoiseQuaternion(float x, float seed)
        {
            float u = Mathf.PerlinNoise(x, 0);
            float v = Mathf.PerlinNoise(x, 10);
            float w = Mathf.PerlinNoise(x, 20);
            return QuaternionFromVector(new(u, v, w));
        }
    }
}
