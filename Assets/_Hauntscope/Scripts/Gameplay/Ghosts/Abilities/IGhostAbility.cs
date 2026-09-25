namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    public interface IGhostAbility
    {
        void Tick(Ghost ghost, float deltaTime);
    }
}
