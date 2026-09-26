using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Gameplay.Pickups
{
    public sealed class PickupFactory : IPickupFactory
    {
        private readonly IRandom _random;

        public PickupFactory(IRandom random)
        {
            _random = random;
        }

        public Pickup Create(PickupData data, Vector3 position)
        {
            var view = Object.Instantiate(data.Prefab, position, Quaternion.identity);
            view.name = data.Id;
            view.SetColor(data.Color);
            return new Pickup(data, data.CreateEffect(_random), view, position);
        }
    }
}
