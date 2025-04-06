using UnityEngine;

namespace Useful.Random
{
    public interface IRandom
    {
        /// <summary>
        /// Get a random float between [0..1].
        /// </summary>
        float NextFloat();

        /// <summary>
        /// Get a random float within given bounds (inclusive).
        /// </summary>
        float NextFloat(float min, float max);

        /// <summary>
        /// Get a random in between [0..n-1].
        /// </summary>
        int NextInt(int maxExclusive);
    }

    public sealed class UnityRandomAdapter : IRandom
    {
        UnityRandomAdapter() { }

        public static UnityRandomAdapter Instance { get; } = new();

        public float NextFloat() => UnityEngine.Random.value;

        public float NextFloat(float min, float max) => UnityEngine.Random.Range(min, max);
        public int NextInt(int maxExclusive) => UnityEngine.Random.Range(0, maxExclusive);
    }

    public sealed class SystemRandomAdapter : IRandom
    {
        readonly System.Random _random;

        public SystemRandomAdapter(System.Random random) => _random = random;

        public float NextFloat() => (float)_random.NextDouble();

        public float NextFloat(float min, float max)
        {
            double result = min + _random.NextDouble() * (max - min);
            return Mathf.Clamp((float)result, min, max);
        }

        public int NextInt(int maxExclusive) => _random.Next(maxExclusive);
    }
}
