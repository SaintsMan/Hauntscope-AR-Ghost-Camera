using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Photo;
using Hauntscope.UI.Common;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    // Drives the prank screen: the strip of caught ghosts, PLACE / MOVE, gestures into the pose, and the shutter, whose
    // photo opens straight in the viewer to be shared.
    public sealed class PrankPhotoPresenter : IStartable, IDisposable
    {
        private const string AimHintKey = "prank.hint.aim";
        private const string AdjustHintKey = "prank.hint.adjust";
        private const string PlaceKey = "prank.place";
        private const string LiftKey = "prank.lift";

        private readonly PrankPhotoView _view;
        private readonly PrankPhotoMode _prank;
        private readonly PhotoViewer _viewer;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private readonly ISceneLoader _scenes;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public PrankPhotoPresenter(PrankPhotoView view, PrankPhotoMode prank, PhotoViewer viewer, ILocalizationService localization, UiFeedback ui,
            ISceneLoader scenes)
        {
            _scenes = scenes;
            _view = view;
            _prank = prank;
            _viewer = viewer;
            _localization = localization;
            _ui = ui;
        }

        public void Start()
        {
            _prank.Phase.Changed += OnPhaseChanged;
            _prank.Ghost.Changed += OnGhostChanged;
            _prank.IsShooting.Changed += OnShootingChanged;
            _localization.Changed += OnLanguageChanged;
            _view.SlotClicked += OnSlotClicked;
            _view.PlaceClicked += OnPlaceClicked;
            _view.ShutterClicked += OnShutterClicked;
            _view.ExitClicked += OnExitClicked;
            _view.Dragged += _prank.Move;
            _view.Pinched += _prank.Resize;
            _view.Twisted += _prank.Turn;

            OnPhaseChanged(_prank.Phase.Value);
        }

        public void Dispose()
        {
            _prank.Phase.Changed -= OnPhaseChanged;
            _prank.Ghost.Changed -= OnGhostChanged;
            _prank.IsShooting.Changed -= OnShootingChanged;
            _localization.Changed -= OnLanguageChanged;
            _view.SlotClicked -= OnSlotClicked;
            _view.PlaceClicked -= OnPlaceClicked;
            _view.ShutterClicked -= OnShutterClicked;
            _view.ExitClicked -= OnExitClicked;
            _view.Dragged -= _prank.Move;
            _view.Pinched -= _prank.Resize;
            _view.Twisted -= _prank.Turn;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void OnPhaseChanged(PrankPhase phase)
        {
            _view.SetVisible(phase != PrankPhase.Off);
            if (phase == PrankPhase.Off)
                return;

            // The roster is read once the mode begins; it does not change during a prank.
            if (_view.SlotCount == 0)
            {
                foreach (var ghost in _prank.Roster)
                    _view.AddSlot(ghost.Icon, ghost.RimColor);
            }

            _view.SetPadActive(phase == PrankPhase.Placed);
            RenderTexts();
            OnGhostChanged(_prank.Ghost.Value);
        }

        private void OnGhostChanged(GhostData ghost)
        {
            _view.SetSelected(ghost != null ? IndexOf(ghost) : -1);
            _view.SetName(ghost != null ? _localization.Get(LocalizationTable.Ghosts, ghost.NameKey).ToUpperInvariant() : string.Empty);
            OnShootingChanged(_prank.IsShooting.Value);
        }

        private void OnShootingChanged(bool shooting)
        {
            _view.SetShutterEnabled(!shooting && _prank.Ghost.Value != null);
        }

        private void OnLanguageChanged()
        {
            RenderTexts();
            OnGhostChanged(_prank.Ghost.Value);
        }

        private void RenderTexts()
        {
            var placed = _prank.Phase.Value == PrankPhase.Placed;
            _view.SetHint(_localization.Get(LocalizationTable.Ui, placed ? AdjustHintKey : AimHintKey));
            _view.SetPlaceLabel(_localization.Get(LocalizationTable.Ui, placed ? LiftKey : PlaceKey));
        }

        private void OnSlotClicked(int index)
        {
            _ui.PlayClick();
            _prank.Select(_prank.Roster[index]);
        }

        private void OnPlaceClicked()
        {
            _ui.PlayClick();
            if (_prank.Phase.Value == PrankPhase.Placed)
                _prank.Lift();
            else
                _prank.Place();
        }

        private void OnExitClicked()
        {
            _ui.PlayBack();
            _scenes.LoadAsync(SceneId.MainMenu, _lifetime.Token).Forget();
        }

        private void OnShutterClicked()
        {
            ShootAsync(_lifetime.Token).Forget();
        }

        private async UniTaskVoid ShootAsync(CancellationToken cancellationToken)
        {
            var photo = await _prank.ShootAsync(cancellationToken);
            if (photo != null)
                _viewer.Open(photo);
        }

        private int IndexOf(GhostData ghost)
        {
            for (var i = 0; i < _prank.Roster.Count; i++)
            {
                if (_prank.Roster[i] == ghost)
                    return i;
            }

            return -1;
        }
    }
}
