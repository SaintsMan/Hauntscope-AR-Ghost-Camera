using Hauntscope.Gameplay.Tools;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Feedback
{
    // Hands the trail and how brightly the beam shows each mark to the floor decals, every frame.
    public sealed class UvTrailFeedback : ITickable
    {
        private readonly UvFlashlight _uv;
        private readonly IUvTrailView _view;

        public UvTrailFeedback(UvFlashlight uv, IUvTrailView view)
        {
            _uv = uv;
            _view = view;
        }

        public void Tick()
        {
            var trail = _uv.Trail;
            for (var i = 0; i < trail.Capacity; i++)
                _view.SetMark(i, trail[i], _uv.VisibilityOf(i));
        }
    }
}
