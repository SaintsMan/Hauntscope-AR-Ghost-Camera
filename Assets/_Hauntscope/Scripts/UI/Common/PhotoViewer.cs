using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Photo;

namespace Hauntscope.UI.Common
{
    // Which photo is open full-screen, if any. Screens that show a thumbnail open it here and never talk to the viewer.
    public sealed class PhotoViewer
    {
        private readonly ObservableValue<PhotoRecord> _current = new ObservableValue<PhotoRecord>();

        public IReadOnlyObservableValue<PhotoRecord> Current => _current;

        public bool IsOpen => _current.Value != null;

        public void Open(PhotoRecord photo)
        {
            _current.Value = photo;
        }

        public void Close()
        {
            _current.Value = null;
        }
    }
}
