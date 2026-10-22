using System.Collections.Generic;
using Hauntscope.Gameplay.Season;

namespace Hauntscope.Gameplay.Engagement
{
    // The main menu's own cards (the daily ration first, then others, the review request last) open one at a time
    // and never on top of each other.
    public sealed class MenuPopupQueue
    {
        private readonly Queue<IMenuPopup> _waiting = new Queue<IMenuPopup>();

        private readonly DemoSwitches _demo;

        private IMenuPopup _current;

        public MenuPopupQueue()
            : this(DemoSwitches.Off)
        {
        }

        public MenuPopupQueue(DemoSwitches demo)
        {
            _demo = demo;
        }

        public bool IsIdle => _current == null && _waiting.Count == 0;

        public bool IsOpen(IMenuPopup popup) => _current == popup;

        public void Enqueue(IMenuPopup popup)
        {
            // A promo take wants the menu bare: no card of the game stands in its way.
            if (_demo.Quiet && _demo.IsOn)
                return;

            if (popup == _current || _waiting.Contains(popup))
                return;

            _waiting.Enqueue(popup);
            if (_current == null)
                OpenNext();
        }

        // False when nothing is open.
        public bool CloseCurrent()
        {
            if (_current == null)
                return false;

            _current.Close();
            return true;
        }

        public void NotifyClosed(IMenuPopup popup)
        {
            if (popup != _current)
                return;

            _current = null;
            OpenNext();
        }

        private void OpenNext()
        {
            if (_waiting.Count == 0)
                return;

            _current = _waiting.Dequeue();
            _current.Open();
        }
    }
}
