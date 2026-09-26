using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Contracts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.UI.Common;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    // The board hears about a hunt after its card is up, so the toast waits for the board, not for the result.
    public sealed class ContractToastPresenter : IStartable, IDisposable
    {
        private const string OneKey = "result.contract.one";
        private const string ManyKey = "result.contract.many";
        private const string ClaimKey = "result.contract.claim";

        private readonly ContractToastView _view;
        private readonly ContractBoard _board;
        private readonly HuntSession _session;
        private readonly ContractDescriber _describer;
        private readonly ILocalizationService _localization;

        private bool _isRecorded;

        public ContractToastPresenter(ContractToastView view, ContractBoard board, HuntSession session, ContractDescriber describer,
            ILocalizationService localization)
        {
            _view = view;
            _board = board;
            _session = session;
            _describer = describer;
            _localization = localization;
        }

        public void Start()
        {
            _board.Recorded += OnRecorded;
            _session.Result.Changed += OnResultChanged;
            _localization.Changed += Render;
            _view.Hide();
        }

        public void Dispose()
        {
            _board.Recorded -= OnRecorded;
            _session.Result.Changed -= OnResultChanged;
            _localization.Changed -= Render;
        }

        private void OnRecorded()
        {
            _isRecorded = true;
            Render();
        }

        private void OnResultChanged(HuntResult result)
        {
            if (result != null)
                return;

            _isRecorded = false;
            _view.Hide();
        }

        private void Render()
        {
            var done = _board.JustCompleted;
            if (!_isRecorded || _session.Result.Value == null || done.Count == 0)
            {
                _view.Hide();
                return;
            }

            var title = done.Count == 1
                ? _localization.Get(LocalizationTable.Ui, OneKey)
                : _localization.Get(LocalizationTable.Ui, ManyKey, done.Count);
            _view.Show(title, _describer.Describe(done[0]), _localization.Get(LocalizationTable.Ui, ClaimKey));
        }
    }
}
