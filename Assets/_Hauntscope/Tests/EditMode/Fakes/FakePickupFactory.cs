using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Pickups;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakePickupFactory : IPickupFactory
    {
        private readonly IRandom _random;

        public FakePickupFactory(IRandom random)
        {
            _random = random;
        }

        public List<FakePickupView> Views { get; } = new List<FakePickupView>();

        public Pickup Create(PickupData data, Vector3 position)
        {
            var view = new FakePickupView();
            Views.Add(view);
            return new Pickup(data, data.CreateEffect(_random), view, position);
        }
    }
}
