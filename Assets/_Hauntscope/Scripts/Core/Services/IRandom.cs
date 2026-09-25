namespace Hauntscope.Core.Services
{
    public interface IRandom
    {
        float Value { get; }

        float Range(float minInclusive, float maxInclusive);

        int Range(int minInclusive, int maxExclusive);
    }
}
