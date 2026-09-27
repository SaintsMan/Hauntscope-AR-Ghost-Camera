namespace Hauntscope.Gameplay.Tools
{
    public sealed class Toolbelt
    {
        private readonly GhostLens _lens;
        private readonly CaptureBeam _beam;
        private readonly SpiritBox _spiritBox;
        private readonly ViewSelector _views;
        private readonly ITool[] _tools;

        public Toolbelt(GhostLens lens, CaptureBeam beam, SpiritBox spiritBox, ViewSelector views)
        {
            _lens = lens;
            _beam = beam;
            _spiritBox = spiritBox;
            _views = views;
            _tools = new ITool[] { lens, beam, spiritBox, views };
        }

        public GhostLens Lens => _lens;

        public CaptureBeam Beam => _beam;

        public SpiritBox SpiritBox => _spiritBox;

        public ViewSelector Views => _views;

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

        public void ToggleSpiritBox()
        {
            if (_spiritBox.IsActive.Value)
                _spiritBox.Deactivate();
            else
                _spiritBox.Activate();
        }

        public void CycleView()
        {
            _views.Cycle();
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
