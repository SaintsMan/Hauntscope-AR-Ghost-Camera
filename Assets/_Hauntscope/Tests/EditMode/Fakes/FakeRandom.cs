using System.Collections.Generic;
using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeRandom : IRandom
    {
        private readonly Queue<float> _values = new Queue<float>();

        public float DefaultValue { get; set; } = 0.5f;

        public float Value => Next();

        public void Enqueue(params float[] values)
        {
            foreach (var value in values)
                _values.Enqueue(value);
        }

        public float Range(float minInclusive, float maxInclusive)
        {
            return Mathf.Lerp(minInclusive, maxInclusive, Next());
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            var value = minInclusive + Mathf.FloorToInt((maxExclusive - minInclusive) * Next());
            return Mathf.Clamp(value, minInclusive, maxExclusive - 1);
        }

        private float Next()
        {
            return _values.Count > 0 ? _values.Dequeue() : DefaultValue;
        }
    }
}
