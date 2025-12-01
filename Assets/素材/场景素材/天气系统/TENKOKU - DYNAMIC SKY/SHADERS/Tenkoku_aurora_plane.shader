Shader "TENKOKU/aurora_plane" {
	Properties {

		_Height ("Height", float) = 1.0

		_aurSpeed ("Aurora Speed", Range(0.0, 1.0)) = 0.25
		_aurLatSpeed ("Aurora Lateral Speed", Range(0.0, 1.0)) = 0.1
		_aurDir ("Aurora Direction", Range(-1.0, 1.0)) = -1.0
		_distAmt ("Distortion Amount", Range(0.0, 1.0)) = 0.1

		_overallAlpha ("Overall Alpha", Range(0.0, 1.0)) = 1.0

		_aurTint1a ("Aurora Tint1a", Color) = (1.0, 1.0, 1.0, 1.0)
		_aurTint1b ("Aurora Tint1b", Color) = (1.0, 1.0, 1.0, 1.0)
		_aurTint2a ("Aurora Tint2a", Color) = (1.0, 1.0, 1.0, 1.0)
		_aurTint2b ("Aurora Tint2b", Color) = (1.0, 1.0, 1.0, 1.0)
		_aurTint3a ("Aurora Tint3a", Color) = (1.0, 1.0, 1.0, 1.0)
		_aurTint3b ("Aurora Tint3b", Color) = (1.0, 1.0, 1.0, 1.0)

		_MainTex ("Clouds A", 2D) = "white" {}
		_DistortTex ("Normal Distortion)", 2D) = "white" {}
		_BlendTex ("Blend", 2D) = "white" {}
	}
	SubShader {





//#####  AURORA TOP  #####

	Tags { "Queue"="Transparent-1" }
		Blend One One
		Offset 1,880000
		Fog {Mode Off}
		Lighting Off

		CGPROGRAM
		#pragma surface surf AuroraLight vertex:vert alpha nofog noambient noforwardadd
		uniform sampler2D _MainTex;
		uniform sampler2D _DistortTex;
		uniform sampler2D _BlendTex;
		float4 windCoords;
		float4 _aurTint3a;
		float4 _aurTint3b;
		float _aurSpeed;
		float _aurLatSpeed;
		float _aurDir;
		float _overallAlpha;
		float _Height;
		float _distAmt;
		float _Tenkoku_AuroraAmt;
		float _Tenkoku_AuroraSpd;

		struct Input {
			//float4 screenPos;	
			float2 uv_MainTex;
			float2 uv_DistortTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingAuroraLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = lerp(fixed4(0,0,0,0),fixed4(s.Albedo*2,s.Alpha),_overallAlpha);
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.xyz -= v.normal*(17.5 * _Height);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			float3 distort = UnpackNormal(tex2D(_DistortTex, IN.uv_DistortTex + float2(_Time.x*0.1,_Time.x*0.02)));
			half4 c = tex2D(_MainTex, IN.uv_MainTex + (distort.xz * _distAmt) + (float2(0,1*_aurDir)*(_Time.x*_aurSpeed*_Tenkoku_AuroraSpd)) + float2(-_Time.x*_aurLatSpeed*_Tenkoku_AuroraSpd,0));
			half edgeBlend = tex2D(_BlendTex, IN.uv_BlendTex).r;
			half colMorph = tex2D(_BlendTex, IN.uv_BlendTex * 0.25 + float2(0,_Time.x*0.5)).a;
			o.Albedo = lerp(_aurTint3b.rgb,_aurTint3a.rgb,colMorph);
			o.Alpha = c.b * _aurTint3a.a * edgeBlend * 0.2 * _Tenkoku_AuroraAmt;
			//float4 uv0 = IN.screenPos; uv0.xy;
		}
		ENDCG



	Tags { "Queue"="Transparent-1" }
		Blend One One
		Offset 1,880000
		Fog {Mode Off}
		Lighting Off

		CGPROGRAM
		#pragma surface surf AuroraLight vertex:vert alpha nofog noambient noforwardadd
		uniform sampler2D _MainTex;
		uniform sampler2D _DistortTex;
		uniform sampler2D _BlendTex;
		float4 windCoords;
		float4 _aurTint3a;
		float4 _aurTint3b;
		float _aurSpeed;
		float _aurLatSpeed;
		float _aurDir;
		float _overallAlpha;
		float _Height;
		float _distAmt;
		float _Tenkoku_AuroraAmt;
		float _Tenkoku_AuroraSpd;
		
		struct Input {
			//float4 screenPos;	
			float2 uv_MainTex;
			float2 uv_DistortTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingAuroraLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = lerp(half4(0,0,0,0),half4(s.Albedo*2,s.Alpha),_overallAlpha);
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.xyz -= v.normal*(16.0 * _Height);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			float3 distort = UnpackNormal(tex2D(_DistortTex, IN.uv_DistortTex + float2(_Time.x*0.1,_Time.x*0.02)));
			half4 c = tex2D(_MainTex, IN.uv_MainTex + (distort.xz * _distAmt) + (float2(0,1*_aurDir)*(_Time.x*_aurSpeed*_Tenkoku_AuroraSpd)) + float2(-_Time.x*_aurLatSpeed*_Tenkoku_AuroraSpd,0));
			half edgeBlend = tex2D(_BlendTex, IN.uv_BlendTex).r;
			half colMorph = tex2D(_BlendTex, IN.uv_BlendTex * 0.25 + float2(0,_Time.x*0.5)).a;
			o.Albedo = lerp(_aurTint3b.rgb,_aurTint3a.rgb,colMorph);
			o.Alpha = c.b * _aurTint3a.a * edgeBlend * 0.2 * _Tenkoku_AuroraAmt;
			//float4 uv0 = IN.screenPos; uv0.xy;
		}
		ENDCG


	Tags { "Queue"="Transparent-1" }
		Blend One One
		Offset 1,880000
		Fog {Mode Off}
		Lighting Off
		
		CGPROGRAM
		#pragma surface surf AuroraLight vertex:vert alpha nofog noambient noforwardadd
		uniform sampler2D _MainTex;
		uniform sampler2D _DistortTex;
		uniform sampler2D _BlendTex;
		float4 windCoords;
		float4 _aurTint3a;
		float4 _aurTint3b;
		float _aurSpeed;
		float _aurLatSpeed;
		float _aurDir;
		float _overallAlpha;
		float _Height;
		float _distAmt;
		float _Tenkoku_AuroraAmt;
		float _Tenkoku_AuroraSpd;
		
		struct Input {
			//float4 screenPos;	
			float2 uv_MainTex;
			float2 uv_DistortTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingAuroraLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = lerp(half4(0,0,0,0),half4(s.Albedo*2,s.Alpha),_overallAlpha);
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.xyz -= v.normal*(14.5 * _Height);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			float3 distort = UnpackNormal(tex2D(_DistortTex, IN.uv_DistortTex + float2(_Time.x*0.1,_Time.x*0.02)));
			half4 c = tex2D(_MainTex, IN.uv_MainTex + (distort.xz * _distAmt) + (float2(0,1*_aurDir)*(_Time.x*_aurSpeed*_Tenkoku_AuroraSpd)) + float2(-_Time.x*_aurLatSpeed*_Tenkoku_AuroraSpd,0));
			half edgeBlend = tex2D(_BlendTex, IN.uv_BlendTex).r;
			half colMorph = tex2D(_BlendTex, IN.uv_BlendTex * 0.25 + float2(0,_Time.x*0.5)).a;
			o.Albedo = lerp(_aurTint3b.rgb,_aurTint3a.rgb,colMorph);
			o.Alpha = c.b * _aurTint3a.a * edgeBlend * 0.3 * _Tenkoku_AuroraAmt;
			//float4 uv0 = IN.screenPos; uv0.xy;
		}
		ENDCG


	Tags { "Queue"="Transparent-1" }
		Blend One One
		Offset 1,880000
		Fog {Mode Off}
		Lighting Off
		
		CGPROGRAM
		#pragma surface surf AuroraLight vertex:vert alpha nofog noambient noforwardadd
		uniform sampler2D _MainTex;
		uniform sampler2D _DistortTex;
		uniform sampler2D _BlendTex;
		float4 windCoords;
		float4 _aurTint3a;
		float4 _aurTint3b;
		float _aurSpeed;
		float _aurLatSpeed;
		float _aurDir;
		float _overallAlpha;
		float _Height;
		float _distAmt;
		float _Tenkoku_AuroraAmt;
		float _Tenkoku_AuroraSpd;
		
		struct Input {
			//float4 screenPos;	
			float2 uv_MainTex;
			float2 uv_DistortTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingAuroraLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = lerp(half4(0,0,0,0),half4(s.Albedo*2,s.Alpha),_overallAlpha);
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.xyz -= v.normal*(13.0 * _Height);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			float3 distort = UnpackNormal(tex2D(_DistortTex, IN.uv_DistortTex + float2(_Time.x*0.1,_Time.x*0.02)));
			half4 c = tex2D(_MainTex, IN.uv_MainTex + (distort.xz * _distAmt) + (float2(0,1*_aurDir)*(_Time.x*_aurSpeed*_Tenkoku_AuroraSpd)) + float2(-_Time.x*_aurLatSpeed*_Tenkoku_AuroraSpd,0));
			half edgeBlend = tex2D(_BlendTex, IN.uv_BlendTex).r;
			half colMorph = tex2D(_BlendTex, IN.uv_BlendTex * 0.25 + float2(0,_Time.x*0.5)).a;
			o.Albedo = lerp(_aurTint3b.rgb,_aurTint3a.rgb,colMorph);
			o.Alpha = c.b * _aurTint3a.a * edgeBlend * 0.4 * _Tenkoku_AuroraAmt;
			//float4 uv0 = IN.screenPos; uv0.xy;
		}
		ENDCG


	Tags { "Queue"="Transparent-1" }
		Blend One One
		Offset 1,880000
		Fog {Mode Off}
		Lighting Off
		
		CGPROGRAM
		#pragma surface surf AuroraLight vertex:vert alpha nofog noambient noforwardadd
		uniform sampler2D _MainTex;
		uniform sampler2D _DistortTex;
		uniform sampler2D _BlendTex;
		float4 windCoords;
		float4 _aurTint3a;
		float4 _aurTint3b;
		float _aurSpeed;
		float _aurLatSpeed;
		float _aurDir;
		float _overallAlpha;
		float _Height;
		float _distAmt;
		float _Tenkoku_AuroraAmt;
		float _Tenkoku_AuroraSpd;
		
		struct Input {
			//float4 screenPos;	
			float2 uv_MainTex;
			float2 uv_DistortTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingAuroraLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = lerp(half4(0,0,0,0),half4(s.Albedo*2,s.Alpha),_overallAlpha);
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.xyz -= v.normal*(11.5 * _Height);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			float3 distort = UnpackNormal(tex2D(_DistortTex, IN.uv_DistortTex + float2(_Time.x*0.1,_Time.x*0.02)));
			half4 c = tex2D(_MainTex, IN.uv_MainTex + (distort.xz * _distAmt) + (float2(0,1*_aurDir)*(_Time.x*_aurSpeed*_Tenkoku_AuroraSpd)) + float2(-_Time.x*_aurLatSpeed*_Tenkoku_AuroraSpd,0));
			half edgeBlend = tex2D(_BlendTex, IN.uv_BlendTex).r;
			half colMorph = tex2D(_BlendTex, IN.uv_BlendTex * 0.25 + float2(0,_Time.x*0.5)).a;
			o.Albedo = lerp(_aurTint3b.rgb,_aurTint3a.rgb,colMorph);
			o.Alpha = c.b * _aurTint3a.a * edgeBlend * 0.5 * _Tenkoku_AuroraAmt;
			//float4 uv0 = IN.screenPos; uv0.xy;
		}
		ENDCG



	Tags { "Queue"="Transparent-1" }
		Blend One One
		Offset 1,880000
		Fog {Mode Off}
		Lighting Off
		
		CGPROGRAM
		#pragma surface surf AuroraLight vertex:vert alpha nofog noambient noforwardadd
		uniform sampler2D _MainTex;
		uniform sampler2D _DistortTex;
		uniform sampler2D _BlendTex;
		float4 windCoords;
		float4 _aurTint3a;
		float4 _aurTint3b;
		float _aurSpeed;
		float _aurLatSpeed;
		float _aurDir;
		float _overallAlpha;
		float _Height;
		float _distAmt;
		float _Tenkoku_AuroraAmt;
		float _Tenkoku_AuroraSpd;
		
		struct Input {
			//float4 screenPos;	
			float2 uv_MainTex;
			float2 uv_DistortTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingAuroraLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = lerp(half4(0,0,0,0),half4(s.Albedo*2,s.Alpha),_overallAlpha);
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.xyz -= v.normal*(10.0 * _Height);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			float3 distort = UnpackNormal(tex2D(_DistortTex, IN.uv_DistortTex + float2(_Time.x*0.1,_Time.x*0.02)));
			half4 c = tex2D(_MainTex, IN.uv_MainTex + (distort.xz * _distAmt) + (float2(0,1*_aurDir)*(_Time.x*_aurSpeed*_Tenkoku_AuroraSpd)) + float2(-_Time.x*_aurLatSpeed*_Tenkoku_AuroraSpd,0));
			half edgeBlend = tex2D(_BlendTex, IN.uv_BlendTex).r;
			half colMorph = tex2D(_BlendTex, IN.uv_BlendTex * 0.25 + float2(0,_Time.x*0.5)).a;
			o.Albedo = lerp(_aurTint3b.rgb,_aurTint3a.rgb,colMorph);
			o.Alpha = c.b * _aurTint3a.a * edgeBlend * 0.7 * _Tenkoku_AuroraAmt;
			//float4 uv0 = IN.screenPos; uv0.xy;
		}
		ENDCG

//##### END  #####
		





 


//#####  AURORA MIDDLE  #####

		Tags { "Queue"="Transparent-1" }
		Blend One One
		Offset 1,880000
		Fog {Mode Off}
		Lighting Off
		
		CGPROGRAM
		#pragma surface surf AuroraLight vertex:vert alpha nofog noambient noforwardadd
		uniform sampler2D _MainTex;
		uniform sampler2D _DistortTex;
		uniform sampler2D _BlendTex;
		float4 windCoords;
		float4 _aurTint2a;
		float4 _aurTint2b;
		float _aurSpeed;
		float _aurLatSpeed;
		float _aurDir;
		float _overallAlpha;
		float _Height;
		float _distAmt;
		float _Tenkoku_AuroraAmt;
		float _Tenkoku_AuroraSpd;
		
		struct Input {
			//float4 screenPos;	
			float2 uv_MainTex;
			float2 uv_DistortTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingAuroraLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = lerp(half4(0,0,0,0),half4(s.Albedo*2,s.Alpha),_overallAlpha);
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.xyz -= v.normal*(11.5 * _Height);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			float3 distort = UnpackNormal(tex2D(_DistortTex, IN.uv_DistortTex + float2(_Time.x*0.1,_Time.x*0.02)));
			half4 c = tex2D(_MainTex, IN.uv_MainTex + (distort.xz * _distAmt) + (float2(0,1*_aurDir)*(_Time.x*_aurSpeed*_Tenkoku_AuroraSpd)) + float2(-_Time.x*_aurLatSpeed*_Tenkoku_AuroraSpd,0));
			half edgeBlend = tex2D(_BlendTex, IN.uv_BlendTex).r;
			half colMorph = tex2D(_BlendTex, IN.uv_BlendTex * 0.25 + float2(0,_Time.x*0.5)).a;
			o.Albedo = lerp(_aurTint2b.rgb,_aurTint2a.rgb,colMorph);
			o.Alpha = c.g * _aurTint2a.a * edgeBlend * 0.2 * _Tenkoku_AuroraAmt;
			//float4 uv0 = IN.screenPos; uv0.xy;
		}
		ENDCG



		Tags { "Queue"="Transparent-1" }
		Blend One One
		Offset 1,880000
		Fog {Mode Off}
		Lighting Off
		
		CGPROGRAM
		#pragma surface surf AuroraLight vertex:vert alpha nofog noambient noforwardadd
		uniform sampler2D _MainTex;
		uniform sampler2D _DistortTex;
		uniform sampler2D _BlendTex;
		float4 windCoords;
		float4 _aurTint2a;
		float4 _aurTint2b;
		float _aurSpeed;
		float _aurLatSpeed;
		float _aurDir;
		float _overallAlpha;
		float _Height;
		float _distAmt;
		float _Tenkoku_AuroraAmt;
		float _Tenkoku_AuroraSpd;
		
		struct Input {
			//float4 screenPos;	
			float2 uv_MainTex;
			float2 uv_DistortTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingAuroraLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = lerp(half4(0,0,0,0),half4(s.Albedo*2,s.Alpha),_overallAlpha);
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.xyz -= v.normal*(9.5 * _Height);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			float3 distort = UnpackNormal(tex2D(_DistortTex, IN.uv_DistortTex + float2(_Time.x*0.1,_Time.x*0.02)));
			half4 c = tex2D(_MainTex, IN.uv_MainTex + (distort.xz * _distAmt) + (float2(0,1*_aurDir)*(_Time.x*_aurSpeed*_Tenkoku_AuroraSpd)) + float2(-_Time.x*_aurLatSpeed*_Tenkoku_AuroraSpd,0));
			half edgeBlend = tex2D(_BlendTex, IN.uv_BlendTex).r;
			half colMorph = tex2D(_BlendTex, IN.uv_BlendTex * 0.25 + float2(0,_Time.x*0.5)).a;
			o.Albedo = lerp(_aurTint2b.rgb,_aurTint2a.rgb,colMorph);
			o.Alpha = c.g * _aurTint2a.a * edgeBlend * 0.3 * _Tenkoku_AuroraAmt;
			//float4 uv0 = IN.screenPos; uv0.xy;
		}
		ENDCG



		Tags { "Queue"="Transparent-1" }
		Blend One One
		Offset 1,880000
		Fog {Mode Off}
		Lighting Off
		
		CGPROGRAM
		#pragma surface surf AuroraLight vertex:vert alpha nofog noambient noforwardadd
		uniform sampler2D _MainTex;
		uniform sampler2D _DistortTex;
		uniform sampler2D _BlendTex;
		float4 windCoords;
		float4 _aurTint2a;
		float4 _aurTint2b;
		float _aurSpeed;
		float _aurLatSpeed;
		float _aurDir;
		float _overallAlpha;
		float _Height;
		float _distAmt;
		float _Tenkoku_AuroraAmt;
		float _Tenkoku_AuroraSpd;
		
		struct Input {
			//float4 screenPos;	
			float2 uv_MainTex;
			float2 uv_DistortTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingAuroraLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = lerp(half4(0,0,0,0),half4(s.Albedo*2,s.Alpha),_overallAlpha);
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.xyz -= v.normal*(7.5 * _Height);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			float3 distort = UnpackNormal(tex2D(_DistortTex, IN.uv_DistortTex + float2(_Time.x*0.1,_Time.x*0.02)));
			half4 c = tex2D(_MainTex, IN.uv_MainTex + (distort.xz * _distAmt) + (float2(0,1*_aurDir)*(_Time.x*_aurSpeed*_Tenkoku_AuroraSpd)) + float2(-_Time.x*_aurLatSpeed*_Tenkoku_AuroraSpd,0));
			half edgeBlend = tex2D(_BlendTex, IN.uv_BlendTex).r;
			half colMorph = tex2D(_BlendTex, IN.uv_BlendTex * 0.25 + float2(0,_Time.x*0.5)).a;
			o.Albedo = lerp(_aurTint2b.rgb,_aurTint2a.rgb,colMorph);
			o.Alpha = c.g * _aurTint2a.a * edgeBlend * 0.3 * _Tenkoku_AuroraAmt;
			//float4 uv0 = IN.screenPos; uv0.xy;
		}
		ENDCG


		Tags { "Queue"="Transparent-1" }
		Blend One One
		Offset 1,880000
		Fog {Mode Off}
		Lighting Off
		
		CGPROGRAM
		#pragma surface surf AuroraLight vertex:vert alpha nofog noambient noforwardadd
		uniform sampler2D _MainTex;
		uniform sampler2D _DistortTex;
		uniform sampler2D _BlendTex;
		float4 windCoords;
		float4 _aurTint2a;
		float4 _aurTint2b;
		float _aurSpeed;
		float _aurLatSpeed;
		float _aurDir;
		float _overallAlpha;
		float _Height;
		float _distAmt;
		float _Tenkoku_AuroraAmt;
		float _Tenkoku_AuroraSpd;
		
		struct Input {
			//float4 screenPos;	
			float2 uv_MainTex;
			float2 uv_DistortTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingAuroraLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = lerp(half4(0,0,0,0),half4(s.Albedo*2,s.Alpha),_overallAlpha);
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.xyz -= v.normal*(5.5 * _Height);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			float3 distort = UnpackNormal(tex2D(_DistortTex, IN.uv_DistortTex + float2(_Time.x*0.1,_Time.x*0.02)));
			half4 c = tex2D(_MainTex, IN.uv_MainTex + (distort.xz * _distAmt) + (float2(0,1*_aurDir)*(_Time.x*_aurSpeed*_Tenkoku_AuroraSpd)) + float2(-_Time.x*_aurLatSpeed*_Tenkoku_AuroraSpd,0));
			half edgeBlend = tex2D(_BlendTex, IN.uv_BlendTex).r;
			half colMorph = tex2D(_BlendTex, IN.uv_BlendTex * 0.25 + float2(0,_Time.x*0.5)).a;
			o.Albedo = lerp(_aurTint2b.rgb,_aurTint2a.rgb,colMorph);
			o.Alpha = c.g * _aurTint2a.a * edgeBlend * 0.4 * _Tenkoku_AuroraAmt;
			//float4 uv0 = IN.screenPos; uv0.xy;
		}
		ENDCG



		Tags { "Queue"="Transparent-1" }
		Blend One One
		Offset 1,880000
		Fog {Mode Off}
		Lighting Off
		
		CGPROGRAM
		#pragma surface surf AuroraLight vertex:vert alpha nofog noambient noforwardadd
		uniform sampler2D _MainTex;
		uniform sampler2D _DistortTex;
		uniform sampler2D _BlendTex;
		float4 windCoords;
		float4 _aurTint2a;
		float4 _aurTint2b;
		float _aurSpeed;
		float _aurLatSpeed;
		float _aurDir;
		float _overallAlpha;
		float _Height;
		float _distAmt;
		float _Tenkoku_AuroraAmt;
		float _Tenkoku_AuroraSpd;
		
		struct Input {
			//float4 screenPos;	
			float2 uv_MainTex;
			float2 uv_DistortTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingAuroraLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = lerp(half4(0,0,0,0),half4(s.Albedo*2,s.Alpha),_overallAlpha);
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.xyz -= v.normal*(3.5 * _Height);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			float3 distort = UnpackNormal(tex2D(_DistortTex, IN.uv_DistortTex + float2(_Time.x*0.1,_Time.x*0.02)));
			half4 c = tex2D(_MainTex, IN.uv_MainTex + (distort.xz * _distAmt) + (float2(0,1*_aurDir)*(_Time.x*_aurSpeed*_Tenkoku_AuroraSpd)) + float2(-_Time.x*_aurLatSpeed*_Tenkoku_AuroraSpd,0));
			half edgeBlend = tex2D(_BlendTex, IN.uv_BlendTex).r;
			half colMorph = tex2D(_BlendTex, IN.uv_BlendTex * 0.25 + float2(0,_Time.x*0.5)).a;
			o.Albedo = lerp(_aurTint2b.rgb,_aurTint2a.rgb,colMorph);
			o.Alpha = c.g * _aurTint2a.a * edgeBlend * 0.7 * _Tenkoku_AuroraAmt;
			//float4 uv0 = IN.screenPos; uv0.xy;
		}
		ENDCG
//##### END  #####






//#####  AURORA BOTTOM  #####


		Tags { "Queue"="Transparent-1" }
		Blend One One
		Offset 1,880000
		Fog {Mode Off}
		Lighting Off
		
		CGPROGRAM
		#pragma surface surf AuroraLight vertex:vert alpha nofog noambient noforwardadd
		uniform sampler2D _MainTex;
		uniform sampler2D _DistortTex;
		uniform sampler2D _BlendTex;
		float4 windCoords;
		float4 _aurTint1a;
		float4 _aurTint1b;
		float _aurSpeed;
		float _aurLatSpeed;
		float _aurDir;
		float _overallAlpha;
		float _Height;
		float _distAmt;
		float _Tenkoku_AuroraAmt;
		float _Tenkoku_AuroraSpd;
		
		struct Input {
			//float4 screenPos;	
			float2 uv_MainTex;
			float2 uv_DistortTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingAuroraLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = lerp(half4(0,0,0,0),half4(s.Albedo*2,s.Alpha),_overallAlpha);
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.xyz -= v.normal*(2.5 * _Height);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			float3 distort = UnpackNormal(tex2D(_DistortTex, IN.uv_DistortTex + float2(_Time.x*0.1,_Time.x*0.02)));
			half4 c = tex2D(_MainTex, IN.uv_MainTex + (distort.xz * _distAmt) + (float2(0,1*_aurDir)*(_Time.x*_aurSpeed*_Tenkoku_AuroraSpd)) + float2(-_Time.x*_aurLatSpeed*_Tenkoku_AuroraSpd,0));
			half edgeBlend = tex2D(_BlendTex, IN.uv_BlendTex).r;
			half colMorph = tex2D(_BlendTex, IN.uv_BlendTex * 0.25 + float2(0,_Time.x*0.5)).a;
			o.Albedo = lerp(_aurTint1b.rgb,_aurTint1a.rgb,colMorph);
			o.Alpha = c.r * _aurTint1a.a * edgeBlend * 0.2 * _Tenkoku_AuroraAmt;
			//float4 uv0 = IN.screenPos; uv0.xy;
		}
		ENDCG



		Tags { "Queue"="Transparent-1" }
		Blend One One
		Offset 1,880000
		Fog {Mode Off}
		Lighting Off
		
		CGPROGRAM
		#pragma surface surf AuroraLight vertex:vert alpha nofog noambient noforwardadd
		uniform sampler2D _MainTex;
		uniform sampler2D _DistortTex;
		uniform sampler2D _BlendTex;
		float4 windCoords;
		float4 _aurTint1a;
		float4 _aurTint1b;
		float _aurSpeed;
		float _aurLatSpeed;
		float _aurDir;
		float _overallAlpha;
		float _Height;
		float _distAmt;
		float _Tenkoku_AuroraAmt;
		float _Tenkoku_AuroraSpd;
		
		struct Input {
			//float4 screenPos;	
			float2 uv_MainTex;
			float2 uv_DistortTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingAuroraLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = lerp(half4(0,0,0,0),half4(s.Albedo*2,s.Alpha),_overallAlpha);
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.xyz -= v.normal*(1.25 * _Height);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			float3 distort = UnpackNormal(tex2D(_DistortTex, IN.uv_DistortTex + float2(_Time.x*0.1,_Time.x*0.02)));
			half4 c = tex2D(_MainTex, IN.uv_MainTex + (distort.xz * _distAmt) + (float2(0,1*_aurDir)*(_Time.x*_aurSpeed*_Tenkoku_AuroraSpd)) + float2(-_Time.x*_aurLatSpeed*_Tenkoku_AuroraSpd,0));
			half edgeBlend = tex2D(_BlendTex, IN.uv_BlendTex).r;
			half colMorph = tex2D(_BlendTex, IN.uv_BlendTex * 0.25 + float2(0,_Time.x*0.5)).a;
			o.Albedo = lerp(_aurTint1b.rgb,_aurTint1a.rgb,colMorph);
			o.Alpha = c.r * _aurTint1a.a * edgeBlend * 0.5 * _Tenkoku_AuroraAmt;
			//float4 uv0 = IN.screenPos; uv0.xy;
		}
		ENDCG


		Tags { "Queue"="Transparent-1" }
		Blend One One
		Offset 1,880000
		Fog {Mode Off}
		Lighting Off
		
		CGPROGRAM
		#pragma surface surf AuroraLight vertex:vert alpha nofog noambient noforwardadd
		uniform sampler2D _MainTex;
		uniform sampler2D _DistortTex;
		uniform sampler2D _BlendTex;
		float4 windCoords;
		float4 _aurTint1a;
		float4 _aurTint1b;
		float _aurSpeed;
		float _aurLatSpeed;
		float _aurDir;
		float _overallAlpha;
		float _Height;
		float _distAmt;
		float _Tenkoku_AuroraAmt;
		float _Tenkoku_AuroraSpd;
		
		struct Input {
			//float4 screenPos;	
			float2 uv_MainTex;
			float2 uv_DistortTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingAuroraLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = lerp(half4(0,0,0,0),half4(s.Albedo*2,s.Alpha),_overallAlpha);
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.xyz -= v.normal*(0.0 * _Height);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			float3 distort = UnpackNormal(tex2D(_DistortTex, IN.uv_DistortTex + float2(_Time.x*0.1,_Time.x*0.02)));
			half4 c = tex2D(_MainTex, IN.uv_MainTex + (distort.xz * _distAmt) + (float2(0,1*_aurDir)*(_Time.x*_aurSpeed*_Tenkoku_AuroraSpd)) + float2(-_Time.x*_aurLatSpeed*_Tenkoku_AuroraSpd,0));
			half edgeBlend = tex2D(_BlendTex, IN.uv_BlendTex).r;
			half colMorph = tex2D(_BlendTex, IN.uv_BlendTex * 0.25 + float2(0,_Time.x*0.5)).a;
			o.Albedo = lerp(_aurTint1b.rgb,_aurTint1a.rgb,colMorph);
			o.Alpha = c.r * _aurTint1a.a * edgeBlend * 0.7 * _Tenkoku_AuroraAmt;
			//float4 uv0 = IN.screenPos; uv0.xy;
		}
		ENDCG

//##### END  #####




	} 
}
