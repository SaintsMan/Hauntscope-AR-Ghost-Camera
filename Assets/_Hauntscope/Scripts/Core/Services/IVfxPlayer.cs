using UnityEngine;

namespace Hauntscope.Core.Services
{
    public interface IVfxPlayer
    {
        void Play(VfxId id, Vector3 position, Color color);

        IVfxLoop PlayLoop(VfxId id, Vector3 position, Color color);
    }
}
