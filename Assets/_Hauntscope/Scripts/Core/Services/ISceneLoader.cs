using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Core.Services
{
    public interface ISceneLoader
    {
        UniTask LoadAsync(SceneId scene, CancellationToken cancellationToken);
    }
}
