namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    public sealed class BlinkAbility : IGhostAbility
    {
        private readonly float _visibleDuration;
        private readonly float _period;
        private float _phase;

        public BlinkAbility(float visibleDuration, float period)
        {
            _visibleDuration = visibleDuration;
            _period = period;
        }

        public void Tick(Ghost ghost, float deltaTime)
        {
            _phase = (_phase + deltaTime) % _period;
            ghost.SetVisible(_phase < _visibleDuration);
        }
    }
}
