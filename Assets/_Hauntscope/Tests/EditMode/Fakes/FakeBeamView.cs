using Hauntscope.Gameplay.Tools;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeBeamView : IBeamView
    {
        public bool IsVisible { get; private set; }

        public int DrawCount { get; private set; }

        public Vector3 Origin { get; private set; }

        public Vector3 Target { get; private set; }

        public float Intensity { get; private set; }

        public bool IsLocked { get; private set; }

        public Color IdleColor { get; private set; }

        public Color LockedColor { get; private set; }

        public void SetVisible(bool visible)
        {
            IsVisible = visible;
        }

        public void SetColors(Color idle, Color locked)
        {
            IdleColor = idle;
            LockedColor = locked;
        }

        public void SetBeam(Vector3 origin, Vector3 target, float intensity, bool locked)
        {
            DrawCount++;
            Origin = origin;
            Target = target;
            Intensity = intensity;
            IsLocked = locked;
        }
    }
}
