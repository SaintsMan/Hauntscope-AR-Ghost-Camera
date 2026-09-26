using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Photo;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakePhotoCapture : IPhotoCapture
    {
        private readonly IPhotoStorage _storage;

        public FakePhotoCapture(IPhotoStorage storage)
        {
            _storage = storage;
        }

        public bool Fails { get; set; }

        public PhotoCaption LastCaption { get; private set; }

        public Action OnCapture { get; set; }

        public UniTask<string> CaptureAsync(PhotoCaption caption, CancellationToken cancellationToken)
        {
            LastCaption = caption;
            OnCapture?.Invoke();
            if (Fails)
                throw new InvalidOperationException("disk full");

            return UniTask.FromResult(_storage.Save(new byte[1]));
        }
    }
}
