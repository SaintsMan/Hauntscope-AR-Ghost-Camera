using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Ghosts;

namespace Hauntscope.Gameplay.Hunt
{
    public sealed class HuntSession
    {
        private readonly ObservableValue<Ghost> _ghost = new ObservableValue<Ghost>();

        public IReadOnlyObservableValue<Ghost> Ghost => _ghost;

        public void SetGhost(Ghost ghost)
        {
            _ghost.Value = ghost;
        }
    }
}
