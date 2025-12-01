// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TENKOKU/planet_shader" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
}

Category {
	//Tags { "Queue"="Overlay+8" }
	//Tags {"Queue"="Background-5"}
Tags {"Queue"="Background+1603"}
	//Blend SrcAlpha OneMinusSrcAlpha
	AlphaTest Greater .03
	
	//Blend OneMinusDstColor SrcAlpha
	
	//Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
	Blend SrcAlpha One
	//Blend One One
	//Blend One One // Additive
	//Blend OneMinusDstColor One // Soft Additive
	//Blend DstColor Zero // Multiplicative
	
	//BlendOp Sub
    //Blend SrcAlpha One
	
	//BlendOp Max

	//ColorMask B
	
	Cull Back Lighting On Fog {Mode Off}
	
	//ZTest Less 
	//ZTest Greater
	ZWrite Off
	Offset 1,960500

	
	Stencil {
		Ref 2
		Comp Greater
		Pass Replace 
		Fail Keep
		ZFail Replace
	}
	

	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest nofog
			//#pragma multi_compile_particles

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _TintColor;
			
			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				//#ifdef SOFTPARTICLES_ON
				//float4 projPos : TEXCOORD1;
				//#endif
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//#ifdef SOFTPARTICLES_ON
				//o.projPos = ComputeScreenPos (o.vertex);
				//COMPUTE_EYEDEPTH(o.projPos.z);
				//#endif
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			sampler2D _CameraDepthTexture;
			float _InvFade;
			
			half4 frag (v2f i) : SV_Target
			{
				
				//#ifdef SOFTPARTICLES_ON
				//float sceneZ = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r);
				//float partZ = i.projPos.x;
				//float fade = saturate (sceneZ);
				//i.color.a = saturate(fade*0.1);
				//#endif
				
				i.color *= 3.0 * _TintColor * tex2D(_MainTex, i.texcoord);

				return i.color;
				
			}
			ENDCG 
		}
	} 	
	
}
}
