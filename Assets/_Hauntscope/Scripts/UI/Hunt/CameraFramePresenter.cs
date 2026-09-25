using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Tools;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class CameraFramePresenter : IStartable, ITickable, IDisposable
    {
        private const string TimecodeKey = "hud.timecode";
        private const int SecondsPerMinute = 60;
        private const int SecondsPerHour = 3600;

        private readonly CameraFrameView _view;
        private readonly Battery _battery;
        private readonly HudConfig _config;
        private readonly ISfxPlayer _sfx;
        private readonly ILocalizationService _localization;
        private readonly IRandom _random;

        private float _recordedTime;
        private int _shownSecond = -1;
        private float _recBlinkTime;
        private bool _recDotVisible = true;
        private float _lowBlinkTime;
        private bool _lowHighlighted = true;
        private float _grainTime;

        public CameraFramePresenter(
            CameraFrameView view,
            Battery battery,
            HudConfig config,
            ISfxPlayer sfx,
            ILocalizationService localization,
            IRandom random)
        {
            _view = view;
            _battery = battery;
            _config = config;
            _sfx = sfx;
            _localization = localization;
            _random = random;
        }

        public void Start()
        {
            _battery.Charge.Changed += OnBatteryChanged;
            _battery.IsLow.Changed += OnBatteryLowChanged;
            _localization.Changed += OnLanguageChanged;
            RenderTimecode(true);
            RenderBattery();
            _view.SetRecDotVisible(true);
        }

        public void Tick()
        {
            var deltaTime = Time.deltaTime;
            _recordedTime += deltaTime;
            RenderTimecode(false);

            _recBlinkTime += deltaTime;
            if (_recBlinkTime >= _config.RecBlinkInterval)
            {
                _recBlinkTime = 0f;
                _recDotVisible = !_recDotVisible;
                _view.SetRecDotVisible(_recDotVisible);
            }

            if (_battery.IsLow.Value)
            {
                _lowBlinkTime += deltaTime;
                if (_lowBlinkTime >= _config.LowBatteryBlinkInterval)
                {
                    _lowBlinkTime = 0f;
                    _lowHighlighted = !_lowHighlighted;
                    RenderBattery();
                }
            }

            _grainTime += deltaTime;
            if (_grainTime >= 1f / _config.GrainFrameRate)
            {
                _grainTime = 0f;
                _view.SetGrainOffset(new Vector2(_random.Value, _random.Value));
            }
        }

        public void Dispose()
        {
            _battery.Charge.Changed -= OnBatteryChanged;
            _battery.IsLow.Changed -= OnBatteryLowChanged;
            _localization.Changed -= OnLanguageChanged;
        }

        private void OnBatteryChanged(float charge)
        {
            RenderBattery();
        }

        private void OnBatteryLowChanged(bool low)
        {
            _lowHighlighted = true;
            _lowBlinkTime = 0f;
            RenderBattery();
            if (low)
                _sfx.Play2D(_config.LowBatteryClip, _config.LowBatteryVolume, 1f);
        }

        private void OnLanguageChanged()
        {
            RenderTimecode(true);
        }

        private void RenderBattery()
        {
            _view.SetBattery(_battery.Normalized, _battery.IsLow.Value, _lowHighlighted);
        }

        private void RenderTimecode(bool force)
        {
            var seconds = Mathf.FloorToInt(_recordedTime);
            if (!force && seconds == _shownSecond)
                return;

            _shownSecond = seconds;
            _view.SetTimecode(_localization.Get(LocalizationTable.Ui, TimecodeKey,
                seconds / SecondsPerHour, seconds / SecondsPerMinute % SecondsPerMinute, seconds % SecondsPerMinute));
        }
    }
}
