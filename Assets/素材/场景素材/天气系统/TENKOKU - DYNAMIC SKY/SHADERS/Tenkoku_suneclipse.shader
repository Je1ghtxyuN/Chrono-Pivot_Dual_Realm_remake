// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TENKOKU/suneclipse_shader" {
 Properties 
 {
 _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
 _MainTex ("Base (RGB)", 2D) = "white" {}
 }
 

 
 Category {
	//Tags { "Queue"="Overlay+11" }
	//Tags {"Queue"="Background-7"}
	Tags { "Queue"="Background+1603"}
	Blend One One
	Cull Back
	ZWrite Off
	Fog {Mode Off}
	//Offset 1,981000

	//Tags { "Queue"="Background+1604"}
	Offset 1,996000

 	//Stencil {
	//	Ref 2
	//	Comp Greater
	//	Pass Replace 
	//	Fail Keep
	//	ZFail Replace
	//}

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
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _Tenkoku_EclipseFactor;
			float _Tenkoku_TotalEclipseFactor;
			float4 _TintColor;


			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				//float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			half4 frag (v2f i) : COLOR {
				half4 tex = tex2D(_MainTex, i.texcoord);
				tex.a = 1.0;//tex.r * (_TintColor.a - saturate(lerp(-5.0,1.0,_Tenkoku_EclipseFactor)));
				tex.rgb = tex.rgb * _TintColor.rgb * 2.0 * saturate(lerp(1.0,-16.0, saturate(_Tenkoku_TotalEclipseFactor * 10)));
				//tex.a = 0.0;
				return tex;// * 2.0;
			}
			ENDCG 
		}
	} 	



		
 
}
}