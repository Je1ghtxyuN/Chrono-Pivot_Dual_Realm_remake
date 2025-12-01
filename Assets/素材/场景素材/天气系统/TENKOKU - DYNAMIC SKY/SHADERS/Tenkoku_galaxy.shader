// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TENKOKU/galaxy_shader" {
Properties {
	_SIntensity ("Star Intensity", Range(0.0,1.0)) = 1.0
	_GIntensity ("Galaxy Intensity", Range(0.0,1.0)) = 1.0
	_Color ("Main Color", Color) = (1,1,1,1)
	_GTex ("Galaxy Tex", 2D) = "white" {}
	_STex ("Star Detail Tex", 2D) = "white" {} 
	_CubeTex ("Cube Tex", Cube) = "white" {}
	_perturbation ("Perturbation", Range(0.0,1.0)) = 1.0
}

SubShader {

	Tags { "Queue"="Background+1601"}
	Blend One One
	Cull Front
	Lighting Off
	ZWrite Off
	Fog {Mode Off}
	
	Offset 1,996000
	
	Pass {
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest nofog
		#include "UnityCG.cginc"

		sampler2D _GTex;
		samplerCUBE _CubeTex;

		fixed4 _Color;
		float _GIntensity, _SIntensity;
		float _tenkokuIsLinear;
		float4 _TenkokuAmbientColor;
		float _Tenkoku_AtmosphereDensity;
		float _Tenkoku_AmbientGI;

		float _useCube;
		float _useStar;
		float _useGlxy;
		float _perturbation;

		struct appdata_t {
			float4 vertex : POSITION;
			float3 texcoord : TEXCOORD0;
			float3 texcoord1 : TEXCOORD1;
			float3 normal : NORMAL;
		};

		struct v2f {
			float4 vertex : POSITION;
			float3 texcoord : TEXCOORD0;
			float3 texcoord1 : TEXCOORD1;
		};

		v2f vert (appdata_t v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.texcoord = v.texcoord;
			o.texcoord.y = 1.0-o.texcoord.y;
			o.texcoord1 = v.normal;
			return o;
		}

		fixed4 frag (v2f i) : COLOR
		{

			//coordinates
			half4 col = half4(0,0,0,1);
			
			//galaxy 2D spheremap
			if (_useCube == 0.0 && _useGlxy <= 1.0){
				half3 gtex = tex2D(_GTex, i.texcoord);
				col.rgb = lerp(half3(0,0,0),gtex.rgb * _GIntensity,_Color.a);
			}

			//galaxy cubemap
			if (_useCube == 1.0 && _useGlxy <= 1.0){
				half3 gCtex = texCUBE(_CubeTex, i.texcoord1);
				col.rgb = lerp(half3(0,0,0),gCtex.rgb * _GIntensity,_Color.a);
			}

			//gamma
			half gammaFac = lerp(2.4,1.0,_tenkokuIsLinear);
			col.rgb *= gammaFac;

			//final
			col.a = (1.0-_TenkokuAmbientColor.r);
			col.a -= lerp(0.0,1.0,_Tenkoku_AtmosphereDensity*0.25);
			col.a = saturate(col.a);
			col.rgb *= col.a;


			return col;
		}
		ENDCG 
	}
}

Fallback Off

}