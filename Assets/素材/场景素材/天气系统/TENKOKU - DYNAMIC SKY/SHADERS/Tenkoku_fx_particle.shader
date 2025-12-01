// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TENKOKU/fx_Particle" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
}

Category {
	Tags {"Queue"="Overlay+11"}
	Blend SrcAlpha OneMinusSrcAlpha
	//Blend One One
	Cull Back
	Lighting Off
	ZWrite On
	//ZTest Always

	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
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
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			half4 frag (v2f i) : COLOR {
				fixed4 col =  2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
				col.rgb = lerp(_TintColor.rgb,col.rgb,col.a)*0.5;

				//col.rgb = lerp(col.rgb,fixed3(1,1,1),0.25);

				col.rgb = clamp(col.rgb,0.15,1.0);
				//col.rgb = _TintColor.rgb;
				clip (col.a-0.1);
				return col;
			}
			
			ENDCG 
		}
	} 		
}
}
