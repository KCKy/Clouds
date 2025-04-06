using UnityEngine;
using Useful.Math;
using Useful.Random;

namespace Useful.Extensions
{
    public static class RandomExtensions
    {
        public static Quaternion RandomQuaternion(this IRandom random)
        {
            float u = random.NextFloat();
            float v = random.NextFloat();
            float w = random.NextFloat();
            return MathUtils.QuaternionFromVector(new(u, v, w));
        }
    }
}
