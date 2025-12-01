// Crest Ocean System

// Copyright 2024 Wave Harmonic Ltd

#if CREST_URP

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Crest
{
    partial class OceanRenderer
    {
        class UniversalRegisterRendererRequirements : ScriptableRenderPass
        {
            public static UniversalRegisterRendererRequirements Instance { get; set; }

            public UniversalRegisterRendererRequirements()
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
                ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);
            }

            public static void Enable()
            {
                Instance = new UniversalRegisterRendererRequirements();
                RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
                RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
                RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
                RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            }

            public static void Disable()
            {
                RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
                RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            }

            static void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
            {
                if (!Helpers.MaskIncludesLayer(camera.cullingMask, OceanRenderer.Instance.Layer))
                {
                    return;
                }

                if (!OceanRenderer.Instance.OceanMaterial.IsKeywordEnabled("_TRANSPARENCY_ON"))
                {
                    return;
                }

                camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(Instance);
            }

            static void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
            {
                if (!Helpers.MaskIncludesLayer(camera.cullingMask, OceanRenderer.Instance.Layer))
                {
                    return;
                }
            }

#if UNITY_2023_3_OR_NEWER
            class PassData { }

            public override void RecordRenderGraph(UnityEngine.Rendering.RenderGraphModule.RenderGraph graph, ContextContainer frame)
            {
                using (var builder = graph.AddUnsafePass<PassData>("PassName", out var data))
                {
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc<PassData>((data, context) => { });
                }
            }

            [System.Obsolete]
#endif
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                // Blank
            }
        }
    }
}

#endif
