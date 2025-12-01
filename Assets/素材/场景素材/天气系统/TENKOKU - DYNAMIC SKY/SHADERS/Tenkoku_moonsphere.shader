Shader "TENKOKU/moonsphere_shader" {
 Properties 
 {
 _PrimaryTint("Primary Tint", Color) = (1,1,1,1)
 _Color ("Main Color", Color) = (1,1,1,1)
 _AmbientTint ("Ambient Tint", Color) = (1,1,1,1)
 _MainTex ("Base (RGB)", 2D) = "white" {}

 _BRDFTex ("BRDF", 2D) = "white" {}
 _overBright ("OverBright", float) = 1.0
 _dispStrength ("Displace Amount", Range(0.0,3.0)) = 1.0
 _GlowColor ("Glow Color", Color) = (0.5,0.5,0.5,0.5)
 }
 
 SubShader 
 {
 
 

		
		
// Tags {"Queue"= "Overlay+8"}
Tags {"Queue"="Background+1605"}
 Cull Back
 Fog {Mode Off}

Offset 1,993000
Lighting Off
ZWrite Off

CGPROGRAM
#pragma surface surf MyLight alpha nofog noambient noforwardadd

 sampler2D _MainTex;
 fixed4 _AmbientTint;

 float4 _PrimaryTint;
 float4 Tenkoku_MoonLightColor;
 float4 Tenkoku_MoonHorizColor;
 float4 Tenkoku_Vec_SunFwd;
 sampler2D _Tenkoku_SkyTex;
 sampler2D _Tenkoku_SkyBox;
 float4 skyColor;
float4 _Tenkoku_overcastColor;
 float _Tenkoku_NightBright;
 float _Tenkoku_Ambient;
 float Tenkoku_MoonHFac;
 float _tenkokuIsLinear;

 

 struct Input{
 float4 screenPos;
 	float2 uv_MainTex;
 };


 void surf(Input IN, inout SurfaceOutput o){

 	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);

 	tex.rgb *= clamp(lerp(-0.25,3,tex.r),0,3);

	o.Albedo = tex.rgb;
 	o.Alpha = 1.0;

	float4 uv0 = IN.screenPos; uv0.xy;
	uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));
	skyColor = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0));//*0.96;

	half3 mTCol = lerp(half3(1,1,1), Tenkoku_MoonHorizColor.rgb * 2, Tenkoku_MoonHorizColor.a);
	o.Albedo = o.Albedo.rgb * _PrimaryTint.rgb * lerp( mTCol, 1.0, max(Tenkoku_MoonHFac,_Tenkoku_Ambient));

	//Extend Brightness Range
	o.Albedo.r = lerp(0.0, 2.5, o.Albedo.r);
	o.Albedo.g = lerp(0.0, 2.1, o.Albedo.g);
	o.Albedo.b = lerp(0.0, 2.0, o.Albedo.b);

	//gamma shift
	if (_tenkokuIsLinear == 0.0){
		o.Albedo = saturate(o.Albedo * 0.4646);
	}

 }



 fixed4 LightingMyLight(SurfaceOutput s, half3 lightDir, fixed atten){

	half4 light = half4(1,1,1,1);
	half alph = saturate(max(0.0,dot(s.Normal,Tenkoku_Vec_SunFwd.xyz)) * 2);

	light.rgb = lerp(skyColor.rgb, s.Albedo.rgb*max(0.5,Tenkoku_MoonLightColor)*lerp(2.0,5.0,_AmbientTint), alph);

	light.rgb = lerp(light.rgb,skyColor.rgb, min(0.8,_AmbientTint.r));

	//Max Light vs Sky
	light.rgb = max(skyColor.rgb,light.rgb);




	light.a = saturate(lerp(1,-3,_Tenkoku_overcastColor.a));

 	return light;
 }
 
      	

 ENDCG
 
 
 
 
 
 
 		
		
		
 
 }
 }