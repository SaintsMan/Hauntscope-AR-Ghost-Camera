using System;
using System.Text;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Tools;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    // Shows the box once the agency has issued it, switches it, and prints what its voice says.
    public sealed class SpiritBoxPresenter : IStartable, IDisposable
    {
        // Kept with the seen tips, so resetting the tips brings the NEW mark back too.
        private const string UsedKey = "spirit_box.used";
        private const string AnswerKey = "hud.spirit.answer";
        private const string SeparatorKey = "hud.spirit.separator";
        private const string HidingKey = "hud.spirit.hiding";
        private const string CrackleKey = "hud.spirit.static";
        private const string CloseKey = "hud.spirit.close";
        private const string FarKey = "hud.spirit.far";

        private static readonly string[] DirectionKeys = { "hud.spirit.ahead", "hud.spirit.left", "hud.spirit.right", "hud.spirit.behind" };

        private readonly SpiritBoxView _view;
        private readonly Toolbelt _toolbelt;
        private readonly PlayerProgress _progress;
        private readonly ILocalizationService _localization;
        private readonly SpiritBoxConfig _config;
        private readonly StringBuilder _words = new StringBuilder();

        public SpiritBoxPresenter(SpiritBoxView view, Toolbelt toolbelt, PlayerProgress progress, ILocalizationService localization,
            SpiritBoxConfig config)
        {
            _view = view;
            _toolbelt = toolbelt;
            _progress = progress;
            _localization = localization;
            _config = config;
        }

        private SpiritBox Box => _toolbelt.SpiritBox;

        public void Start()
        {
            _view.Clicked += OnClicked;
            Box.IsActive.Changed += OnActiveChanged;
            Box.Answered += OnAnswered;
            Box.Crackled += OnCrackled;
            _progress.Changed += RenderAvailability;

            _view.HideAnswer();
            _view.SetActive(Box.IsActive.Value);
            RenderAvailability();
        }

        public void Dispose()
        {
            _view.Clicked -= OnClicked;
            Box.IsActive.Changed -= OnActiveChanged;
            Box.Answered -= OnAnswered;
            Box.Crackled -= OnCrackled;
            _progress.Changed -= RenderAvailability;
        }

        // A capture in a night shift can issue the box between rounds, without a new scene.
        private void RenderAvailability()
        {
            var unlocked = Box.IsUnlocked;
            _view.SetAvailable(unlocked);
            _view.SetNew(unlocked && !_progress.HasSeenTip(UsedKey));
        }

        private void OnClicked()
        {
            _toolbelt.ToggleSpiritBox();
            if (!_progress.HasSeenTip(UsedKey))
                _progress.MarkTipSeen(UsedKey);
        }

        private void OnActiveChanged(bool active)
        {
            _view.SetActive(active);
            if (!active)
                _view.HideAnswer();
        }

        private void OnAnswered(SpiritBoxAnswer answer)
        {
            _words.Clear();
            if (answer.IsHiding)
                _words.Append(Ui(HidingKey)).Append(Ui(SeparatorKey));
            _words.Append(Ui(DirectionKeys[(int)answer.Direction]));
            if (answer.Range == SpiritRange.Close)
                _words.Append(Ui(SeparatorKey)).Append(Ui(CloseKey));
            else if (answer.Range == SpiritRange.Far)
                _words.Append(Ui(SeparatorKey)).Append(Ui(FarKey));
            _view.ShowAnswer(_localization.Get(LocalizationTable.Ui, AnswerKey, _words.ToString()), _config.AnswerShowTime);
        }

        private void OnCrackled()
        {
            _view.ShowAnswer(Ui(CrackleKey), _config.AnswerShowTime);
        }

        private string Ui(string key)
        {
            return _localization.Get(LocalizationTable.Ui, key);
        }
    }
}
