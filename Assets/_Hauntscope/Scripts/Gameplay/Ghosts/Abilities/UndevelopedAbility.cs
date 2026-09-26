namespace Hauntscope.Gameplay.Ghosts.Abilities
{
    // The negative (GDD 5.32) lives on film: the lens never shows it (Ghost.IsPhotoOnly). A photo with it in the frame
    // develops it for a few seconds, and only then can the lens and the beam get hold of it; every new photo starts
    // the clock again.
    public sealed class UndevelopedAbility : IGhostAbility
    {
        private readonly float _developDuration;

        private int _photos;
        private float _remaining;

        public UndevelopedAbility(float developDuration)
        {
            _developDuration = developDuration;
        }

        public void Tick(Ghost ghost, float deltaTime)
        {
            if (ghost.PhotoCount != _photos)
            {
                _photos = ghost.PhotoCount;
                _remaining = _developDuration;
                ghost.SetDeveloped(true);
                return;
            }

            if (!ghost.IsDeveloped)
                return;

            _remaining -= deltaTime;
            if (_remaining <= 0f)
                ghost.SetDeveloped(false);
        }
    }
}
