using System;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Tools;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Onboarding
{
    // The field tools open by their own rules (GDD 5.33); this tells the first-minutes funnel when each one was issued and
    // when the agent first switched it on, so a tool nobody ever uses shows up as a gap between the two.
    public sealed class ToolFeatureReporter : IStartable, IDisposable
    {
        private readonly FeatureUnlocks _features;
        private readonly PlayerProgress _progress;
        private readonly SpiritBox _spiritBox;
        private readonly NightVision _night;
        private readonly ThermalVision _thermal;
        private readonly UvFlashlight _uv;
        private readonly EvpRecorder _evp;
        private readonly ViewSelector _views;

        public ToolFeatureReporter(FeatureUnlocks features, PlayerProgress progress, SpiritBox spiritBox, NightVision night,
            ThermalVision thermal, UvFlashlight uv, EvpRecorder evp, ViewSelector views)
        {
            _features = features;
            _progress = progress;
            _spiritBox = spiritBox;
            _night = night;
            _thermal = thermal;
            _uv = uv;
            _evp = evp;
            _views = views;
        }

        public void Start()
        {
            _progress.Changed += ReportIssued;
            _spiritBox.IsActive.Changed += OnSpiritBoxChanged;
            _views.Current.Changed += OnViewModeChanged;
            _evp.Phase.Changed += OnEvpPhaseChanged;
            ReportIssued();
        }

        public void Dispose()
        {
            _progress.Changed -= ReportIssued;
            _spiritBox.IsActive.Changed -= OnSpiritBoxChanged;
            _views.Current.Changed -= OnViewModeChanged;
            _evp.Phase.Changed -= OnEvpPhaseChanged;
        }

        private void ReportIssued()
        {
            if (_spiritBox.IsUnlocked)
                _features.Unlock(Feature.SpiritBox);
            if (_night.IsUnlocked)
                _features.Unlock(Feature.NightVision);
            if (_thermal.IsUnlocked)
                _features.Unlock(Feature.Thermal);
            if (_uv.IsUnlocked)
                _features.Unlock(Feature.Uv);
            if (_evp.IsUnlocked)
                _features.Unlock(Feature.Evp);
        }

        private void OnSpiritBoxChanged(bool active)
        {
            if (active)
                _features.NotifyUsed(Feature.SpiritBox);
        }

        private void OnViewModeChanged(IViewMode mode)
        {
            if (mode == null)
                return;

            if (ReferenceEquals(mode, _night))
                _features.NotifyUsed(Feature.NightVision);
            else if (ReferenceEquals(mode, _thermal))
                _features.NotifyUsed(Feature.Thermal);
            else if (ReferenceEquals(mode, _uv))
                _features.NotifyUsed(Feature.Uv);
        }

        private void OnEvpPhaseChanged(EvpPhase phase)
        {
            if (phase == EvpPhase.Recording)
                _features.NotifyUsed(Feature.Evp);
        }
    }
}
