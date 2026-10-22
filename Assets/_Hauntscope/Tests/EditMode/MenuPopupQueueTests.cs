using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Season;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class MenuPopupQueueTests
    {
        [Test]
        public void Enqueue_Idle_OpensAtOnce()
        {
            var queue = new MenuPopupQueue();
            var popup = new Popup();

            queue.Enqueue(popup);

            Assert.AreEqual(1, popup.Opened);
            Assert.IsFalse(queue.IsIdle);
        }

        [Test]
        public void Enqueue_QuietPromoTake_OpensNothing()
        {
            var queue = new MenuPopupQueue(new DemoSwitchesBuilder().Halloween().Build());
            var popup = new Popup();

            queue.Enqueue(popup);

            Assert.AreEqual(0, popup.Opened);
            Assert.IsTrue(queue.IsIdle);
        }

        [Test]
        public void Enqueue_NoSwitchesFile_OpensAsUsual()
        {
            var queue = new MenuPopupQueue(DemoSwitches.Off);
            var popup = new Popup();

            queue.Enqueue(popup);

            Assert.AreEqual(1, popup.Opened);
        }

        [Test]
        public void Enqueue_WhileAnotherIsOpen_WaitsForIt()
        {
            var queue = new MenuPopupQueue();
            var first = new Popup();
            var second = new Popup();
            queue.Enqueue(first);

            queue.Enqueue(second);

            Assert.AreEqual(0, second.Opened);
        }

        [Test]
        public void NotifyClosed_Current_OpensTheNext()
        {
            var queue = new MenuPopupQueue();
            var first = new Popup();
            var second = new Popup();
            queue.Enqueue(first);
            queue.Enqueue(second);

            queue.NotifyClosed(first);

            Assert.AreEqual(1, second.Opened);
            Assert.IsTrue(queue.IsOpen(second));
        }

        [Test]
        public void NotifyClosed_Last_LeavesTheQueueIdle()
        {
            var queue = new MenuPopupQueue();
            var popup = new Popup();
            queue.Enqueue(popup);

            queue.NotifyClosed(popup);

            Assert.IsTrue(queue.IsIdle);
        }

        [Test]
        public void Enqueue_AlreadyOpen_IsIgnored()
        {
            var queue = new MenuPopupQueue();
            var popup = new Popup();
            queue.Enqueue(popup);

            queue.Enqueue(popup);
            queue.NotifyClosed(popup);

            Assert.AreEqual(1, popup.Opened);
            Assert.IsTrue(queue.IsIdle);
        }

        [Test]
        public void NotifyClosed_NotTheOpenOne_IsIgnored()
        {
            var queue = new MenuPopupQueue();
            var first = new Popup();
            var second = new Popup();
            queue.Enqueue(first);
            queue.Enqueue(second);

            queue.NotifyClosed(second);

            Assert.IsTrue(queue.IsOpen(first));
            Assert.AreEqual(0, second.Opened);
        }

        [Test]
        public void CloseCurrent_OneOpen_AsksItToClose()
        {
            var queue = new MenuPopupQueue();
            var popup = new Popup();
            queue.Enqueue(popup);

            var closed = queue.CloseCurrent();

            Assert.IsTrue(closed);
            Assert.AreEqual(1, popup.Closed);
        }

        [Test]
        public void CloseCurrent_NothingOpen_ReturnsFalse()
        {
            Assert.IsFalse(new MenuPopupQueue().CloseCurrent());
        }

        private sealed class Popup : IMenuPopup
        {
            public int Opened { get; private set; }

            public int Closed { get; private set; }

            public void Open()
            {
                Opened++;
            }

            public void Close()
            {
                Closed++;
            }
        }
    }
}
