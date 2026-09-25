namespace Hauntscope.Gameplay.Tools
{
    public sealed class Toolbelt
    {
        private readonly GhostLens _lens;
        private readonly CaptureBeam _beam;
        private readonly ITool[] _tools;

        public Toolbelt(GhostLens lens, CaptureBeam beam)
        {
            _lens = lens;
            _beam = beam;
            _tools = new ITool[] { lens, beam };
        }

        public GhostLens Lens => _lens;

        public CaptureBeam Beam => _beam;

        public float TotalDrainPerSecond
        {
            get
            {
                var drain = 0f;
                foreach (var tool in _tools)
                {
                    if (tool.IsActive.Value)
                        drain += tool.DrainPerSecond;
                }

                return drain;
            }
        }

        public void ToggleLens()
        {
            if (_lens.IsActive.Value)
                _lens.Deactivate();
            else
                _lens.Activate();
        }

        public void StartBeam()
        {
            _lens.Activate();
            _beam.Activate();
        }

        public void StopBeam()
        {
            _beam.Deactivate();
        }

        public void DeactivateAll()
        {
            foreach (var tool in _tools)
                tool.Deactivate();
        }

        // Order matters: the lens updates Reveal before the beam checks it.
        public void Tick(float deltaTime)
        {
            foreach (var tool in _tools)
                tool.Tick(deltaTime);
        }
    }
}
