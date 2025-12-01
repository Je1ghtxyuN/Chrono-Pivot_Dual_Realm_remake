// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

#if CREST_URP

namespace Crest
{
    using Crest.Internal;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    partial class UnderwaterEffectPassURP : ScriptableRenderPass
    {
        const string PassName = "Underwater Effect";
        const string SHADER_UNDERWATER_EFFECT = "Hidden/Crest/Underwater/Underwater Effect URP";
        static readonly int sp_TemporaryColor = Shader.PropertyToID("_TemporaryColor");

        readonly PropertyWrapperMaterial _underwaterEffectMaterial;
        RenderTargetIdentifier _colorTarget;
        RenderTargetIdentifier _depthTarget;
        RenderTargetIdentifier _depthStencilTarget = new RenderTargetIdentifier(UnderwaterRenderer.ShaderIDs.s_CrestWaterVolumeStencil, 0, CubemapFace.Unknown, -1);
        RenderTargetIdentifier _temporaryColorTarget = new RenderTargetIdentifier(sp_TemporaryColor, 0, CubemapFace.Unknown, -1);
        RenderTargetIdentifier _cameraDepthTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
        RenderTexture _depthStencil;
        RenderTexture _temporaryColor;

        bool _firstRender = true;
        Camera _camera;

        static int s_InstanceCount;
        static RenderObjectsWithoutFogPass s_ApplyFogToTransparentObjects;
        UnderwaterRenderer _underwaterRenderer;

        public UnderwaterEffectPassURP()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);
            _underwaterEffectMaterial = new PropertyWrapperMaterial(SHADER_UNDERWATER_EFFECT);
            _underwaterEffectMaterial.material.hideFlags = HideFlags.HideAndDontSave;
        }

        internal void CleanUp()
        {
            CoreUtils.Destroy(_underwaterEffectMaterial.material);
        }

        public void Enable(UnderwaterRenderer underwaterRenderer)
        {
            s_InstanceCount++;
            _underwaterRenderer = underwaterRenderer;

            if (s_ApplyFogToTransparentObjects == null)
            {
                s_ApplyFogToTransparentObjects = new RenderObjectsWithoutFogPass();
            }

            RenderPipelineManager.beginCameraRendering -= EnqueuePass;
            RenderPipelineManager.beginCameraRendering += EnqueuePass;
        }

        public void Disable()
        {
            if (--s_InstanceCount <= 0)
            {
                RenderPipelineManager.beginCameraRendering -= EnqueuePass;
            }
        }

        static void EnqueuePass(ScriptableRenderContext context, Camera camera)
        {
            var ur = UnderwaterRenderer.Get(camera);

            if (!ur || !ur.IsActive)
            {
                return;
            }

            if (!Helpers.MaskIncludesLayer(camera.cullingMask, OceanRenderer.Instance.Layer))
            {
                return;
            }

            // Enqueue the pass. This happens every frame.
            var renderer = camera.GetUniversalAdditionalCameraData().scriptableRenderer;
            renderer.EnqueuePass(ur._urpEffectPass);
            if (ur.EnableShaderAPI)
            {
                renderer.EnqueuePass(s_ApplyFogToTransparentObjects);
                s_ApplyFogToTransparentObjects._underwaterRenderer = ur;
            }
        }

#if UNITY_2023_3_OR_NEWER
        void OnSetup(CommandBuffer buffer, PassData renderingData)
#else
        public override void OnCameraSetup(CommandBuffer buffer, ref RenderingData renderingData)
#endif
        {
#if UNITY_2023_3_OR_NEWER
            _colorTarget = renderingData.colorTargetHandle.RT;
            _depthTarget = renderingData.depthTargetHandle.RT;
#elif UNITY_2022_3_OR_NEWER
            _colorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            _depthTarget = renderingData.cameraData.renderer.cameraDepthTargetHandle;
#else
            _colorTarget = renderingData.cameraData.renderer.cameraColorTarget;
            _depthTarget = renderingData.cameraData.renderer.cameraDepthTarget;
#endif
            _camera = renderingData.cameraData.camera;
        }

#if UNITY_2023_3_OR_NEWER
        void ExecutePass(ScriptableRenderContext context, CommandBuffer commandBuffer, PassData renderingData)
#else
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
#endif
        {
            var camera = renderingData.cameraData.camera;
            var cameraDescriptor = renderingData.cameraData.cameraTargetDescriptor;


            // Ensure legacy underwater fog is disabled.
            if (_firstRender)
            {
                OceanRenderer.Instance.OceanMaterial.DisableKeyword("_OLD_UNDERWATER");
            }

#if UNITY_EDITOR
            if (!UnderwaterRenderer.IsFogEnabledForEditorCamera(camera))
            {
                return;
            }
#endif

#if !UNITY_2023_3_OR_NEWER
            CommandBuffer commandBuffer = CommandBufferPool.Get("Underwater Effect");
#endif

            {
                var descriptor = cameraDescriptor;
                descriptor.msaaSamples = 1;
                _temporaryColor = RenderTexture.GetTemporary(descriptor);
                _temporaryColorTarget = new RenderTargetIdentifier(_temporaryColor, 0, CubemapFace.Unknown, -1);
            }

            if (_underwaterRenderer.UseStencilBufferOnEffect)
            {
                var descriptor = cameraDescriptor;
                descriptor.colorFormat = RenderTextureFormat.Depth;
                descriptor.depthBufferBits = 24;
                descriptor.SetMSAASamples(_camera);
                descriptor.bindMS = descriptor.msaaSamples > 1;

                _depthStencil = RenderTexture.GetTemporary(descriptor);
                _depthStencilTarget = new RenderTargetIdentifier(_depthStencil, 0, CubemapFace.Unknown, -1);
            }

            UnderwaterRenderer.UpdatePostProcessMaterial(
                _underwaterRenderer,
                _underwaterRenderer._mode,
                camera,
                _underwaterEffectMaterial,
                _underwaterRenderer._sphericalHarmonicsData,
                _underwaterRenderer._meniscus,
                _firstRender || _underwaterRenderer._copyOceanMaterialParamsEachFrame,
                _underwaterRenderer._debug._viewOceanMask,
                _underwaterRenderer._debug._viewStencil,
                _underwaterRenderer._filterOceanData,
                ref _underwaterRenderer._currentOceanMaterial,
                _underwaterRenderer.EnableShaderAPI
            );

            // Create a separate stencil buffer context by copying the depth texture.
            if (_underwaterRenderer.UseStencilBufferOnEffect)
            {
                CoreUtils.SetRenderTarget(commandBuffer, _depthStencilTarget);
                Helpers.Blit(commandBuffer, _depthStencilTarget, Helpers.UtilityMaterial, (int)Helpers.UtilityPass.CopyDepth);
            }

            // Copy color buffer.
            if (Helpers.IsMSAAEnabled(camera))
            {
                Helpers.Blit(commandBuffer, _temporaryColorTarget, Helpers.UtilityMaterial, (int)Helpers.UtilityPass.CopyColor);
            }
            else
            {
                commandBuffer.CopyTexture(_colorTarget, _temporaryColorTarget);
            }

            commandBuffer.SetGlobalTexture(UnderwaterRenderer.ShaderIDs.s_CrestCameraColorTexture, _temporaryColorTarget);

            if (_underwaterRenderer.UseStencilBufferOnEffect)
            {
                CoreUtils.SetRenderTarget(commandBuffer, _colorTarget, _depthStencilTarget);
            }
            else
            {
#if UNITY_2022_1_OR_NEWER
                CoreUtils.SetRenderTarget(commandBuffer, _colorTarget, _depthTarget);
#elif UNITY_2021_3_OR_NEWER
                // Some modes require a depth buffer but getting the depth buffer to bind depends on whether depth
                // prepass is used in addition to one of Unity's render passes being active like SSAO. Turns out that if
                // the depth target type is camera target, then we must rely on implicit binding below.
                if (_underwaterRenderer._mode != UnderwaterRenderer.Mode.FullScreen && _cameraDepthTarget != _depthTarget)
                {
                    CoreUtils.SetRenderTarget(commandBuffer, _colorTarget, _depthTarget);
                }
                else
                {
                    CoreUtils.SetRenderTarget(commandBuffer, _colorTarget);
                }
#else
                if (_underwaterRenderer._mode != UnderwaterRenderer.Mode.FullScreen && renderingData.cameraData.xrRendering)
                {
                    CoreUtils.SetRenderTarget(commandBuffer, _colorTarget, _depthTarget);
                }
                else
                {
                    CoreUtils.SetRenderTarget(commandBuffer, _colorTarget);
                }
#endif
            }

            _underwaterRenderer.ExecuteEffect(commandBuffer, _underwaterEffectMaterial.material);

            RenderTexture.ReleaseTemporary(_temporaryColor);
            RenderTexture.ReleaseTemporary(_depthStencil);

            if (_underwaterRenderer.UseStencilBufferOnEffect)
            {
                commandBuffer.ReleaseTemporaryRT(UnderwaterRenderer.ShaderIDs.s_CrestWaterVolumeStencil);
            }

#if !UNITY_2023_3_OR_NEWER
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
#endif

            _firstRender = false;
        }

        partial class RenderObjectsWithoutFogPass : ScriptableRenderPass
        {
            FilteringSettings m_FilteringSettings;
            internal UnderwaterRenderer _underwaterRenderer;

            static readonly List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>()
            {
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("LightweightForward"),
            };

            public RenderObjectsWithoutFogPass()
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
                m_FilteringSettings = new FilteringSettings(RenderQueueRange.transparent, 0);
            }

#if UNITY_2023_3_OR_NEWER
            void ExecutePass(ScriptableRenderContext context, CommandBuffer buffer, PassData renderingData)
#else
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
#endif
            {
                m_FilteringSettings.layerMask = _underwaterRenderer._transparentObjectLayers;

#if !UNITY_2023_3_OR_NEWER
                var buffer = CommandBufferPool.Get();
#endif

                // Disable Unity's fog keywords as there is no option to ignore fog for the Shader Graph.
                if (RenderSettings.fog)
                {
                    switch (RenderSettings.fogMode)
                    {
                        case FogMode.Exponential:
                            buffer.DisableShaderKeyword("FOG_EXP");
                            break;
                        case FogMode.Linear:
                            buffer.DisableShaderKeyword("FOG_LINEAR");
                            break;
                        case FogMode.ExponentialSquared:
                            buffer.DisableShaderKeyword("FOG_EXP2");
                            break;
                    }
                }

                buffer.EnableShaderKeyword("CREST_UNDERWATER_OBJECTS_PASS");
                // If we want anything to apply to DrawRenderers, it has to be executed before:
                // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.DrawRenderers.html
                context.ExecuteCommandBuffer(buffer);
                buffer.Clear();

#if UNITY_2023_3_OR_NEWER
                var drawingSettings = RenderingUtils.CreateDrawingSettings
                (
                    m_ShaderTagIdList,
                    renderingData.renderingData,
                    renderingData.cameraData,
                    renderingData.lightData,
                    SortingCriteria.CommonTransparent
                );

                var parameters = new RendererListParams(renderingData.cullResults, drawingSettings, m_FilteringSettings);
                var list = context.CreateRendererList(ref parameters);

                buffer.DrawRendererList(list);
#else
                var drawingSettings = CreateDrawingSettings
                (
                    m_ShaderTagIdList,
                    ref renderingData,
                    SortingCriteria.CommonTransparent
                );

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings);
#endif

                // Revert fog keywords.
                if (RenderSettings.fog)
                {
                    switch (RenderSettings.fogMode)
                    {
                        case FogMode.Exponential:
                            buffer.EnableShaderKeyword("FOG_EXP");
                            break;
                        case FogMode.Linear:
                            buffer.EnableShaderKeyword("FOG_LINEAR");
                            break;
                        case FogMode.ExponentialSquared:
                            buffer.EnableShaderKeyword("FOG_EXP2");
                            break;
                    }
                }

                buffer.DisableShaderKeyword("CREST_UNDERWATER_OBJECTS_PASS");

#if !UNITY_2023_3_OR_NEWER
                context.ExecuteCommandBuffer(buffer);
                CommandBufferPool.Release(buffer);
#endif
            }
        }
    }
}

#endif // CREST_URP
