using Hauntscope.Core.Services;

namespace Hauntscope.Infrastructure.Random
{
    public sealed class UnityRandom : IRandom
    {
        public float Value => UnityEngine.Random.value;

        public float Range(float minInclusive, float maxInclusive)
        {
            return UnityEngine.Random.Range(minInclusive, maxInclusive);
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            return UnityEngine.Random.Range(minInclusive, maxExclusive);
        }
    }
}
