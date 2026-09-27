using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Tools;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Feedback
{
    // Puts the current camera mode's filter over the viewfinder, with the sound of the mode switching on.
    public sealed class ViewModeFeedback : IStartable, IDisposable
    {
        private readonly ViewSelector _views;
        private readonly IViewFilter _filter;
        private readonly ISfxPlayer _sfx;
        private readonly ViewConfig _config;

        public ViewModeFeedback(ViewSelector views, IViewFilter filter, ISfxPlayer sfx, ViewConfig config)
        {
            _views = views;
            _filter = filter;
            _sfx = sfx;
            _config = config;
        }

        public void Start()
        {
            _views.Current.Changed += OnModeChanged;
            Render(_views.Current.Value);
        }

        public void Dispose()
        {
            _views.Current.Changed -= OnModeChanged;
            _filter.Hide();
        }

        private void OnModeChanged(IViewMode mode)
        {
            Render(mode);
            if (mode != null && mode.OnClip != null)
                _sfx.Play2D(mode.OnClip, mode.OnVolume, 1f);
            else if (mode == null && _config.OffClip != null)
                _sfx.Play2D(_config.OffClip, _config.OffVolume, 1f);
        }

        private void Render(IViewMode mode)
        {
            if (mode != null && mode.Filter != null)
                _filter.Show(mode.Filter);
            else
                _filter.Hide();
        }
    }
}
