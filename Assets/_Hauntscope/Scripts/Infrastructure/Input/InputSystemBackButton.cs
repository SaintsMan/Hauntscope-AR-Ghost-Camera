using System;
using Hauntscope.Core.Services;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Hauntscope.Infrastructure.Input
{
    public sealed class InputSystemBackButton : IBackButton, IInitializable, IDisposable
    {
        // The Input System reports Android's Back button and back gesture as the Escape key.
        private const string Binding = "<Keyboard>/escape";

        private readonly InputAction _action = new InputAction("Back", InputActionType.Button, Binding);

        public event Action Pressed;

        public void Initialize()
        {
            _action.performed += OnPerformed;
            _action.Enable();
        }

        public void Dispose()
        {
            _action.performed -= OnPerformed;
            _action.Disable();
            _action.Dispose();
        }

        private void OnPerformed(InputAction.CallbackContext context)
        {
            Pressed?.Invoke();
        }
    }
}
