// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TENKOKU/star_shader" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
}

Category {

	//Tags {"Queue"="Background-24"}
	Tags {"Queue"="Background+1602"}
	//Blend SrcAlpha OneMinusSrcAlpha
	Blend One One


	Cull Off
	Lighting Off
	ZWrite Off
	Fog {Mode Off}
	Colormask RGB
	Offset 1,995500


	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma fragmentoption ARB_precision_hint_fastest nofog
			//alpha
			//#pragma multi_compile_particles

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _TintColor;
			float _tenkokuIsLinear;
			float4 _TenkokuAmbientColor;
			float _Tenkoku_AtmosphereDensity;
			float _Tenkoku_Ambient;
			float starTypeIndex;



			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			//sampler2D _CameraDepthTexture;
			//float _InvFade;
			
			half4 frag (v2f i) : SV_Target
			{
				
				fixed4 col;
				col = i.color * 3.0 * _TintColor * tex2D(_MainTex, i.texcoord);
				col.rgb *= col.a;
				//col.a = 5.0;//starIntensity * starTypeIndex;//5.0;

				half gammaFac = lerp(2.2,1.0,_tenkokuIsLinear);
				col *= gammaFac;


				//col.rgb = i.color;
				col = saturate(col);
				//col.a *= (1.0-_TenkokuAmbientColor.r);
				//col.a -= lerp(0.0,1.0,_Tenkoku_AtmosphereDensity*0.25);
				//col.a = saturate(col.a);

col.rgb = col.rgb * (1.0 - saturate(_Tenkoku_Ambient*4));
col.a = 1.0;

//clip(col.a - 0.2);
				return col;
				
			}
			ENDCG 
		}
	} 	
	
}
}
