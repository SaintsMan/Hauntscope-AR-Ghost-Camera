using System;
using System.Collections.Generic;
using Hauntscope.Core.StateMachines;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class StateMachineTests
    {
        private List<string> _log;
        private FakeState _first;
        private FakeState _second;
        private FakeState _third;
        private StateMachine _machine;

        [SetUp]
        public void SetUp()
        {
            _log = new List<string>();
            _first = new FakeState("First", _log);
            _second = new FakeState("Second", _log);
            _third = new FakeState("Third", _log);
            _machine = new StateMachine();
        }

        [Test]
        public void Start_WithInitialState_EntersIt()
        {
            _machine.Start(_first);

            Assert.AreSame(_first, _machine.CurrentState);
            Assert.AreEqual(1, _first.EnterCount);
        }

        [Test]
        public void Start_NullState_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _machine.Start(null));
        }

        [Test]
        public void Tick_BeforeStart_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _machine.Tick(0.1f));
        }

        [Test]
        public void Tick_NoConditionMet_TicksCurrentStateWithDeltaTime()
        {
            _machine.AddTransition(_first, _second, () => false);
            _machine.Start(_first);

            _machine.Tick(0.25f);

            Assert.AreSame(_first, _machine.CurrentState);
            Assert.AreEqual(1, _first.TickCount);
            Assert.AreEqual(0.25f, _first.LastDeltaTime);
        }

        [Test]
        public void Tick_ConditionMet_ExitsCurrentBeforeEnteringTarget()
        {
            _machine.AddTransition(_first, _second, () => true);
            _machine.Start(_first);
            _log.Clear();

            _machine.Tick(0.1f);

            CollectionAssert.AreEqual(new[] { "First.Exit", "Second.Enter" }, _log);
            Assert.AreSame(_second, _machine.CurrentState);
        }

        [Test]
        public void Tick_ConditionMet_TicksNewStateInsteadOfOld()
        {
            _machine.AddTransition(_first, _second, () => true);
            _machine.Start(_first);

            _machine.Tick(0.1f);

            Assert.AreEqual(0, _first.TickCount);
            Assert.AreEqual(1, _second.TickCount);
        }

        [Test]
        public void Tick_TransitionFromOtherState_IsIgnored()
        {
            _machine.AddTransition(_second, _third, () => true);
            _machine.Start(_first);

            _machine.Tick(0.1f);

            Assert.AreSame(_first, _machine.CurrentState);
        }

        [Test]
        public void Tick_SeveralConditionsMet_UsesFirstAddedTransition()
        {
            _machine.AddTransition(_first, _second, () => true);
            _machine.AddTransition(_first, _third, () => true);
            _machine.Start(_first);

            _machine.Tick(0.1f);

            Assert.AreSame(_second, _machine.CurrentState);
            Assert.AreEqual(0, _third.EnterCount);
        }

        [Test]
        public void Tick_AfterTransition_UsesTransitionsOfNewState()
        {
            _machine.AddTransition(_first, _second, () => true);
            _machine.AddTransition(_second, _third, () => true);
            _machine.Start(_first);

            _machine.Tick(0.1f);
            _machine.Tick(0.1f);

            Assert.AreSame(_third, _machine.CurrentState);
        }

        [Test]
        public void Tick_AnyTransitionConditionMet_LeavesAnyState()
        {
            var goToThird = false;
            _machine.AddAnyTransition(_third, () => goToThird);
            _machine.AddTransition(_first, _second, () => true);
            _machine.Start(_first);
            _machine.Tick(0.1f);
            goToThird = true;

            _machine.Tick(0.1f);

            Assert.AreSame(_third, _machine.CurrentState);
        }

        [Test]
        public void Tick_AnyAndRegularTransitionMet_AnyTransitionWins()
        {
            _machine.AddTransition(_first, _second, () => true);
            _machine.AddAnyTransition(_third, () => true);
            _machine.Start(_first);

            _machine.Tick(0.1f);

            Assert.AreSame(_third, _machine.CurrentState);
        }

        [Test]
        public void Tick_AnyTransitionToCurrentState_DoesNotReenter()
        {
            _machine.AddAnyTransition(_first, () => true);
            _machine.Start(_first);

            _machine.Tick(0.1f);
            _machine.Tick(0.1f);

            Assert.AreEqual(1, _first.EnterCount);
        }

        [Test]
        public void AddAnyTransition_NullCondition_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _machine.AddAnyTransition(_first, null));
        }

        [Test]
        public void AddTransition_NullFrom_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _machine.AddTransition(null, _second, () => true));
        }

        [Test]
        public void AddTransition_NullTo_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _machine.AddTransition(_first, null, () => true));
        }

        [Test]
        public void AddTransition_NullCondition_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _machine.AddTransition(_first, _second, null));
        }
    }
}
