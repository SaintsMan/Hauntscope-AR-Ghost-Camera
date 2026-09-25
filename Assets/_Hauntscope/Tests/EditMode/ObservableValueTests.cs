using Hauntscope.Core.Observables;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ObservableValueTests
    {
        [Test]
        public void Constructor_WithInitialValue_ExposesIt()
        {
            var observable = new ObservableValue<int>(5);

            Assert.AreEqual(5, observable.Value);
        }

        [Test]
        public void Value_SetDifferent_RaisesChangedWithNewValue()
        {
            var observable = new ObservableValue<int>(1);
            var received = 0;
            observable.Changed += value => received = value;

            observable.Value = 2;

            Assert.AreEqual(2, received);
        }

        [Test]
        public void Value_SetSame_DoesNotRaiseChanged()
        {
            var observable = new ObservableValue<int>(1);
            var raised = false;
            observable.Changed += _ => raised = true;

            observable.Value = 1;

            Assert.IsFalse(raised);
        }

        [Test]
        public void Value_SetDifferent_IsUpdatedBeforeNotification()
        {
            var observable = new ObservableValue<string>("a");
            string seen = null;
            observable.Changed += _ => seen = observable.Value;

            observable.Value = "b";

            Assert.AreEqual("b", seen);
        }

        [Test]
        public void Value_HandlerUnsubscribed_IsNotCalled()
        {
            var observable = new ObservableValue<int>();
            var calls = 0;
            void Handler(int _) => calls++;
            observable.Changed += Handler;
            observable.Changed -= Handler;

            observable.Value = 3;

            Assert.AreEqual(0, calls);
        }
    }
}
