// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TENKOKU/fx_Particle_Lit" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_NightFac ("Night Factor", Range(0.0,1.0)) = 0.1
	_LightFac ("Light Factor", Range(0.0,1.0)) = 1.0
	_LightningFac ("Lightning Factor", Range(0.0,1.0)) = 1.0
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	_OverBright("Overbright", Range(0.0,5.0)) = 1.0
}

Category {
	//Tags { "Queue"="Geometry+1" "IgnoreProjector"="True" "RenderType"="Opaque" }
	//Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" }
	//Blend SrcAlpha OneMinusSrcAlpha
	//Cull Off Lighting Off ZWrite Off


	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	//AlphaTest Greater .01
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off



	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_particles
			//#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;
			//float4 _TenkokuAmbientColor;
			//float _Tenkoku_Ambient;
			float4 _Tenkoku_overcastColor;
			float _Tenkoku_Ambient;
			float _Tenkoku_AmbientGI;
			float _LightFac,_LightningFac;
			float _NightFac;
			float _OverBright;

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD1;
				UNITY_FOG_COORDS(1)
				//#ifdef SOFTPARTICLES_ON
				float4 projPos : TEXCOORD2;
				//#endif
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//#ifdef SOFTPARTICLES_ON
				o.projPos = ComputeScreenPos (o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
				//#endif
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);

				o.uv = v.texcoord.xy;
				//o.uv_depth = v.texcoord.xy;

				return o;
			}

			sampler2D _Tenkoku_SkyTex;
			sampler2D_float _CameraDepthTexture;
			float _InvFade;
			float Tenkoku_LightningLightIntensity;

			fixed4 frag (v2f i) : SV_Target
			{
				//#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
				float partZ = i.projPos.z;
				float fade = saturate (_InvFade * (sceneZ-partZ));
				i.color.a *= fade;
				//#endif
				
				fixed4 origColor = i.color * tex2D(_MainTex, i.texcoord);
				fixed4 col = _TintColor * origColor;
				col.a = col.a * 3;
				UNITY_APPLY_FOG(i.fogCoord, col);

				//lit fog
				col.rgb = col.rgb * lerp(1.0,_Tenkoku_AmbientGI,_LightFac);
				col.rgb = lerp(col.rgb,tex2D(_Tenkoku_SkyTex, i.uv).rgb,0.04);
				col.rgb = col.rgb * lerp(1,0.35,_Tenkoku_overcastColor.a) * _TintColor.rgb;

				//lightning
				//col.rgb = max(col.rgb,half3(0.025,0.025,0.025)) + Tenkoku_LightningLightIntensity * 2 * _LightningFac;
				col.rgb = max(col.rgb,(origColor.rgb*_NightFac).rgb) + Tenkoku_LightningLightIntensity * 2 * _LightningFac;
				col.rgb *= _OverBright;
//col.a = max(0,col.a);
//col.rgb = max(col.rgb,half3(0.5,0.5,0.5));

//col.a = partZ*col.a;
				return col;
			}
			ENDCG 
		}
	}	
}
}
