using System;
using System.Collections.Generic;

namespace Hauntscope.Core.StateMachines
{
    public sealed class StateMachine
    {
        private readonly Dictionary<IState, List<Transition>> _transitions = new Dictionary<IState, List<Transition>>();
        private readonly List<Transition> _anyTransitions = new List<Transition>();
        private readonly List<Transition> _noTransitions = new List<Transition>(0);
        private List<Transition> _currentTransitions;

        public IState CurrentState { get; private set; }

        public void AddTransition(IState from, IState to, Func<bool> condition)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            if (!_transitions.TryGetValue(from, out var transitions))
            {
                transitions = new List<Transition>();
                _transitions.Add(from, transitions);
            }

            transitions.Add(CreateTransition(to, condition));
        }

        public void AddAnyTransition(IState to, Func<bool> condition)
        {
            _anyTransitions.Add(CreateTransition(to, condition));
        }

        public void Start(IState initialState)
        {
            if (initialState == null)
                throw new ArgumentNullException(nameof(initialState));

            ChangeState(initialState);
        }

        public void Tick(float deltaTime)
        {
            if (CurrentState == null)
                throw new InvalidOperationException("StateMachine must be started before Tick.");

            var next = FindNextState();
            if (next != null)
                ChangeState(next);

            CurrentState.Tick(deltaTime);
        }

        private IState FindNextState()
        {
            for (var i = 0; i < _anyTransitions.Count; i++)
            {
                var transition = _anyTransitions[i];
                if (transition.To != CurrentState && transition.Condition())
                    return transition.To;
            }

            for (var i = 0; i < _currentTransitions.Count; i++)
            {
                var transition = _currentTransitions[i];
                if (transition.Condition())
                    return transition.To;
            }

            return null;
        }

        private void ChangeState(IState state)
        {
            CurrentState?.Exit();
            CurrentState = state;
            _currentTransitions = _transitions.TryGetValue(state, out var transitions) ? transitions : _noTransitions;
            CurrentState.Enter();
        }

        private static Transition CreateTransition(IState to, Func<bool> condition)
        {
            if (to == null)
                throw new ArgumentNullException(nameof(to));
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            return new Transition(to, condition);
        }

        private sealed class Transition
        {
            public Transition(IState to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }

            public IState To { get; }

            public Func<bool> Condition { get; }
        }
    }
}
