using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Hunt;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class TutorialPresenter : IStartable, IDisposable
    {
        private const string CounterKey = "tutorial.counter";
        private const string WalkKey = "tutorial.walk";
        private const string EmfKey = "tutorial.emf";
        private const string LensKey = "tutorial.lens";
        private const string BeamKey = "tutorial.beam";

        private readonly TutorialFlow _flow;
        private readonly TutorialView _view;
        private readonly ILocalizationService _localization;

        public TutorialPresenter(TutorialFlow flow, TutorialView view, ILocalizationService localization)
        {
            _flow = flow;
            _view = view;
            _localization = localization;
        }

        public void Start()
        {
            _flow.Step.Changed += OnStepChanged;
            _localization.Changed += OnLanguageChanged;
            OnStepChanged(_flow.Step.Value);
        }

        public void Dispose()
        {
            _flow.Step.Changed -= OnStepChanged;
            _localization.Changed -= OnLanguageChanged;
        }

        private void OnStepChanged(TutorialStep step)
        {
            var key = HintKey(step);
            _view.SetVisible(key != null);
            _view.SetWalkCue(step == TutorialStep.Walk);
            _view.SetEmfCue(step == TutorialStep.FollowEmf);
            _view.SetLensCue(step == TutorialStep.UseLens);
            _view.SetBeamCue(step == TutorialStep.HoldBeam);
            if (key == null)
                return;

            _view.SetHint(_localization.Get(LocalizationTable.Ui, key));
            _view.SetCounter(_localization.Get(LocalizationTable.Ui, CounterKey, _flow.StepNumber, _flow.StepCount));
        }

        private void OnLanguageChanged()
        {
            OnStepChanged(_flow.Step.Value);
        }

        private static string HintKey(TutorialStep step)
        {
            switch (step)
            {
                case TutorialStep.Walk:
                    return WalkKey;
                case TutorialStep.FollowEmf:
                    return EmfKey;
                case TutorialStep.UseLens:
                    return LensKey;
                case TutorialStep.HoldBeam:
                    return BeamKey;
                default:
                    return null;
            }
        }
    }
}
