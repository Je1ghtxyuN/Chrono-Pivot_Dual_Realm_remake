// Crest Ocean System

// Copyright 2022 Wave Harmonic Ltd

Shader "Hidden/Crest/SphereGradientBackground"
{
	Properties
	{
		_ColorTowardsSun("_ColorTowardsSun", Color) = (1, 1, 1)
		_ColorAwayFromSun("_ColorAwayFromSun", Color) = (1, 1, 1)
		_Exponent("_Exponent", Float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode"="UniversalForward" }
		LOD 100

		Pass
		{
			Cull Front
			Blend Off

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			struct Attributes
			{
				float3 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 vertex : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings vert (Attributes v)
			{
				Varyings o;
				ZERO_INITIALIZE(Varyings, o);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = TransformObjectToHClip(v.vertex);
				o.positionWS = TransformObjectToWorld(v.vertex);
				return o;
			}

			float3 _ColorTowardsSun;
			float3 _ColorAwayFromSun;
			float _Exponent;

			float4 frag (Varyings i) : SV_Target
			{
				float3 worldPosition = i.positionWS;
				float3 viewDirection = normalize(i.positionWS - _WorldSpaceCameraPos);

				Light light = GetMainLight();
				const real3 lightDir = light.direction;
				const real3 lightCol = light.color * light.distanceAttenuation;

				float alpha = saturate(0.5 * dot(viewDirection, lightDir) + 0.5);
				alpha = pow(alpha, _Exponent);

				float3 col = lerp(_ColorAwayFromSun, _ColorTowardsSun, alpha);
				return float4(col * saturate(max(lightCol.r, max(lightCol.g, lightCol.b))), 1.0);
			}
			ENDHLSL
		}
	}
}
