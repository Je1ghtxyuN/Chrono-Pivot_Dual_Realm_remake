
// Crest Ocean System

// Copyright 2022 Wave Harmonic Ltd

namespace Crest.Examples
{
    using UnityEngine;
    using UnityEngine.Rendering;

    /// <summary>
    /// Loads a Render Pipeline Asset when entering play mode. Useful if wanting to enforce quality or features for a
    /// scene.
    /// </summary>
    public class LoadRenderPipelineAsset : MonoBehaviour
    {
        [SerializeField]
        RenderPipelineAsset _renderPipelineAsset;

        RenderPipelineAsset _oldRenderPipelineAssetGraphics;
        RenderPipelineAsset _oldRenderPipelineAssetQuality;

        void OnEnable()
        {
            _oldRenderPipelineAssetGraphics = GraphicsSettings.defaultRenderPipeline;
            _oldRenderPipelineAssetQuality = QualitySettings.renderPipeline;

            if (_renderPipelineAsset == null)
            {
                return;
            }

            GraphicsSettings.defaultRenderPipeline = _renderPipelineAsset;
            QualitySettings.renderPipeline = _renderPipelineAsset;
        }

        void OnDisable()
        {
            if (_renderPipelineAsset == null)
            {
                return;
            }

            GraphicsSettings.defaultRenderPipeline = _oldRenderPipelineAssetGraphics;
            QualitySettings.renderPipeline = _oldRenderPipelineAssetQuality;
        }
    }
}
