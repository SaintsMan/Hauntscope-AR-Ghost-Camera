using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Tools;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeViewMode : IViewMode
    {
        private readonly ObservableValue<bool> _isActive = new ObservableValue<bool>();

        public bool IsUnlocked { get; set; } = true;

        public string LabelKey { get; set; } = "hud.view.fake";

        public Material Filter { get; set; }

        public AudioClip OnClip => null;

        public float OnVolume => 1f;

        public IReadOnlyObservableValue<bool> IsActive => _isActive;

        public float DrainPerSecond { get; set; } = 1f;

        public int Ticks { get; private set; }

        public void Activate()
        {
            _isActive.Value = true;
        }

        public void Deactivate()
        {
            _isActive.Value = false;
        }

        public void Tick(float deltaTime)
        {
            Ticks++;
        }
    }
}
