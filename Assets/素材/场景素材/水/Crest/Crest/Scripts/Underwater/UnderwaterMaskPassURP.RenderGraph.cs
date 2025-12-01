// Crest Ocean System

// Copyright 2024 Wave Harmonic Ltd

#if CREST_URP
#if UNITY_2023_3_OR_NEWER

namespace Crest
{
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.RenderGraphModule;
    using UnityEngine.Rendering.Universal;

    partial class UnderwaterMaskPassURP : ScriptableRenderPass
    {
        class PassData
        {
            public UniversalCameraData cameraData;

            public void Init(ContextContainer frameData, IUnsafeRenderGraphBuilder builder = null)
            {
                cameraData = frameData.Get<UniversalCameraData>();
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
                    OnSetup(buffer, data);
                    ExecutePass(context.GetRenderContext(), buffer, data);
                });
            }
        }

        // Called before Configure.
        [System.Obsolete]
        public override void OnCameraSetup(CommandBuffer buffer, ref RenderingData renderingData)
        {
            passData.Init(renderingData.GetFrameData());
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            passData.Init(renderingData.GetFrameData());
            var cmd = CommandBufferPool.Get(PassName);
            OnSetup(cmd, passData);
            ExecutePass(context, cmd, passData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}

#endif // UNITY_2023_3_OR_NEWER
#endif // CREST_URP
