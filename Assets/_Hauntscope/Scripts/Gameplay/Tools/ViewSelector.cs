using System.Collections.Generic;
using Hauntscope.Core.Observables;

namespace Hauntscope.Gameplay.Tools
{
    // The MODE button: steps through the issued camera modes and back to the plain picture. It is the only thing that
    // switches modes, so exactly one (or none) is ever on. On the toolbelt as one tool: its drain is the current mode's.
    public sealed class ViewSelector : ITool
    {
        private readonly IReadOnlyList<IViewMode> _modes;
        private readonly ObservableValue<IViewMode> _current = new ObservableValue<IViewMode>();
        private readonly ObservableValue<bool> _isActive = new ObservableValue<bool>();

        public ViewSelector(IReadOnlyList<IViewMode> modes)
        {
            _modes = modes;
        }

        // Null is the plain picture.
        public IReadOnlyObservableValue<IViewMode> Current => _current;

        public IReadOnlyObservableValue<bool> IsActive => _isActive;

        public float DrainPerSecond => _current.Value != null ? _current.Value.DrainPerSecond : 0f;

        public bool HasUnlocked
        {
            get
            {
                for (var i = 0; i < _modes.Count; i++)
                {
                    if (_modes[i].IsUnlocked)
                        return true;
                }

                return false;
            }
        }

        // Next issued mode after the current one; after the last comes the plain picture.
        public void Cycle()
        {
            var start = _current.Value != null ? IndexOf(_current.Value) + 1 : 0;
            for (var i = start; i < _modes.Count; i++)
            {
                if (!_modes[i].IsUnlocked)
                    continue;

                Select(_modes[i]);
                return;
            }

            Select(null);
        }

        public void Activate()
        {
            if (_current.Value == null)
                Cycle();
        }

        public void Deactivate()
        {
            Select(null);
        }

        // Every mode ticks, on or off, so an effect it left on the ghost can fade out after switching away.
        public void Tick(float deltaTime)
        {
            for (var i = 0; i < _modes.Count; i++)
                _modes[i].Tick(deltaTime);
        }

        private void Select(IViewMode mode)
        {
            if (_current.Value == mode)
                return;

            _current.Value?.Deactivate();
            mode?.Activate();
            _current.Value = mode;
            _isActive.Value = mode != null;
        }

        private int IndexOf(IViewMode mode)
        {
            for (var i = 0; i < _modes.Count; i++)
            {
                if (_modes[i] == mode)
                    return i;
            }

            return -1;
        }
    }
}
