// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TENKOKU/fx_Particle_Lightning" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_LightFac ("LightFactor", Range(0.0,1.0)) = 1.0
	_LightningFac ("LightningFactor", Range(0.0,1.0)) = 1.0
}

Category {
	//Tags { "Queue"="Geometry+1" "IgnoreProjector"="True" "RenderType"="Opaque" }
	//Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" }
	//Blend SrcAlpha OneMinusSrcAlpha
	//Cull Off Lighting Off ZWrite Off


	Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" }
	//Tags {"Queue"="Geometry"}
	//Blend SrcAlpha OneMinusSrcAlpha
	Blend One One
	ColorMask RGBA
	Cull Off Lighting Off ZWrite Off
 	//Offset 1,70000



    //Tags {"Queue"="Background+1605"}
    //Cull Front
    //Fog {Mode Off}
    //Offset 1,80000
    //Blend SrcAlpha OneMinusSrcAlpha



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
			float _LightFac,_LightningFac;

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
				//UNITY_FOG_COORDS(1)
				//#ifdef SOFTPARTICLES_ON
				//float4 projPos : TEXCOORD2;
				//#endif
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.uv = v.texcoord.xy;
				return o;
			}

			float Tenkoku_LightningIntensity;
			float Tenkoku_LightningLightIntensity;

			fixed4 frag (v2f i) : SV_Target
			{
				
				fixed4 col = i.color * _TintColor * tex2D(_MainTex, i.texcoord);

				//lightning
				//Tenkoku_LightningIntensity = 1.0;
				//Tenkoku_LightningIntensity
				//Tenkoku_LightningLightIntensity
				col.rgb = col.rgb * _TintColor.rgb * Tenkoku_LightningLightIntensity * lerp(0.5,10,col.b);
				col.a = 1.0;//Tenkoku_LightningLightIntensity;


				//clip(col.r-0.1);

				return col;
			}
			ENDCG 
		}
	}	
}
}
