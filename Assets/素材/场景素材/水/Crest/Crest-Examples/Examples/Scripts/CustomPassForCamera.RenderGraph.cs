// Crest Ocean System

// Copyright 2024 Wave Harmonic Ltd

#if CREST_URP
#if UNITY_2023_3_OR_NEWER

namespace Crest.Examples
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.RenderGraphModule;
    using UnityEngine.Rendering.Universal;

    partial class CustomPassForCameraBase
    {
        partial class CustomPassURP
        {
            class PassData
            {
                public UniversalCameraData cameraData;
                public RenderGraphHelper.Handle colorTargetHandle;
                public RenderGraphHelper.Handle depthTargetHandle;

                public void Init(ContextContainer frameData, IUnsafeRenderGraphBuilder builder = null)
                {
                    var resources = frameData.Get<UniversalResourceData>();
                    cameraData = frameData.Get<UniversalCameraData>();

                    if (builder == null)
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        colorTargetHandle = cameraData.renderer.cameraColorTargetHandle;
                        depthTargetHandle = cameraData.renderer.cameraDepthTargetHandle;
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    else
                    {
                        colorTargetHandle = resources.activeColorTexture;
                        depthTargetHandle = resources.activeDepthTexture;
                    }
                }
            }

            readonly PassData passData = new();

            public override void RecordRenderGraph(RenderGraph graph, ContextContainer frame)
            {
                using (var builder = graph.AddUnsafePass<PassData>(PassName, out var data))
                {
                    data.Init(frame, builder);
                    builder.AllowPassCulling(false);

                    builder.SetRenderFunc<PassData>((data, context) =>
                    {
                        var buffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
                        ExecutePass(context.GetRenderContext(), buffer, data);
                    });
                }
            }

            [System.Obsolete]
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                // This will execute in edit mode always, but if an event callback is not set to "Editor And Runtime"
                // then the entire thing will no longer work in play mode. I believe this is because it will execute
                // lots of nothing which either triggers a bug or Unity ignores it as an optimisation.
                if (!Application.isPlaying)
                {
                    return;
                }

                passData.Init(renderingData.GetFrameData());
                var buffer = CommandBufferPool.Get(PassName);
                ExecutePass(context, buffer, passData);
                context.ExecuteCommandBuffer(buffer);
                CommandBufferPool.Release(buffer);
            }
        }
    }
}

#endif
#endif
