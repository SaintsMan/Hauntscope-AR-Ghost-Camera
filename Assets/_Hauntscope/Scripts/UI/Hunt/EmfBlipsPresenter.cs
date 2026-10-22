using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Season;
using Hauntscope.Gameplay.Tools;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    // Feeds the compass strip from the radar: shown while it hears more than one source, so a hunt with a single ghost
    // looks exactly as it always did.
    public sealed class EmfBlipsPresenter : IStartable, ITickable, IDisposable
    {
        private const string CaptionKey = "hud.scope.caption";
        private const int MinSources = 2;

        private readonly EmfBlipsView _view;
        private readonly EmfRadar _radar;
        private readonly HuntPause _pause;
        private readonly HalloweenCollection _collection;
        private readonly ILocalizationService _localization;

        public EmfBlipsPresenter(EmfBlipsView view, EmfRadar radar, HuntPause pause, HalloweenCollection collection, ILocalizationService localization)
        {
            _view = view;
            _radar = radar;
            _pause = pause;
            _collection = collection;
            _localization = localization;
        }

        public void Start()
        {
            _localization.Changed += RenderText;
            _collection.Worn += RenderSkin;
            RenderText();
            RenderSkin();
            _view.SetBlips(_radar.Signals, false);
        }

        public void Tick()
        {
            _view.SetBlips(_radar.Signals, _radar.Signals.Count >= MinSources && !_pause.IsPaused);
        }

        public void Dispose()
        {
            _localization.Changed -= RenderText;
            _collection.Worn -= RenderSkin;
        }

        private void RenderText()
        {
            _view.SetCaption(_localization.Get(LocalizationTable.Ui, CaptionKey));
        }

        private void RenderSkin()
        {
            var skin = _collection.WornIn(HalloweenSkinSlot.Radar);
            _view.SetTint(skin != null ? skin.Color : new UnityEngine.Color(1f, 0.71f, 0.28f, 1f));
        }
    }
}
