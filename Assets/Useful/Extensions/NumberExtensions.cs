using System;

namespace Useful.Extensions
{
    public static class NumberExtensions
    {
        // TODO: make this for other options
        public static void DecreaseClamped(ref this float value, float amount) => value = MathF.Max(0, value - amount);
        public static void DecreaseClamped(ref this int value, int amount) => value = System.Math.Max(0, value - amount);
    }
}
