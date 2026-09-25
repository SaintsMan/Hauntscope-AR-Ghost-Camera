using System;
using Hauntscope.Gameplay.Environment;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class LaunchConfig
    {
        [SerializeField] private PermissionResult _editorCameraPermission = PermissionResult.Granted;

        public LaunchConfig()
        {
        }

        public LaunchConfig(PermissionResult editorCameraPermission)
        {
            _editorCameraPermission = editorCameraPermission;
        }

        public PermissionResult EditorCameraPermission => _editorCameraPermission;
    }
}
