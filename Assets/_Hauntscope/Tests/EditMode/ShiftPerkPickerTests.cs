using System.Collections.Generic;
using Hauntscope.Gameplay.Shift;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ShiftPerkPickerTests
    {
        private readonly List<ShiftPerkData> _pool = new List<ShiftPerkData>();

        [SetUp]
        public void SetUp()
        {
            for (var i = 0; i < 5; i++)
                _pool.Add(ScriptableObject.CreateInstance<ChargePerkData>());
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var perk in _pool)
                Object.DestroyImmediate(perk);
            _pool.Clear();
        }

        [Test]
        public void Pick_LargerPool_DealsTheRequestedNumberAllDifferent()
        {
            var random = new FakeRandom();
            random.Enqueue(0f, 0f, 0f);
            var result = new List<ShiftPerkData>();

            new ShiftPerkPicker(random).Pick(_pool, 3, result);

            Assert.AreEqual(3, result.Count);
            CollectionAssert.AllItemsAreUnique(result);
        }

        [Test]
        public void Pick_SmallerPool_DealsTheWholePool()
        {
            var result = new List<ShiftPerkData>();

            new ShiftPerkPicker(new FakeRandom()).Pick(_pool.GetRange(0, 2), 3, result);

            CollectionAssert.AreEquivalent(_pool.GetRange(0, 2), result);
        }

        [Test]
        public void Pick_Again_ReplacesThePreviousOffer()
        {
            var picker = new ShiftPerkPicker(new FakeRandom());
            var result = new List<ShiftPerkData>();
            picker.Pick(_pool, 3, result);

            picker.Pick(_pool, 2, result);

            Assert.AreEqual(2, result.Count);
        }
    }
}
