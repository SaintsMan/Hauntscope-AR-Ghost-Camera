using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Hauntscope.Infrastructure.Rendering
{
    // One full-screen blit of the camera picture through the filter material, after post-processing and before the
    // overlay canvas, so the HUD stays untouched.
    public sealed class ViewFilterPass : ScriptableRenderPass
    {
        private const string PassName = "Hauntscope View Filter";

        public ViewFilterPass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            requiresIntermediateTexture = true;
        }

        public Material Material { get; set; }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (Material == null)
                return;

            var resources = frameData.Get<UniversalResourceData>();
            if (resources.isActiveTargetBackBuffer)
                return;

            var source = resources.activeColorTexture;
            var description = renderGraph.GetTextureDesc(source);
            description.name = PassName;
            description.clearBuffer = false;
            var destination = renderGraph.CreateTexture(description);
            renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(source, destination, Material, 0), PassName);
            resources.cameraColor = destination;
        }
    }
}
