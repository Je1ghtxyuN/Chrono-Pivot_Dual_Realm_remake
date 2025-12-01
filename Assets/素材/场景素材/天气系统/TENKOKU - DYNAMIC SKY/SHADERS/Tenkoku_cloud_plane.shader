// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObject2ClipPos(*)'

Shader "TENKOKU/cloud_plane" {
	Properties {
		_dist ("Distance", float) = 500.0

		_brightMult ("Brightness", float) = 1.0
		_cloudHeight ("Cloud Height", float) = 1.0
		//_cloudSpeed ("Cloud Speed", float) = 1.0
		_sizeCloud ("Cloud Size", Range(0.0, 1.0)) = 1.0

		_amtCloudS ("Cloud Stratus", Range(0.0, 1.0)) = 1.0
		_amtCloudC ("Cloud Cirrus", Range(0.0, 1.0)) = 1.0
		_amtCloudM ("Cloud Cumulus", Range(0.0, 1.0)) = 1.0
		_amtCloudO ("Cloud Overcast", Range(0.0, 1.0)) = 1.0

		_clpCloud ("Cloud Clip", Range(0.0, 1.0)) = 1.0

		_colTint ("Cloud Tint", Color) = (1.0, 1.0, 1.0, 1.0)

		_colCloudS ("Cloud Stratus Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_colCloudC ("Cloud Cirrus Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_colCloud ("Cloud Cumulus Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_colCloudO ("Cloud Overcast Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex ("Clouds A", 2D) = "white" {}
		_CloudTexB ("Clouds B)", 2D) = "white" {}
		_BlendTex ("Blend", 2D) = "white" {}

	}
	SubShader {





//#####  ALTOSTRATUS CLOUD  #####
		Tags { "Queue"="Transparent-1" }
		Fog {Mode Off}
		Blend SrcAlpha OneMinusSrcAlpha
 		Offset 1,880000
 		Fog {Mode Off}
 		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf CloudLight vertex:vert alpha nofog noambient noforwardadd
		sampler2D _MainTex;
		sampler2D _BlendTex;
		float _cloudHeight;
		//float _cloudSpeed;
		float4 _colCloudS;
		float _clpCloud;
		float4 windCoords;
		float _amtCloudS;
		float4 _TenkokuCloudColor,_Tenkoku_Daylight,_Tenkoku_Nightlight;
		float4 _TenkokuCloudHighlightColor;
		sampler2D _Tenkoku_SkyBox;
		float4 _Tenkoku_overcastColor;
		float4 skyColor,_cloudSpd;
		float4 Tenkoku_Vec_MoonFwd,Tenkoku_Vec_SunFwd;
		float _Tenkoku_Ambient;
	float depth;
	float _Tenkoku_shaderDepth;
	float _TenkokuDist;
float _brightMult;
float _tenkokuTimer;

		struct Input {
			float4 screenPos;	
			//float3 pos;
			float2 uv_MainTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingCloudLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = half4(0,0,0,0);

			//alpha
			col.a = s.Alpha;

			//final lighting
			//col.rgb = saturate(saturate(max(max(_Tenkoku_Daylight.r,_Tenkoku_Daylight.g),_Tenkoku_Daylight.b) * dot(Tenkoku_Vec_SunFwd,half3(0,1,0))) + skyColor.rgb);
			half3 Tenkoku_SunFwd_half = half3(Tenkoku_Vec_SunFwd.x, Tenkoku_Vec_SunFwd.y, Tenkoku_Vec_SunFwd.z);
			
			col.rgb = saturate(saturate(max(max(_Tenkoku_Daylight.r,_Tenkoku_Daylight.g),_Tenkoku_Daylight.b) * dot(Tenkoku_SunFwd_half,half3(0,1,0))) + skyColor.rgb);

			//col.rgb = col.rgb * lerp(1.0-_brightMult,_brightMult,_Tenkoku_Ambient);
			//col.rgb = col.rgb * lerp(1.0-_brightMult,_brightMult*depth,_Tenkoku_Ambient);
			col.rgb += (saturate(1.0-_Tenkoku_Ambient) * _Tenkoku_Nightlight * 20 * (1.0-saturate(pow(0.998,dot(-viewDir,Tenkoku_Vec_MoonFwd.xyz)-0.5))));
			half3 fCol = lerp(half3(1,1,1),lerp(_TenkokuCloudColor.rgb,half3(1,1,1),_Tenkoku_overcastColor.a*2),lerp(0.0,0.8,dot(-Tenkoku_SunFwd_half,viewDir)));
			col.rgb = col.rgb * lerp(fixed3(1.0,1.0,1.0),fixed3(fCol.rgb),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
			//col.rgb = saturate(lerp(skyColor.rgb * 1.15,col.rgb,depth));
			


			col.rgb = lerp(col.rgb,col.rgb*max(1,(1.5*saturate(dot(-viewDir,Tenkoku_SunFwd_half)))),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
col.rgb = saturate(lerp(skyColor.rgb,col.rgb,depth));
			col = col * atten;
			return col;

		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			v.vertex.xyz -= v.normal*0.2;
		}
		void surf (Input IN, inout SurfaceOutput o) {
			//half c = tex2D(_MainTex, IN.uv_MainTex * 1.0 + float2(1.0,_Time*_cloudSpeed*0.3)).b*0.9;
			half c = tex2D(_MainTex, IN.uv_MainTex * 0.5 + (windCoords.xy*_cloudSpd.x)).b;
			//half m = tex2D(_MainTex, IN.uv_MainTex *30.0 + float2(1.0,_Time*(_cloudSpeed*0.3)+max(_Time*0.5,(_cloudSpeed*0.25)))).b;
			o.Albedo = fixed3(1,1,1);
			o.Alpha = c*_amtCloudS;//saturate(lerp(m,2.0,c))*min(0.75,c)*_colCloudS.a;
			o.Alpha *= tex2D(_BlendTex, IN.uv_BlendTex).r;
			
			float4 uv0 = IN.screenPos; uv0.xy;
        	uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));
			skyColor = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0));
			//clip(o.Alpha-_clpCloud);
half dpth = max(IN.screenPos.w,0.001)/_TenkokuDist;
depth = saturate(lerp(3,0,dpth));

depth = 1-saturate(max(IN.screenPos.w,0.001) / (_TenkokuDist * 0.15));

		}
		ENDCG
//##### END  #####





//#####  CIRRUS CLOUD  #####
		Tags { "Queue"="Transparent-1" }
		Fog {Mode Off}
		Blend SrcAlpha OneMinusSrcAlpha

 		Offset 1,880000
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf CloudLight vertex:vert alpha noambient noforwardadd
		sampler2D _MainTex;
		sampler2D _BlendTex;
		float _cloudHeight;
		//float _cloudSpeed;
		float4 _colCloudC;
		float _clpCloud;
		float4 windCoords;
		float _amtCloudC;
		float4 _TenkokuCloudColor,_Tenkoku_Daylight,_Tenkoku_Nightlight;
		float4 _TenkokuCloudHighlightColor;
		sampler2D _Tenkoku_SkyBox;
		float4 _Tenkoku_overcastColor;
		float4 skyColor,_cloudSpd;
		float4 Tenkoku_Vec_MoonFwd,Tenkoku_Vec_SunFwd;
		float _Tenkoku_Ambient;
	float depth;
	float _Tenkoku_shaderDepth;
	float _TenkokuDist;
float _brightMult;
float _tenkokuTimer;

		struct Input {
			float4 screenPos;	
			//float3 pos;
			float2 uv_MainTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingCloudLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = half4(0,0,0,0);

			col.a = s.Alpha;

			//final lighting
			half3 Tenkoku_SunFwd_half = half3(Tenkoku_Vec_SunFwd.x, Tenkoku_Vec_SunFwd.y, Tenkoku_Vec_SunFwd.z);
			col.rgb = saturate(saturate(max(max(_Tenkoku_Daylight.r,_Tenkoku_Daylight.g),_Tenkoku_Daylight.b) * dot(Tenkoku_SunFwd_half,half3(0,1,0))) + skyColor.rgb);
			//col.rgb = col.rgb * lerp(1.0-_brightMult,_brightMult*0.9,_Tenkoku_Ambient);
			col.rgb = col.rgb * lerp(1.0-_brightMult,(_brightMult*0.7)*depth,_Tenkoku_Ambient);
			col.rgb += (saturate(1.0-_Tenkoku_Ambient) * _Tenkoku_Nightlight * 40 * (1.0-saturate(pow(0.998,dot(-viewDir,Tenkoku_Vec_MoonFwd.xyz)-0.5))));
			half3 fCol = lerp(half3(1,1,1),lerp(_TenkokuCloudColor.rgb,half3(1,1,1),_Tenkoku_overcastColor.a*2),lerp(0.0,0.8,dot(-Tenkoku_SunFwd_half,viewDir)));
			col.rgb = col.rgb * lerp(fixed3(1.0,1.0,1.0),fixed3(fCol.rgb),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
			//col.rgb = saturate(lerp(skyColor.rgb * 1.15,col.rgb,depth));
			


			col.rgb = lerp(col.rgb,col.rgb*max(1,(3*saturate(dot(-viewDir,Tenkoku_SunFwd_half)))),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
col.rgb = saturate(lerp(skyColor.rgb,col.rgb,depth));
			col = col * atten;
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			v.vertex.xyz -= v.normal*0.1;
		}
		void surf (Input IN, inout SurfaceOutput o) {
			//half c = tex2D(_MainTex, IN.uv_MainTex *1.0 + float2(1.0,_Time*_cloudSpeed*0.5)).g*0.9;
			half c = tex2D(_MainTex, IN.uv_MainTex * 1.4 + (windCoords.xy*_cloudSpd.y)).g;
			//half m = tex2D(_MainTex, IN.uv_MainTex *30.0 + float2(1.0,_Time*(_cloudSpeed*0.5)+max(_Time*0.5,(_cloudSpeed*0.25)))).g;
			o.Albedo = fixed3(1,1,1);
			o.Alpha = c * _amtCloudC;//*_colCloudC.a;
			o.Alpha *= tex2D(_BlendTex, IN.uv_BlendTex).r;
			
			float4 uv0 = IN.screenPos; uv0.xy;
			uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));
			skyColor = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0));

half dpth = max(IN.screenPos.w,0.001)/_TenkokuDist;
depth = saturate(lerp(3,0,dpth));

depth = 1-saturate(max(IN.screenPos.w,0.001) / (_TenkokuDist * 0.15));

		}
		ENDCG


		Tags { "Queue"="Transparent-1" }
		Fog {Mode Off}
		Blend SrcAlpha OneMinusSrcAlpha

 		Offset 1,880000
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf CloudLight vertex:vert alpha noambient noforwardadd
		sampler2D _MainTex;
		sampler2D _BlendTex;
		float _cloudHeight;
		//float _cloudSpeed;
		float4 _colCloudC;
		float _clpCloud;
		float4 windCoords;
		float _amtCloudC;
		float4 _TenkokuCloudColor,_Tenkoku_Daylight,_Tenkoku_Nightlight;
		float4 _TenkokuCloudHighlightColor;
		sampler2D _Tenkoku_SkyBox;
		float4 _Tenkoku_overcastColor;
		float4 skyColor,_cloudSpd;
		float4 Tenkoku_Vec_MoonFwd,Tenkoku_Vec_SunFwd;
		float _Tenkoku_Ambient;
	float depth;
	float _Tenkoku_shaderDepth;
	float _TenkokuDist;
float _brightMult;
float _tenkokuTimer;

		struct Input {
			float4 screenPos;	
			//float3 pos;
			float2 uv_MainTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingCloudLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = half4(0,0,0,0);

			col.a = s.Alpha;

			//final lighting
			half3 Tenkoku_SunFwd_half = half3(Tenkoku_Vec_SunFwd.x, Tenkoku_Vec_SunFwd.y, Tenkoku_Vec_SunFwd.z);
			col.rgb = saturate(saturate(max(max(_Tenkoku_Daylight.r,_Tenkoku_Daylight.g),_Tenkoku_Daylight.b) * dot(Tenkoku_SunFwd_half,half3(0,1,0))) + skyColor.rgb);
			//col.rgb = col.rgb * lerp(1.0-_brightMult,_brightMult*0.9,_Tenkoku_Ambient);
			col.rgb = col.rgb * lerp(1.0-_brightMult,(_brightMult*0.8)*depth,_Tenkoku_Ambient);
			col.rgb += (saturate(1.0-_Tenkoku_Ambient) * _Tenkoku_Nightlight * 30 * (1.0-saturate(pow(0.998,dot(-viewDir,Tenkoku_Vec_MoonFwd.xyz)-0.5))));
			half3 fCol = lerp(half3(1,1,1),lerp(_TenkokuCloudColor.rgb,half3(1,1,1),_Tenkoku_overcastColor.a*2),lerp(0.0,0.8,dot(-Tenkoku_SunFwd_half,viewDir)));
			col.rgb = col.rgb * lerp(fixed3(1.0,1.0,1.0),fixed3(fCol.rgb),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
			//col.rgb = saturate(lerp(skyColor.rgb * 1.15,col.rgb,depth));
			


			col.rgb = lerp(col.rgb,col.rgb*max(1,(3*saturate(dot(-viewDir,Tenkoku_SunFwd_half)))),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));

col.rgb = saturate(lerp(skyColor.rgb,col.rgb,depth));
			col = col * atten;
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			v.vertex.xyz -= v.normal*0.198;
		}
		void surf (Input IN, inout SurfaceOutput o) {
			//half c = tex2D(_MainTex, IN.uv_MainTex *1.0 + float2(1.0,_Time*_cloudSpeed*0.5)).g*0.9;
			half c = tex2D(_MainTex, IN.uv_MainTex * 1.4 + (windCoords.xy*_cloudSpd.y)).g;
			//half m = tex2D(_MainTex, IN.uv_MainTex *30.0 + float2(1.0,_Time*(_cloudSpeed*0.5)+max(_Time*0.5,(_cloudSpeed*0.25)))).g;
			o.Albedo = fixed3(1,1,1);
			o.Alpha = c*_amtCloudC;//saturate(lerp(m,2.0,c))*min(0.75,c)*_colCloudC.a;
			o.Alpha *= tex2D(_BlendTex, IN.uv_BlendTex).r;
			
			float4 uv0 = IN.screenPos; uv0.xy;
			uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));
			skyColor = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0));

half dpth = max(IN.screenPos.w,0.001)/_TenkokuDist;
depth = saturate(lerp(3,0,dpth));

depth = 1-saturate(max(IN.screenPos.w,0.001) / (_TenkokuDist * 0.15));
		}
		ENDCG
//##### END  #####













//#####  LOW CLOUD  #####

		Tags { "Queue"="Transparent-1" }
		Fog {Mode Off}
		Blend SrcAlpha OneMinusSrcAlpha
 		Offset 1,880000
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf CloudLight vertex:vert alpha noambient noforwardadd
		sampler2D _MainTex;
		sampler2D _BlendTex;
		sampler2D _CloudTexB;
		float _cloudHeight;
		//float _cloudSpeed;
		float4 _colCloud;
		float _clpCloud;
		float _sizeCloud;
		float4 windCoords;
		float _amtCloudM;
		float4 _colTint;
		float4 _TenkokuCloudColor,_Tenkoku_Daylight,_Tenkoku_Nightlight;
		float4 _TenkokuCloudHighlightColor;
		sampler2D _Tenkoku_SkyBox;
		float4 _Tenkoku_overcastColor;
		float4 skyColor,_cloudSpd;
		float4 Tenkoku_Vec_MoonFwd,Tenkoku_Vec_SunFwd;
		float _Tenkoku_Ambient;
	float depth;
	float _Tenkoku_shaderDepth;
	float _TenkokuDist;
float _brightMult;
float _tenkokuTimer;

		struct Input {
			float4 screenPos;	
			//float3 pos;
			float2 uv_MainTex;
			float2 uv_BlendTex;
			float2 uv_CloudTexB;
		};
		fixed4 LightingCloudLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = half4(0,0,0,0);

			half3 Tenkoku_SunFwd_half = half3(Tenkoku_Vec_SunFwd.x, Tenkoku_Vec_SunFwd.y, Tenkoku_Vec_SunFwd.z);
			col.a = s.Alpha * 0.4;
			col.a *= 0.2;
col.a = col.a * (1-saturate(_Tenkoku_overcastColor.a*4.0));
			//final lighting
			col.rgb = saturate(saturate(max(max(_Tenkoku_Daylight.r,_Tenkoku_Daylight.g),_Tenkoku_Daylight.b) * dot(Tenkoku_SunFwd_half,half3(0,1,0))) + skyColor.rgb);
			col.rgb = col.rgb * lerp(1.0-_brightMult,_brightMult*1.1*depth,_Tenkoku_Ambient);
			col.rgb += (saturate(1.0-_Tenkoku_Ambient) * _Tenkoku_Nightlight * 40 * (1.0-saturate(pow(0.998,dot(-viewDir,Tenkoku_Vec_MoonFwd.xyz)-0.5))));
			half3 fCol = lerp(half3(1,1,1),lerp(_TenkokuCloudColor.rgb,half3(1,1,1),_Tenkoku_overcastColor.a*2),lerp(0.0,0.1,dot(-Tenkoku_SunFwd_half,viewDir)));
			col.rgb = col.rgb * lerp(fixed3(1.0,1.0,1.0),fixed3(fCol.rgb),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
			//col.rgb = saturate(lerp(skyColor.rgb * 1.15,col.rgb,depth));
			


			col.rgb = lerp(col.rgb,col.rgb*max(1,(6*saturate(dot(-viewDir,Tenkoku_SunFwd_half)))),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
col.rgb = saturate(lerp(skyColor.rgb,col.rgb,depth));

			col *= atten;
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			v.vertex.xyz += v.normal*0.013*_cloudHeight;
			o.screenPos = UnityObjectToClipPos (v.vertex);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			//half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB + float2(1.0,_Time*_cloudSpeed));
			half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB * 1.0 + (windCoords.xy*_cloudSpd.z));
			half d = 0;//tex2D(_MainTex, IN.uv_MainTex * 1.0 + (windCoords.xy*_cloudSpd.z)).a;
			//half m = tex2D(_CloudTexB, IN.uv_CloudTexB *20.0 + float2(1.0,_Time*(_cloudSpeed)+max(_Time*0.5,(_cloudSpeed*0.25)))).a;
			//o.Albedo = _colTint;//fixed3(1,1,1);

			o.Alpha = lerp(0.0,c.r,saturate(lerp(0.0,4.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.g,saturate(lerp(-1.0,3.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.b,saturate(lerp(-2.0,2.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.a,saturate(lerp(-3.0,1.0,_sizeCloud)));
			o.Alpha += lerp(0.0,d,saturate(lerp(-4.0,1.0,_sizeCloud)));
			o.Alpha = saturate(o.Alpha*1.0);//*_amtCloudM;

			half f = tex2D(_CloudTexB, IN.uv_CloudTexB * 8.0 + ((_Time.x*_cloudSpd*0.00001)+windCoords.xy*10.0)).a*0.6;
			o.Alpha = saturate(o.Alpha+lerp(-1.0,0.7,f))*(o.Alpha*3);

			o.Alpha = saturate(o.Alpha);
			o.Alpha *= tex2D(_BlendTex, IN.uv_BlendTex).r;
			
			float4 uv0 = IN.screenPos; uv0.xy;
			uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));
			skyColor = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0));
			clip(o.Alpha-_clpCloud);
			
half dpth = max(IN.screenPos.w,0.001)/_TenkokuDist;
depth = saturate(lerp(3,0,dpth));

depth = 1-saturate(max(IN.screenPos.w,0.001) / (_TenkokuDist * 0.15));

		}
		ENDCG




		Tags { "Queue"="Transparent-1" }
		Fog {Mode Off}
		Blend SrcAlpha OneMinusSrcAlpha
 		Offset 1,880000
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf CloudLight vertex:vert alpha noambient noforwardadd
		sampler2D _MainTex;
		sampler2D _BlendTex;
		sampler2D _CloudTexB;
		float _cloudHeight;
		//float _cloudSpeed;
		float4 _colCloud;
		float _clpCloud;
		float _sizeCloud;
		float4 windCoords;
		float _amtCloudM;
		float4 _colTint;
		float4 _TenkokuCloudColor,_Tenkoku_Daylight,_Tenkoku_Nightlight;
		float4 _TenkokuCloudHighlightColor;
		sampler2D _Tenkoku_SkyBox;
		float4 _Tenkoku_overcastColor;
		float4 skyColor,_cloudSpd;
		float4 Tenkoku_Vec_MoonFwd,Tenkoku_Vec_SunFwd;
		float _Tenkoku_Ambient;
		float depth;
		float _Tenkoku_shaderDepth;
	float _TenkokuDist;
float _brightMult;
float _tenkokuTimer;

		struct Input {
			float4 screenPos;	
			//float3 pos;
			float2 uv_MainTex;
			float2 uv_BlendTex;
			float2 uv_CloudTexB;
		};
		fixed4 LightingCloudLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = half4(0,0,0,0);

			half3 Tenkoku_SunFwd_half = half3(Tenkoku_Vec_SunFwd.x, Tenkoku_Vec_SunFwd.y, Tenkoku_Vec_SunFwd.z);

			col.a = s.Alpha * 0.5;
			col.a *= 0.2;
col.a = col.a * (1-saturate(_Tenkoku_overcastColor.a*4.0));
			//final lighting
			col.rgb = saturate(saturate(max(max(_Tenkoku_Daylight.r,_Tenkoku_Daylight.g),_Tenkoku_Daylight.b) * dot(Tenkoku_SunFwd_half,half3(0,1,0))) + skyColor.rgb);
			col.rgb = col.rgb * lerp(1.0-_brightMult,_brightMult*1.1*depth,_Tenkoku_Ambient);
			col.rgb += (saturate(1.0-_Tenkoku_Ambient) * _Tenkoku_Nightlight * 40 * (1.0-saturate(pow(0.998,dot(-viewDir,Tenkoku_Vec_MoonFwd.xyz)-0.5))));
			half3 fCol = lerp(half3(1,1,1),lerp(_TenkokuCloudColor.rgb,half3(1,1,1),_Tenkoku_overcastColor.a*2),lerp(0.0,0.1,dot(-Tenkoku_SunFwd_half,viewDir)));
			col.rgb = col.rgb * lerp(fixed3(1.0,1.0,1.0),fixed3(fCol.rgb),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
			//col.rgb = saturate(lerp(skyColor.rgb * 1.15,col.rgb,depth));
			


			col.rgb = lerp(col.rgb,col.rgb*max(1,(5*saturate(dot(-viewDir,Tenkoku_SunFwd_half)))),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
col.rgb = saturate(lerp(skyColor.rgb,col.rgb,depth));
			col *= atten;
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			v.vertex.xyz += v.normal*0.014*_cloudHeight;
			o.screenPos = UnityObjectToClipPos (v.vertex);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			//half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB + float2(1.0,_Time*_cloudSpeed));
			half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB * 1.0 + (windCoords.xy*_cloudSpd.z));
			half d = 0;//tex2D(_MainTex, IN.uv_MainTex * 1.0 + (windCoords.xy*_cloudSpd.z)).a;
			//half m = tex2D(_CloudTexB, IN.uv_CloudTexB *20.0 + float2(1.0,_Time*(_cloudSpeed)+max(_Time*0.5,(_cloudSpeed*0.25)))).a;
			o.Albedo = fixed3(1,1,1);

			o.Alpha = lerp(0.0,c.r,saturate(lerp(0.0,4.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.g,saturate(lerp(-1.0,3.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.b,saturate(lerp(-2.0,2.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.a,saturate(lerp(-3.0,1.0,_sizeCloud)));
			o.Alpha += lerp(0.0,d,saturate(lerp(-4.0,1.0,_sizeCloud)));
			o.Alpha = saturate(o.Alpha*1.0);//*_amtCloudM;

			half f = tex2D(_CloudTexB, IN.uv_CloudTexB * 6.0 + ((_Time.x*_cloudSpd*0.00001)+windCoords.xy*8.0)).a*0.4;
			o.Alpha = saturate(o.Alpha+lerp(-1.0,0.7,f))*(o.Alpha*3);

			o.Alpha = saturate(o.Alpha);
			o.Alpha *= tex2D(_BlendTex, IN.uv_BlendTex).r;
			
			float4 uv0 = IN.screenPos; uv0.xy;
			uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));
			skyColor = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0));
			clip(o.Alpha-_clpCloud);
			
half dpth = max(IN.screenPos.w,0.001)/_TenkokuDist;
depth = saturate(lerp(3,0,dpth));

depth = 1-saturate(max(IN.screenPos.w,0.001) / (_TenkokuDist * 0.15));
		}
		ENDCG




		Tags { "Queue"="Transparent-1" }
		Fog {Mode Off}
		Blend SrcAlpha OneMinusSrcAlpha
 		Offset 1,880000
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf CloudLight vertex:vert alpha noambient noforwardadd
		sampler2D _MainTex;
		sampler2D _BlendTex;
		sampler2D _CloudTexB;
		float _cloudHeight;
		//float _cloudSpeed;
		float4 _colCloud;
		float _clpCloud;
		float _sizeCloud;
		float4 windCoords;
		float _amtCloudM;
		float4 _colTint;
		float4 _TenkokuCloudColor,_Tenkoku_Daylight,_Tenkoku_Nightlight;
		float4 _TenkokuCloudHighlightColor;
		sampler2D _Tenkoku_SkyBox;
		float4 _Tenkoku_overcastColor;
		float4 skyColor,_cloudSpd;
		float4 Tenkoku_Vec_MoonFwd,Tenkoku_Vec_SunFwd;
		float _Tenkoku_Ambient;
		float depth;
		float _Tenkoku_shaderDepth;
	float _TenkokuDist;
float _brightMult;
float _tenkokuTimer;

		struct Input {
			float4 screenPos;	
			//float3 pos;
			float2 uv_MainTex;
			float2 uv_BlendTex;
			float2 uv_CloudTexB;
		};
		fixed4 LightingCloudLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = half4(0,0,0,0);

			half3 Tenkoku_SunFwd_half = half3(Tenkoku_Vec_SunFwd.x, Tenkoku_Vec_SunFwd.y, Tenkoku_Vec_SunFwd.z);

			col.a *= 0.2;
col.a = col.a * (1-saturate(_Tenkoku_overcastColor.a*4.0));
			//final lighting
			col.rgb = saturate(saturate(max(max(_Tenkoku_Daylight.r,_Tenkoku_Daylight.g),_Tenkoku_Daylight.b) * dot(Tenkoku_SunFwd_half,half3(0,1,0))) + skyColor.rgb);
			col.rgb = col.rgb * lerp(1.0-_brightMult,_brightMult*1.1*depth,_Tenkoku_Ambient);
			col.rgb += (saturate(1.0-_Tenkoku_Ambient) * _Tenkoku_Nightlight * 40 * (1.0-saturate(pow(0.998,dot(-viewDir,Tenkoku_Vec_MoonFwd.xyz)-0.5))));
			half3 fCol = lerp(half3(1,1,1),lerp(_TenkokuCloudColor.rgb,half3(1,1,1),_Tenkoku_overcastColor.a*2),lerp(0.0,0.2,dot(-Tenkoku_SunFwd_half,viewDir)));
			col.rgb = col.rgb * lerp(fixed3(0.95,0.95,0.95),fixed3(fCol.rgb),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
			//col.rgb = saturate(lerp(skyColor.rgb * 1.15,col.rgb,depth));
			


			col.rgb = lerp(col.rgb,col.rgb*max(1,(4*saturate(dot(-viewDir,Tenkoku_SunFwd_half)))),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
col.rgb = saturate(lerp(skyColor.rgb,col.rgb,depth));
			col *= atten;
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			v.vertex.xyz += v.normal*0.017*_cloudHeight;
			o.screenPos = UnityObjectToClipPos (v.vertex);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			//half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB + float2(1.0,_Time*_cloudSpeed));
			half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB * 1.0 + (windCoords.xy*_cloudSpd.z));
			half d = 0;//tex2D(_MainTex, IN.uv_MainTex * 1.0 + (windCoords.xy*_cloudSpd.z)).a;
			//half m = tex2D(_CloudTexB, IN.uv_CloudTexB *20.0 + float2(1.0,_Time*(_cloudSpeed)+max(_Time*0.5,(_cloudSpeed*0.25)))).a;
			o.Albedo = fixed3(1,1,1);

			o.Alpha = lerp(0.0,c.r,saturate(lerp(0.0,4.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.g,saturate(lerp(-1.0,3.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.b,saturate(lerp(-2.0,2.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.a,saturate(lerp(-3.0,1.0,_sizeCloud)));
			o.Alpha += lerp(0.0,d,saturate(lerp(-4.0,1.0,_sizeCloud)));
			o.Alpha = saturate(o.Alpha*1.0);//*_amtCloudM;

			half f = tex2D(_CloudTexB, IN.uv_CloudTexB * 4.0 + ((_Time.x*_cloudSpd*0.00001)+windCoords.xy*6.0)).a*0.5;
			o.Alpha = saturate(o.Alpha+lerp(-1.0,0.7,f))*(o.Alpha*3);

			o.Alpha = saturate(o.Alpha);
			o.Alpha *= tex2D(_BlendTex, IN.uv_BlendTex).r;
			
			float4 uv0 = IN.screenPos; uv0.xy;
			uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));
			skyColor = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0));
			clip(o.Alpha-_clpCloud);
			
half dpth = max(IN.screenPos.w,0.001)/_TenkokuDist;
depth = saturate(lerp(3,0,dpth));

depth = 1-saturate(max(IN.screenPos.w,0.001) / (_TenkokuDist * 0.15));

		}
		ENDCG




//---
		Tags { "Queue"="Transparent-1" }
		Fog {Mode Off}
		Blend SrcAlpha OneMinusSrcAlpha
 		Offset 1,880000
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf CloudLight vertex:vert alpha noambient noforwardadd
		sampler2D _MainTex;
		sampler2D _BlendTex;
		sampler2D _CloudTexB;
		float _cloudHeight;
		//float _cloudSpeed;
		float4 _colCloud;
		float _clpCloud;
		float _sizeCloud;
		float4 windCoords;
		float _amtCloudM;
		float4 _colTint;
		float4 _TenkokuCloudColor,_Tenkoku_Daylight,_Tenkoku_Nightlight;
		float4 _TenkokuCloudHighlightColor;
		sampler2D _Tenkoku_SkyBox;
		float4 _Tenkoku_overcastColor;
		float4 skyColor,_cloudSpd;
		float4 Tenkoku_Vec_MoonFwd,Tenkoku_Vec_SunFwd;
		float _Tenkoku_Ambient;
		float depth;
		float _Tenkoku_shaderDepth;
	float _TenkokuDist;
float _brightMult;
float _tenkokuTimer;

		struct Input {
			float4 screenPos;	
			//float3 pos;
			float2 uv_MainTex;
			float2 uv_BlendTex;
			float2 uv_CloudTexB;
		};
		fixed4 LightingCloudLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = half4(0,0,0,0);

			half3 Tenkoku_SunFwd_half = half3(Tenkoku_Vec_SunFwd.x, Tenkoku_Vec_SunFwd.y, Tenkoku_Vec_SunFwd.z);

			col.a = s.Alpha * 0.7;
col.a = col.a * (1-saturate(_Tenkoku_overcastColor.a*4.0));
			//final lighting
			col.rgb = saturate(saturate(max(max(_Tenkoku_Daylight.r,_Tenkoku_Daylight.g),_Tenkoku_Daylight.b) * dot(Tenkoku_SunFwd_half,half3(0,1,0))) + skyColor.rgb);
			col.rgb = col.rgb * lerp(1.0-_brightMult,_brightMult*1.1*depth,_Tenkoku_Ambient);
			col.rgb += (saturate(1.0-_Tenkoku_Ambient) * _Tenkoku_Nightlight * 40 * (1.0-saturate(pow(0.998,dot(-viewDir,Tenkoku_Vec_MoonFwd.xyz)-0.5))));
			half3 fCol = lerp(half3(1,1,1),lerp(_TenkokuCloudColor.rgb,half3(1,1,1),_Tenkoku_overcastColor.a*2),lerp(0.0,0.8,dot(-Tenkoku_SunFwd_half,viewDir)));
			col.rgb = col.rgb * lerp(fixed3(0.9,0.9,0.9),fixed3(fCol.rgb),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
			//col.rgb = saturate(lerp(skyColor.rgb * 1.15,col.rgb,depth));
			


			col.rgb = lerp(col.rgb,col.rgb*max(1,(3*saturate(dot(-viewDir,Tenkoku_SunFwd_half)))),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
col.rgb = saturate(lerp(skyColor.rgb,col.rgb,depth));
			col *= atten;
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			v.vertex.xyz += v.normal*0.02*_cloudHeight;
			o.screenPos = UnityObjectToClipPos (v.vertex);
		}
		void surf (Input IN, inout SurfaceOutput o) {

			//half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB + float2(1.0,_Time*_cloudSpeed));
			half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB * 1.0 + (windCoords.xy*_cloudSpd.z));
			half d = 0;//tex2D(_MainTex, IN.uv_MainTex * 1.0 + (windCoords.xy*_cloudSpd.z)).a;
			//half m = tex2D(_CloudTexB, IN.uv_CloudTexB *20.0 + float2(1.0,_Time*(_cloudSpeed)+max(_Time*0.5,(_cloudSpeed*0.25)))).a;
			o.Albedo = fixed3(1,1,1);

			o.Alpha = lerp(0.0,c.r,saturate(lerp(0.0,4.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.g,saturate(lerp(-1.0,3.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.b,saturate(lerp(-2.0,2.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.a,saturate(lerp(-3.0,1.0,_sizeCloud)));
			o.Alpha += lerp(0.0,d,saturate(lerp(-4.0,1.0,_sizeCloud)));
			o.Alpha = saturate(o.Alpha*1.0);//*_amtCloudM;

			half f = tex2D(_CloudTexB, IN.uv_CloudTexB * 6.0 + ((_Time.x*_cloudSpd*0.00001)+windCoords.xy*6.0)).a*0.5;
			o.Alpha = saturate(o.Alpha+lerp(-1.0,0.7,f))*(o.Alpha*3);

			o.Alpha = saturate(o.Alpha);
			o.Alpha *= tex2D(_BlendTex, IN.uv_BlendTex).r;
			
			float4 uv0 = IN.screenPos; uv0.xy;
			uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));
			skyColor = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0));
			clip(o.Alpha-_clpCloud);
			
half dpth = max(IN.screenPos.w,0.001)/_TenkokuDist;
depth = saturate(lerp(3,0,dpth));

depth = 1-saturate(max(IN.screenPos.w,0.001) / (_TenkokuDist * 0.15));

		}
		ENDCG

//---


		Tags { "Queue"="Transparent-1" }
		Fog {Mode Off}
		Blend SrcAlpha OneMinusSrcAlpha
 		Offset 1,880000
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf CloudLight vertex:vert alpha noambient noforwardadd
		sampler2D _MainTex;
		sampler2D _BlendTex;
		sampler2D _CloudTexB;
		float _cloudHeight;
		//float _cloudSpeed;
		float4 _colCloud;
		float _clpCloud;
		float _sizeCloud;
		float4 windCoords;
		float _amtCloudM;
		float4 _colTint;
		float4 _TenkokuCloudColor,_Tenkoku_Daylight,_Tenkoku_Nightlight;
		float4 _TenkokuCloudHighlightColor;
		sampler2D _Tenkoku_SkyBox;
		float4 _Tenkoku_overcastColor;
		float4 skyColor,_cloudSpd;
		float4 Tenkoku_Vec_MoonFwd,Tenkoku_Vec_SunFwd;
		float _Tenkoku_Ambient;
		float depth;
		float _Tenkoku_shaderDepth;
	float _TenkokuDist;
float _brightMult;
float _tenkokuTimer;

		struct Input {
			float4 screenPos;	
			//float3 pos;
			float2 uv_MainTex;
			float2 uv_BlendTex;
			float2 uv_CloudTexB;
		};
		fixed4 LightingCloudLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = half4(0,0,0,0);
			half3 Tenkoku_SunFwd_half = half3(Tenkoku_Vec_SunFwd.x, Tenkoku_Vec_SunFwd.y, Tenkoku_Vec_SunFwd.z);

			col.a = s.Alpha * 0.8;
col.a = col.a * (1-saturate(_Tenkoku_overcastColor.a*4.0));
			//final lighting
			col.rgb = saturate(saturate(max(max(_Tenkoku_Daylight.r,_Tenkoku_Daylight.g),_Tenkoku_Daylight.b) * dot(Tenkoku_SunFwd_half,half3(0,1,0))) + skyColor.rgb);
			col.rgb = col.rgb * lerp(1.0-_brightMult,_brightMult*1.1*depth,_Tenkoku_Ambient);
			col.rgb += (saturate(1.0-_Tenkoku_Ambient) * _Tenkoku_Nightlight * 40 * (1.0-saturate(pow(0.998,dot(-viewDir,Tenkoku_Vec_MoonFwd.xyz)-0.5))));
			half3 fCol = lerp(half3(1,1,1),lerp(_TenkokuCloudColor.rgb,half3(1,1,1),_Tenkoku_overcastColor.a*2),lerp(0.0,0.8,dot(-Tenkoku_SunFwd_half,viewDir)));
			col.rgb = col.rgb * lerp(fixed3(0.85,0.85,0.85),fixed3(fCol.rgb),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
			//col.rgb = saturate(lerp(skyColor.rgb * 1.15,col.rgb,depth));



			col.rgb = lerp(col.rgb,col.rgb*max(1,(2*saturate(dot(-viewDir,Tenkoku_SunFwd_half)))),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
col.rgb = saturate(lerp(skyColor.rgb,col.rgb,depth));
			col *= atten;
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			v.vertex.xyz += v.normal*0.024*_cloudHeight;
			o.screenPos = UnityObjectToClipPos (v.vertex);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			//half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB + float2(1.0,_Time*_cloudSpeed));
			half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB * 1.0 + (windCoords.xy*_cloudSpd.z));
			half d = 0;//tex2D(_MainTex, IN.uv_MainTex * 1.0 + (windCoords.xy*_cloudSpd.z)).a;
			//half m = tex2D(_CloudTexB, IN.uv_CloudTexB *20.0 + float2(1.0,_Time*(_cloudSpeed)+max(_Time*0.5,(_cloudSpeed*0.25)))).a;
			o.Albedo = fixed3(1,1,1);

			o.Alpha = lerp(0.0,c.r,saturate(lerp(0.0,4.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.g,saturate(lerp(-1.0,3.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.b,saturate(lerp(-2.0,2.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.a,saturate(lerp(-3.0,1.0,_sizeCloud)));
			o.Alpha += lerp(0.0,d,saturate(lerp(-4.0,1.0,_sizeCloud)));
			o.Alpha = saturate(o.Alpha*1.0);//*_amtCloudM;

			half f = tex2D(_CloudTexB, IN.uv_CloudTexB * 2.0 + ((_Time.x*_cloudSpd*0.00001)+windCoords.xy*6.0)).a*0.5;
			o.Alpha = saturate(o.Alpha+lerp(-1.0,0.7,f))*(o.Alpha*3);

			o.Alpha = saturate(o.Alpha);
			o.Alpha *= tex2D(_BlendTex, IN.uv_BlendTex).r;
			
			float4 uv0 = IN.screenPos; uv0.xy;
			uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));
			skyColor = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0));
			clip(o.Alpha-_clpCloud);
			
half dpth = max(IN.screenPos.w,0.001)/_TenkokuDist;
depth = saturate(lerp(3,0,dpth));

depth = 1-saturate(max(IN.screenPos.w,0.001) / (_TenkokuDist * 0.15));

		}
		ENDCG

		//--------------------- end detail clouds




		Tags { "Queue"="Transparent-1" }
		Fog {Mode Off}
		Blend SrcAlpha OneMinusSrcAlpha
 		Offset 1,880000
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf CloudLight vertex:vert alpha noambient noforwardadd
		sampler2D _MainTex;
		sampler2D _BlendTex;
		sampler2D _CloudTexB;
		float _cloudHeight;
		//float _cloudSpeed;
		float4 _colCloud;
		float _clpCloud;
		float _sizeCloud;
		float4 windCoords;
		float _amtCloudM;
		float4 _colTint;
		float4 _TenkokuCloudColor,_Tenkoku_Daylight,_Tenkoku_Nightlight;
		float4 _TenkokuCloudHighlightColor;
		sampler2D _Tenkoku_SkyBox;
		float4 _Tenkoku_overcastColor;
		float4 skyColor,_cloudSpd;
		float4 Tenkoku_Vec_MoonFwd,Tenkoku_Vec_SunFwd;
		float _Tenkoku_Ambient;
		float depth;
		float _Tenkoku_shaderDepth;
	float _TenkokuDist;
float _brightMult;
float _tenkokuTimer;

		struct Input {
			float4 screenPos;	
			//float3 pos;
			float2 uv_MainTex;
			float2 uv_BlendTex;
			float2 uv_CloudTexB;
		};
		fixed4 LightingCloudLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = half4(0,0,0,0);
			half3 Tenkoku_SunFwd_half = half3(Tenkoku_Vec_SunFwd.x, Tenkoku_Vec_SunFwd.y, Tenkoku_Vec_SunFwd.z);

			col.a = s.Alpha;
col.a = col.a * (1-saturate(_Tenkoku_overcastColor.a*4.0));
			//final lighting
			col.rgb = saturate(saturate(max(max(_Tenkoku_Daylight.r,_Tenkoku_Daylight.g),_Tenkoku_Daylight.b) * dot(Tenkoku_SunFwd_half,half3(0,1,0))) + skyColor.rgb);
			col.rgb = col.rgb * lerp(1.0-_brightMult,_brightMult*1.1*depth,_Tenkoku_Ambient);
			col.rgb += (saturate(1.0-_Tenkoku_Ambient) * _Tenkoku_Nightlight * 40 * (1.0-saturate(pow(0.998,dot(-viewDir,Tenkoku_Vec_MoonFwd.xyz)-0.5))));
			half3 fCol = lerp(half3(1,1,1),lerp(_TenkokuCloudColor.rgb,half3(1,1,1),_Tenkoku_overcastColor.a*2),lerp(0.0,0.8,dot(-Tenkoku_SunFwd_half,viewDir)));
			col.rgb = col.rgb * lerp(fixed3(0.8,0.8,0.8),fixed3(fCol.rgb),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
			//col.rgb = saturate(lerp(skyColor.rgb * 1.15,col.rgb,depth));



			col.rgb = lerp(col.rgb,col.rgb*max(1,(2*saturate(dot(-viewDir,Tenkoku_SunFwd_half)))),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
col.rgb = saturate(lerp(skyColor.rgb,col.rgb,depth));
			col *= atten;
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			v.vertex.xyz += v.normal*0.027*_cloudHeight;
			o.screenPos = UnityObjectToClipPos (v.vertex);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			//half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB + float2(1.0,_Time*_cloudSpeed));
			half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB * 1.0 + (windCoords.xy*_cloudSpd.z));
			half d = 0;//tex2D(_MainTex, IN.uv_MainTex * 1.0 + (windCoords.xy*_cloudSpd.z)).a;
			//half m = tex2D(_CloudTexB, IN.uv_CloudTexB *20.0 + float2(1.0,_Time*(_cloudSpeed)+max(_Time*0.5,(_cloudSpeed*0.25)))).a;
			o.Albedo = fixed3(1,1,1);

			o.Alpha = lerp(0.0,c.r,saturate(lerp(0.0,4.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.g,saturate(lerp(-1.0,3.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.b,saturate(lerp(-2.0,2.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.a,saturate(lerp(-3.0,1.0,_sizeCloud)));
			o.Alpha += lerp(0.0,d,saturate(lerp(-4.0,1.0,_sizeCloud)));
			o.Alpha = saturate(o.Alpha*1.0);//*_amtCloudM;
			o.Alpha *= tex2D(_BlendTex, IN.uv_BlendTex).r;
			
			float4 uv0 = IN.screenPos; uv0.xy;
			uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));
			skyColor = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0));
			clip(o.Alpha-_clpCloud);
			
half dpth = max(IN.screenPos.w,0.001)/_TenkokuDist;
depth = saturate(lerp(3,0,dpth));

depth = 1-saturate(max(IN.screenPos.w,0.001) / (_TenkokuDist * 0.15));

		}
		ENDCG



		Tags { "Queue"="Transparent-1" }
		Fog {Mode Off}
		Blend SrcAlpha OneMinusSrcAlpha
 		Offset 1,880000
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf CloudLight vertex:vert alpha noambient noforwardadd
		sampler2D _MainTex;
		sampler2D _BlendTex;
		sampler2D _CloudTexB;
		float _cloudHeight;
		//float _cloudSpeed;
		float4 _colCloud;
		float _clpCloud;
		float _sizeCloud;
		float4 windCoords;
		float _amtCloudM;
		float4 _colTint;
		float4 _TenkokuCloudColor,_Tenkoku_Daylight,_Tenkoku_Nightlight;
		float4 _TenkokuCloudHighlightColor;
		sampler2D _Tenkoku_SkyBox;
		float4 _Tenkoku_overcastColor;
		float4 skyColor,_cloudSpd;
		float4 Tenkoku_Vec_MoonFwd,Tenkoku_Vec_SunFwd;
		float _Tenkoku_Ambient;
		float depth;
		float _Tenkoku_shaderDepth;
	float _TenkokuDist;
float _brightMult;
float _tenkokuTimer;

		struct Input {
			float4 screenPos;	
			//float3 pos;
			float2 uv_MainTex;
			float2 uv_BlendTex;
			float2 uv_CloudTexB;
		};
		fixed4 LightingCloudLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = half4(0,0,0,0);
			half3 Tenkoku_SunFwd_half = half3(Tenkoku_Vec_SunFwd.x, Tenkoku_Vec_SunFwd.y, Tenkoku_Vec_SunFwd.z);

			col.a = s.Alpha;
col.a = col.a * (1-saturate(_Tenkoku_overcastColor.a*4.0));
			//final lighting
			col.rgb = saturate(saturate(max(max(_Tenkoku_Daylight.r,_Tenkoku_Daylight.g),_Tenkoku_Daylight.b) * dot(Tenkoku_SunFwd_half,half3(0,1,0))) + skyColor.rgb);
			col.rgb = col.rgb * lerp(1.0-_brightMult,lerp(1.0,_brightMult*1.1,0.75)*depth,_Tenkoku_Ambient);
			col.rgb += (saturate(1.0-_Tenkoku_Ambient) * _Tenkoku_Nightlight * 30 * (1.0-saturate(pow(0.998,dot(-viewDir,Tenkoku_Vec_MoonFwd.xyz)-0.5))));
			half3 fCol = lerp(half3(1,1,1),lerp(_TenkokuCloudColor.rgb,half3(1,1,1),_Tenkoku_overcastColor.a*2),lerp(0.0,1.0,dot(-Tenkoku_SunFwd_half,viewDir)));
			col.rgb = col.rgb * lerp(fixed3(0.75,0.75,0.75),fixed3(fCol.rgb),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
			
			//col.rgb = saturate(lerp(skyColor.rgb * 1.15,col.rgb,depth));

col.rgb = saturate(lerp(skyColor.rgb,col.rgb,depth));

			col *= atten;
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			v.vertex.xyz += v.normal*0.03*_cloudHeight;
			o.screenPos = UnityObjectToClipPos (v.vertex);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			//half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB + float2(1.0,_Time*_cloudSpeed));
			half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB * 1.0 + (windCoords.xy*_cloudSpd.z));
			half d = 0;//tex2D(_MainTex, IN.uv_MainTex * 1.0 + (windCoords.xy*_cloudSpd.z)).a;
			//half m = tex2D(_CloudTexB, IN.uv_CloudTexB *20.0 + float2(1.0,_Time*(_cloudSpeed)+max(_Time*0.5,(_cloudSpeed*0.25)))).a;
			o.Albedo = fixed3(1,1,1);

			o.Alpha = lerp(0.0,c.r,saturate(lerp(0.0,4.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.g,saturate(lerp(-1.0,3.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.b,saturate(lerp(-2.0,2.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.a,saturate(lerp(-3.0,1.0,_sizeCloud)));
			o.Alpha += lerp(0.0,d,saturate(lerp(-4.0,1.0,_sizeCloud)));
			o.Alpha = saturate(o.Alpha*1.0);//*_amtCloudM;

			o.Alpha = lerp(-0.1,1.0,o.Alpha);
			o.Alpha *= tex2D(_BlendTex, IN.uv_BlendTex).r;
			
			float4 uv0 = IN.screenPos; uv0.xy;
			uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));
			skyColor = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0));
			clip(o.Alpha-_clpCloud);
			
half dpth = max(IN.screenPos.w,0.001)/_TenkokuDist;
depth = saturate(lerp(3,0,dpth));

depth = 1-saturate(max(IN.screenPos.w,0.001) / (_TenkokuDist * 0.15));

		}
		ENDCG





		Tags { "Queue"="Transparent-1" }
		Fog {Mode Off}
		Blend SrcAlpha OneMinusSrcAlpha
 		Offset 1,880000
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf CloudLight vertex:vert alpha noambient noforwardadd
		sampler2D _MainTex;
		sampler2D _BlendTex;
		sampler2D _CloudTexB;
		float _cloudHeight;
		//float _cloudSpeed;
		float4 _colCloud;
		float _clpCloud;
		float _sizeCloud;
		float4 windCoords;
		float _amtCloudM;
		float4 _colTint;
		float4 _TenkokuCloudColor,_Tenkoku_Daylight,_Tenkoku_Nightlight;
		float4 _TenkokuCloudHighlightColor;
		sampler2D _Tenkoku_SkyBox;
		float4 _Tenkoku_overcastColor;
		float4 skyColor,_cloudSpd;
		float4 Tenkoku_Vec_MoonFwd,Tenkoku_Vec_SunFwd;
		float _Tenkoku_Ambient;
		float depth;
		float _Tenkoku_shaderDepth;
	float _TenkokuDist;
float _brightMult;
float _tenkokuTimer;

		struct Input {
			float4 screenPos;	
			//float3 pos;
			float2 uv_MainTex;
			float2 uv_BlendTex;
			float2 uv_CloudTexB;
		};
		fixed4 LightingCloudLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = half4(0,0,0,0);
			half3 Tenkoku_SunFwd_half = half3(Tenkoku_Vec_SunFwd.x, Tenkoku_Vec_SunFwd.y, Tenkoku_Vec_SunFwd.z);

			col.a = s.Alpha * _colCloud.a;
col.a = col.a * (1-saturate(_Tenkoku_overcastColor.a*4.0));
			//final lighting
			col.rgb = saturate(saturate(max(max(_Tenkoku_Daylight.r,_Tenkoku_Daylight.g),_Tenkoku_Daylight.b) * dot(Tenkoku_SunFwd_half,half3(0,1,0))) + skyColor.rgb);
			col.rgb = col.rgb * lerp(1.0-_brightMult,lerp(1.0,_brightMult*1.1,0.5)*depth,_Tenkoku_Ambient);
			col.rgb += (saturate(1.0-_Tenkoku_Ambient) * _Tenkoku_Nightlight * 20 * (1.0-saturate(pow(0.998,dot(-viewDir,Tenkoku_Vec_MoonFwd.xyz)-0.5))));

			half3 fCol = lerp(half3(1,1,1),lerp(_TenkokuCloudColor.rgb,half3(1,1,1),_Tenkoku_overcastColor.a*2),lerp(0.0,1.4,dot(-Tenkoku_SunFwd_half,viewDir)));
			col.rgb = col.rgb * lerp(fixed3(0.65,0.65,0.65),fixed3(fCol.rgb),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
			//col.rgb = saturate(lerp(skyColor.rgb * 1.15,col.rgb,depth));

col.rgb = saturate(lerp(skyColor.rgb,col.rgb,depth));

			col *= atten;
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			v.vertex.xyz += v.normal*0.033*_cloudHeight;
			o.screenPos = UnityObjectToClipPos (v.vertex);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			//half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB + float2(1.0,_Time*_cloudSpeed));
			half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB * 1.0 + (windCoords.xy*_cloudSpd.z));
			half d = 0;//tex2D(_MainTex, IN.uv_MainTex * 1.0 + (windCoords.xy*_cloudSpd.z)).a;
			//half m = tex2D(_CloudTexB, IN.uv_CloudTexB *20.0 + float2(1.0,_Time*(_cloudSpeed)+max(_Time*0.5,(_cloudSpeed*0.25)))).a;
			o.Albedo = fixed3(1,1,1);

			o.Alpha = lerp(0.0,c.r,saturate(lerp(0.0,4.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.g,saturate(lerp(-1.0,3.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.b,saturate(lerp(-2.0,2.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.a,saturate(lerp(-3.0,1.0,_sizeCloud)));
			o.Alpha += lerp(0.0,d,saturate(lerp(-4.0,1.0,_sizeCloud)));
			o.Alpha = saturate(o.Alpha*1.0);//*_amtCloudM;
			
			o.Alpha = lerp(-0.25,1.0,o.Alpha);	
			o.Alpha *= tex2D(_BlendTex, IN.uv_BlendTex).r;
			
			float4 uv0 = IN.screenPos; uv0.xy;
			uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));
			skyColor = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0));
			clip(o.Alpha-_clpCloud);
						
half dpth = max(IN.screenPos.w,0.001)/_TenkokuDist;
depth = saturate(lerp(3,0,dpth));

depth = 1-saturate(max(IN.screenPos.w,0.001) / (_TenkokuDist * 0.15));

		}
		ENDCG







		Tags { "Queue"="Transparent-1" }
		Fog {Mode Off}
		Blend SrcAlpha OneMinusSrcAlpha
 		Offset 1,880000
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf CloudLight vertex:vert alpha noambient noforwardadd
		sampler2D _MainTex;
		sampler2D _BlendTex;
		sampler2D _CloudTexB;
		float _cloudHeight;
		//float _cloudSpeed;
		float4 _colCloud;
		float _clpCloud;
		float _sizeCloud;
		float4 windCoords;
		float _amtCloudM;
		float4 _colTint;
		float4 _TenkokuCloudColor,_Tenkoku_Daylight,_Tenkoku_Nightlight;
		float4 _TenkokuCloudHighlightColor;
		sampler2D _Tenkoku_SkyBox;
		float4 _Tenkoku_overcastColor;
		float4 skyColor,_cloudSpd;
		float4 Tenkoku_Vec_MoonFwd,Tenkoku_Vec_SunFwd;
		float _Tenkoku_Ambient;
		float depth;
		float _Tenkoku_shaderDepth;
	float _TenkokuDist;
float _brightMult;
float _tenkokuTimer;

		struct Input {
			float4 screenPos;	
			//float3 pos;
			float2 uv_MainTex;
			float2 uv_BlendTex;
			float2 uv_CloudTexB;
		};
		fixed4 LightingCloudLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = half4(0,0,0,0);
			half3 Tenkoku_SunFwd_half = half3(Tenkoku_Vec_SunFwd.x, Tenkoku_Vec_SunFwd.y, Tenkoku_Vec_SunFwd.z);

			col.a = s.Alpha * _colCloud.a;
col.a = col.a * (1-saturate(_Tenkoku_overcastColor.a*4.0));
			//final lighting
			col.rgb = saturate(saturate(max(max(_Tenkoku_Daylight.r,_Tenkoku_Daylight.g),_Tenkoku_Daylight.b) * dot(Tenkoku_SunFwd_half,half3(0,1,0))) + skyColor.rgb);
			col.rgb = col.rgb * lerp(1.0-_brightMult,lerp(1.0,_brightMult*1.1,0.35)*depth,_Tenkoku_Ambient);
			col.rgb += (saturate(1.0-_Tenkoku_Ambient) * _Tenkoku_Nightlight * 20 * (1.0-saturate(pow(0.998,dot(-viewDir,Tenkoku_Vec_MoonFwd.xyz)-0.5))));

			half3 fCol = lerp(half3(1,1,1),lerp(_TenkokuCloudColor.rgb,half3(1,1,1),_Tenkoku_overcastColor.a*2),lerp(0.0,1.4,dot(-Tenkoku_SunFwd_half,viewDir)));
			col.rgb = col.rgb * lerp(fixed3(0.65,0.65,0.65),fixed3(fCol.rgb),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
			//col.rgb = saturate(lerp(skyColor.rgb * 1.15,col.rgb,depth));

col.rgb = saturate(lerp(skyColor.rgb,col.rgb,depth));

			col *= atten;
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			v.vertex.xyz += v.normal*0.038*_cloudHeight;
			o.screenPos = UnityObjectToClipPos (v.vertex);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			//half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB + float2(1.0,_Time*_cloudSpeed));
			half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB * 1.0 + (windCoords.xy*_cloudSpd.z));
			half d = 0;//tex2D(_MainTex, IN.uv_MainTex * 1.0 + (windCoords.xy*_cloudSpd.z)).a;
			//half m = tex2D(_CloudTexB, IN.uv_CloudTexB *20.0 + float2(1.0,_Time*(_cloudSpeed)+max(_Time*0.5,(_cloudSpeed*0.25)))).a;
			o.Albedo = fixed3(1,1,1);

			o.Alpha = lerp(0.0,c.r,saturate(lerp(0.0,4.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.g,saturate(lerp(-1.0,3.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.b,saturate(lerp(-2.0,2.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.a,saturate(lerp(-3.0,1.0,_sizeCloud)));
			o.Alpha += lerp(0.0,d,saturate(lerp(-4.0,1.0,_sizeCloud)));
			o.Alpha = saturate(o.Alpha*1.0);//*_amtCloudM;
			
			o.Alpha = lerp(-0.4,1.0,o.Alpha);
			o.Alpha *= tex2D(_BlendTex, IN.uv_BlendTex).r;
			
			float4 uv0 = IN.screenPos; uv0.xy;
			uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));
			skyColor = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0));
			clip(o.Alpha-_clpCloud);

half dpth = max(IN.screenPos.w,0.001)/_TenkokuDist;
depth = saturate(lerp(3,0,dpth));

depth = 1-saturate(max(IN.screenPos.w,0.001) / (_TenkokuDist * 0.15));

		}
		ENDCG





		Tags { "Queue"="Transparent-1" }
		Fog {Mode Off}
		Blend SrcAlpha OneMinusSrcAlpha
 		Offset 1,880000
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf CloudLight vertex:vert alpha noambient noforwardadd
		sampler2D _MainTex;
		sampler2D _BlendTex;
		sampler2D _CloudTexB;
		float _cloudHeight;
		//float _cloudSpeed;
		float4 _colCloud;
		float _clpCloud;
		float _sizeCloud;
		float4 windCoords;
		float _amtCloudM;
		float4 _colTint;
		float4 _TenkokuCloudColor,_Tenkoku_Daylight,_Tenkoku_Nightlight;
		float4 _TenkokuCloudHighlightColor;
		sampler2D _Tenkoku_SkyBox;
		float4 _Tenkoku_overcastColor;
		float4 skyColor,_cloudSpd;
		float4 Tenkoku_Vec_MoonFwd,Tenkoku_Vec_SunFwd;
		float _Tenkoku_Ambient;
		float depth;
		float _Tenkoku_shaderDepth;
	float _TenkokuDist;
float _brightMult;
float _tenkokuTimer;

		struct Input {
			float4 screenPos;	
			//float3 pos;
			float2 uv_MainTex;
			float2 uv_BlendTex;
			float2 uv_CloudTexB;
		};
		fixed4 LightingCloudLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = half4(0,0,0,0);
			half3 Tenkoku_SunFwd_half = half3(Tenkoku_Vec_SunFwd.x, Tenkoku_Vec_SunFwd.y, Tenkoku_Vec_SunFwd.z);

			//alpha
			col.a = s.Alpha * _colCloud.a;
col.a = col.a * (1-saturate(_Tenkoku_overcastColor.a*4.0));
			//final lighting
			col.rgb = saturate(saturate(max(max(_Tenkoku_Daylight.r,_Tenkoku_Daylight.g),_Tenkoku_Daylight.b) * dot(Tenkoku_SunFwd_half,half3(0,1,0))) + skyColor.rgb);
			//col.rgb = col.rgb * lerp(_brightMult,1.0,depth);
			col.rgb = col.rgb * lerp(1.0-_brightMult,lerp(1.0,_brightMult*1.1,0.25)*depth,_Tenkoku_Ambient);
			col.rgb += (saturate(1.0-_Tenkoku_Ambient) * _Tenkoku_Nightlight * 12 * (1.0-saturate(pow(0.998,dot(-viewDir,Tenkoku_Vec_MoonFwd.xyz)-0.5))));
			half3 fCol = lerp(half3(1,1,1),lerp(_TenkokuCloudColor.rgb,half3(1,1,1),_Tenkoku_overcastColor.a*2),lerp(0.0,1.5,dot(-Tenkoku_SunFwd_half,viewDir)));
			col.rgb = col.rgb * lerp(fixed3(0.6,0.6,0.6),fixed3(fCol.rgb)*fixed3(0.9,0.9,0.9),saturate(dot(-viewDir,Tenkoku_SunFwd_half)));
			


			//col.rgb = saturate(lerp(skyColor.rgb * 1.15,col.rgb,depth));

col.rgb = saturate(lerp(skyColor.rgb,col.rgb,depth));
			
			col *= atten;
			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			v.vertex.xyz += v.normal*0.04*_cloudHeight;
			o.screenPos = UnityObjectToClipPos (v.vertex);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			//half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB + float2(1.0,_Time*_cloudSpeed));
			half4 c = tex2D(_CloudTexB, IN.uv_CloudTexB * 1.0 + (windCoords.xy*_cloudSpd.z));
			half d = 0;//tex2D(_MainTex, IN.uv_MainTex * 1.0 + (windCoords.xy*_cloudSpd.z)).a;
			//half m = tex2D(_CloudTexB, IN.uv_CloudTexB *20.0 + float2(1.0,_Time*(_cloudSpeed)+max(_Time*0.5,(_cloudSpeed*0.25)))).a;
			o.Albedo = fixed3(1,1,1);

			o.Alpha = lerp(0.0,c.r,saturate(lerp(0.0,4.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.g,saturate(lerp(-1.0,3.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.b,saturate(lerp(-2.0,2.0,_sizeCloud)));
			o.Alpha += lerp(0.0,c.a,saturate(lerp(-3.0,1.0,_sizeCloud)));
			o.Alpha += lerp(0.0,d,saturate(lerp(-4.0,1.0,_sizeCloud)));
			o.Alpha = saturate(o.Alpha*1.0);//*_amtCloudM;
			
			o.Alpha = lerp(-0.5,1.0,o.Alpha);
			o.Alpha *= tex2D(_BlendTex, IN.uv_BlendTex).r;

			half s = tex2D(_CloudTexB, IN.uv_CloudTexB * 3.0 + (windCoords.xy*3.0)).a;
			o.Albedo -= lerp(0,2.0,s);

			o.Albedo = saturate(o.Albedo);

			float4 uv0 = IN.screenPos; uv0.xy;
			uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));
			skyColor = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0));
			clip(o.Alpha-_clpCloud);

half dpth = max(IN.screenPos.w,0.001)/(_TenkokuDist);
depth = saturate(lerp(3,0,dpth));

depth = 1-saturate(max(IN.screenPos.w,0.001) / (_TenkokuDist * 0.15));

		}
		ENDCG

//#####  END LOW CLOUD  #####






//#####  OVERCAST CLOUD  #####
		Tags { "Queue"="Transparent-1" }
		Fog {Mode Off}
		Blend SrcAlpha OneMinusSrcAlpha
 		Offset 1,880000
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf CloudLight vertex:vert alpha noambient noforwardadd
		sampler2D _MainTex;
		sampler2D _BlendTex;
		float _cloudHeight;
		//float _cloudSpeed;
		float4 _colCloudO;
		float _clpCloud;
		float _AmbientShift;
		float4 windCoords;
		float _amtCloudO;
		float4 _TenkokuCloudColor,_Tenkoku_Daylight,_Tenkoku_Nightlight;
		float4 _TenkokuCloudHighlightColor;
		sampler2D _Tenkoku_SkyBox;
		float4 _Tenkoku_overcastColor;
		float4 skyColor,_cloudSpd;
		float4 Tenkoku_Vec_MoonFwd,Tenkoku_Vec_SunFwd,Tenkoku_Vec_LightningFwd;
		float _Tenkoku_Ambient;
		float depth;
		float _Tenkoku_shaderDepth;
	float _TenkokuDist;
float _brightMult;
		float4 _TenkokuAmbientColor;
		float Tenkoku_LightningIntensity;
		float Tenkoku_LightningLightIntensity;
		float4 Tenkoku_LightningColor;
		float blend;
float _tenkokuTimer;
float _FarDist;

		struct Input {
			float4 screenPos;	
			//float3 pos;
			float2 uv_MainTex;
			float2 uv_BlendTex;
		};
		fixed4 LightingCloudLight(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten){
			fixed4 col = half4(0,0,0,0);

			//base color
			col.rgb = _colCloudO.rgb * s.Albedo;

			//alpha
			col.a = saturate(s.Alpha * (_Tenkoku_overcastColor.a*3));
			col.a = saturate(col.a + (_Tenkoku_overcastColor.a*3));

			//overcast
			half skyFac = max(max(skyColor.r,skyColor.g),skyColor.b) * _Tenkoku_overcastColor.a;
			col.rgb = s.Albedo * lerp(_Tenkoku_Ambient*0.1,_Tenkoku_Ambient*0.35,saturate(skyFac*2));


			//skyColor.rgb = lerp(skyColor.rgb*0.58,skyColor.rgb*0.1,saturate(_Tenkoku_overcastColor.a*1.1));

			//skyColor.rgb += Tenkoku_LightningColor * Tenkoku_LightningLightIntensity * 1.4; 
			//col.rgb = saturate(lerp(skyColor.rgb * 1.15,col.rgb,depth));

skyColor.rgb += Tenkoku_LightningColor * Tenkoku_LightningLightIntensity * 1.4; 
col.rgb = saturate(lerp(skyColor.rgb,col.rgb,depth));

			//col.rgb = lerp(col.rgb,col.rgb*half3(0.8,0.89,0.9)*0.75,saturate(_Tenkoku_overcastColor.a*1.5));


//sunlight
//col.rgb *= lerp(1.0, 1.0 * _Tenkoku_Daylight.rgb * skyColor.rgb, saturate((dot(-viewDir,Tenkoku_Vec_SunFwd)-0.3)));//(1-depth) * blend * saturate(lerp(0,0.1*saturate(lerp(-0.1,1,(s.Albedo.r))),dot(-viewDir,Tenkoku_Vec_SunFwd)-0.1)) );


			col *= atten;

//lightning
half3 lCol = Tenkoku_LightningColor;//half3(0.89,0.85,0.68);
half cMask = saturate(lerp(-0.3,2.0,s.Albedo.r));


//bolt lightning
col.rgb += lCol * (Tenkoku_LightningLightIntensity * 0.2 * (1-s.Albedo.r));
col.rgb += lCol * saturate((saturate(dot(-viewDir,Tenkoku_Vec_LightningFwd.xyz)-0.2)) * Tenkoku_LightningLightIntensity * 0.7 * s.Albedo.r);

//cloud lightning
col.rgb += lCol * saturate((saturate(dot(-viewDir,Tenkoku_Vec_LightningFwd.xyz)-0.5)) * Tenkoku_LightningIntensity * 0.05 * s.Albedo.r * cMask);
col.rgb += lCol * saturate((saturate(dot(-viewDir,Tenkoku_Vec_LightningFwd.xyz)-0.4)) * Tenkoku_LightningIntensity * 1.0 * s.Albedo.r);

//lightning root
col.rgb += lCol * ((saturate(dot(-viewDir,Tenkoku_Vec_LightningFwd.xyz)-0.9998)) * Tenkoku_LightningLightIntensity * 2930.8 * s.Albedo.r);
col.rgb += ((saturate(dot(-viewDir,Tenkoku_Vec_LightningFwd.xyz)-0.9999)) * Tenkoku_LightningLightIntensity * 2930.8 * s.Albedo.r);

col.rgb = saturate(lerp(skyColor.rgb,col.rgb,depth));
//col.rgb = skyColor;
//float _FarDist = 1000;
//col.rgb = lerp(float3(col.rgb), float3(skyColor.rgb), depth ); //blend distant sky

			return col;
		}
		inline void vert (inout appdata_full v, out Input o){
 			UNITY_INITIALIZE_OUTPUT(Input,o);
			v.vertex.xyz += v.normal*4;
		}
		void surf (Input IN, inout SurfaceOutput o) {
			half c = tex2D(_MainTex, IN.uv_MainTex * 8.0 + (windCoords.xy*_cloudSpd.w)).r;


			o.Albedo = 0.6;

			half3 cl = fixed3(c,c,c);
			o.Albedo = lerp(o.Albedo,cl,saturate(_Tenkoku_overcastColor.a * 1.0));
			o.Alpha = saturate(lerp(0.0,2.0,saturate(_Tenkoku_overcastColor.a*1)));

			blend = tex2D(_BlendTex, IN.uv_BlendTex).r;

			float4 uv0 = IN.screenPos; uv0.xy;
			uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));
			skyColor = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0));

			half dpth = max(IN.screenPos.w,0.001)/_TenkokuDist;
			depth = saturate(lerp(3,0,dpth));


depth = 1-saturate(max(IN.screenPos.w,0.001) / (_TenkokuDist * 0.15));

		}
		ENDCG




//##### END OVERCAST CLOUD #####






	} 
}
