namespace Hauntscope.Gameplay.Tools
{
    public sealed class Toolbelt
    {
        private readonly GhostLens _lens;
        private readonly ITool[] _tools;

        public Toolbelt(GhostLens lens)
        {
            _lens = lens;
            _tools = new ITool[] { lens };
        }

        public GhostLens Lens => _lens;

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

        public void DeactivateAll()
        {
            foreach (var tool in _tools)
                tool.Deactivate();
        }

        public void Tick(float deltaTime)
        {
            foreach (var tool in _tools)
                tool.Tick(deltaTime);
        }
    }
}
