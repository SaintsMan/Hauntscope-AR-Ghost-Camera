using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using UnityEngine.SceneManagement;

namespace Hauntscope.Infrastructure.Scenes
{
    public sealed class SceneLoader : ISceneLoader
    {
        public UniTask LoadAsync(SceneId scene, CancellationToken cancellationToken)
        {
            // SceneId values are named exactly like the scene files in Build Settings.
            return SceneManager.LoadSceneAsync(scene.ToString()).ToUniTask(cancellationToken: cancellationToken);
        }
    }
}
