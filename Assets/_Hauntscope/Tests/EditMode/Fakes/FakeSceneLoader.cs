using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeSceneLoader : ISceneLoader
    {
        public int LoadCount { get; private set; }

        public SceneId? LastScene { get; private set; }

        public UniTask LoadAsync(SceneId scene, CancellationToken cancellationToken)
        {
            LoadCount++;
            LastScene = scene;
            return UniTask.CompletedTask;
        }
    }
}
