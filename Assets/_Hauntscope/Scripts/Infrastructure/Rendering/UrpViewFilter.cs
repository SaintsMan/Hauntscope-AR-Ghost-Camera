using System;
using Hauntscope.Gameplay.Environment;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer.Unity;

namespace Hauntscope.Infrastructure.Rendering
{
    // Adapter over URP: the filter pass is queued from script for each game camera while a filter is shown, so the
    // renderer asset is never edited at runtime and both the AR and the Virtual Room camera get it.
    public sealed class UrpViewFilter : IViewFilter, IStartable, IDisposable
    {
        private readonly ViewFilterPass _pass = new ViewFilterPass();

        public void Start()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        }

        public void Dispose()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            _pass.Material = null;
        }

        public void Show(Material filter)
        {
            _pass.Material = filter;
        }

        public void Hide()
        {
            _pass.Material = null;
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (_pass.Material == null || camera.cameraType != CameraType.Game)
                return;

            camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(_pass);
        }
    }
}
