namespace Hauntscope.Gameplay.Tools
{
    public sealed class Toolbelt
    {
        private readonly GhostLens _lens;
        private readonly CaptureBeam _beam;
        private readonly SpiritBox _spiritBox;
        private readonly ViewSelector _views;
        private readonly EvpRecorder _evp;
        private readonly ITool[] _tools;

        private bool _lensFromBeam;

        public Toolbelt(GhostLens lens, CaptureBeam beam, SpiritBox spiritBox, ViewSelector views, EvpRecorder evp)
        {
            _lens = lens;
            _beam = beam;
            _spiritBox = spiritBox;
            _views = views;
            _evp = evp;
            _tools = new ITool[] { lens, beam, spiritBox, views, evp };
        }

        public GhostLens Lens => _lens;

        public CaptureBeam Beam => _beam;

        public SpiritBox SpiritBox => _spiritBox;

        public ViewSelector Views => _views;

        public EvpRecorder Evp => _evp;

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
            _lensFromBeam = false;
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

        public void RecordEvp()
        {
            _evp.Activate();
        }

        public void CycleView()
        {
            _views.Cycle();
        }

        // The beam needs the lens; a lens the beam switched on goes off with it, or it would keep eating the battery
        // unseen. One the player had switched on stays on.
        public void StartBeam()
        {
            if (!_beam.IsActive.Value)
                _lensFromBeam = !_lens.IsActive.Value;
            _lens.Activate();
            _beam.Activate();
        }

        public void StopBeam()
        {
            _beam.Deactivate();
            if (_lensFromBeam)
                _lens.Deactivate();
            _lensFromBeam = false;
        }

        public void DeactivateAll()
        {
            _lensFromBeam = false;
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
