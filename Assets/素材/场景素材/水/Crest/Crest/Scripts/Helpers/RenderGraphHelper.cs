// Crest Ocean System

// Copyright 2024 Wave Harmonic Ltd

#if CREST_URP
#if UNITY_2023_3_OR_NEWER

using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Crest
{
    public static class RenderGraphHelper
    {
        public struct Handle
        {
            RTHandle rtHandle;
            TextureHandle textureHandle;

            public readonly RTHandle RT { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => rtHandle ?? textureHandle; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Handle(RTHandle handle) => new() { rtHandle = handle };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Handle(TextureHandle handle) => new() { textureHandle = handle };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator RTHandle(Handle texture) => texture.RT;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator TextureHandle(Handle texture) => texture.textureHandle;
        }

        static readonly FieldInfo s_RenderContext = typeof(InternalRenderGraphContext).GetField("renderContext", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo s_WrappedContext = typeof(UnsafeGraphContext).GetField("wrappedContext", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo s_FrameData = typeof(RenderingData).GetField("frameData", BindingFlags.NonPublic | BindingFlags.Instance);

        public static ScriptableRenderContext GetRenderContext(this UnsafeGraphContext unsafeContext)
        {
            return (ScriptableRenderContext)s_RenderContext.GetValue((InternalRenderGraphContext)s_WrappedContext.GetValue(unsafeContext));
        }

        public static ContextContainer GetFrameData(this ref RenderingData renderingData)
        {
            return (ContextContainer)s_FrameData.GetValue(renderingData);
        }
    }
}

#endif // UNITY_2023_3_OR_NEWER
#endif // CREST_URP
