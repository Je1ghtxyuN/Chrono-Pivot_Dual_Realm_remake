// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// TENKOKU NOTE: This has been modified from PlayDead's original public implementation.
// For more info please see original implementation here: https://github.com/playdeadgames/temporal

// Copyright (c) <2015> <Playdead>
// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE.TXT)
// AUTHOR: Lasse Jon Fuglsang Pedersen <lasse@playdead.com>


Shader "Hidden/Tenkoku_TemporalReprojection"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	//note: normalized random, float=[0;1[
	float PDnrand( float2 n ) {
		return frac( sin(dot(n.xy, float2(12.9898f, 78.233f)))* 43758.5453f );
	}
	float4 PDnrand4( float2 n ) {
		return frac( sin(dot(n.xy, float2(12.9898f, 78.233f)))* float4(43758.5453f, 28001.8384f, 50849.4141f, 12996.89f) );
	}

	//====
	//note: signed random, float=[-1;1[
	float PDsrand( float2 n ) {
		return PDnrand( n ) * 2 - 1;
	}
	float4 PDsrand4( float2 n ) {
		return PDnrand4( n ) * 2 - 1;
	}



	static const float FLT_EPS = 0.00000001f;

	// uniform float4x4 _CameraToWorld;
	uniform sampler2D _CameraDepthTexture;
	uniform float4 _CameraDepthTexture_TexelSize;

	uniform sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;

	uniform sampler2D _VelocityBuffer;
	uniform sampler2D _VelocityNeighborMax;

	uniform float4 _Corner;// xy = ray to (1,1) corner of unjittered frustum at distance 1
	uniform float4 _Jitter;// xy = current frame, zw = previous
	uniform float4x4 _PrevVP;
	uniform sampler2D _PrevTex;
	uniform float _FeedbackMin;
	uniform float _FeedbackMax;
	float fullScreenTemporal;

	struct v2f
	{
		float4 cs_pos : SV_POSITION;
		float2 ss_txc : TEXCOORD0;
		float2 vs_ray : TEXCOORD1;
	};

	v2f vert(appdata_img IN)
	{
		v2f OUT;

		OUT.cs_pos = UnityObjectToClipPos(IN.vertex);
		OUT.ss_txc = IN.texcoord.xy;
		OUT.vs_ray = (2.0 * IN.texcoord.xy - 1.0) * _Corner.xy;
		
		return OUT;
	}


	float3 find_closest_fragment(float2 uv)
	{
		float2 dd = _CameraDepthTexture_TexelSize.xy;
		float2 du = float2(dd.x, 0.0);
		float2 dv = float2(0.0, dd.y);

		float3 dtl = float3(-1, -1, tex2D(_CameraDepthTexture, uv - dv - du).x);
		float3 dtc = float3( 0, -1, tex2D(_CameraDepthTexture, uv - dv).x);
		float3 dtr = float3( 1, -1, tex2D(_CameraDepthTexture, uv - dv + du).x);

		float3 dml = float3(-1,  0, tex2D(_CameraDepthTexture, uv - du).x);
		float3 dmc = float3( 0,  0, tex2D(_CameraDepthTexture, uv).x);
		float3 dmr = float3( 1,  0, tex2D(_CameraDepthTexture, uv + du).x);

		float3 dbl = float3(-1,  1, tex2D(_CameraDepthTexture, uv + dv - du).x);
		float3 dbc = float3( 0,  1, tex2D(_CameraDepthTexture, uv + dv).x);
		float3 dbr = float3( 1,  1, tex2D(_CameraDepthTexture, uv + dv + du).x);

		float3 dmin = dtl;
		if (dmin.z > dtc.z) dmin = dtc;
		if (dmin.z > dtr.z) dmin = dtr;

		if (dmin.z > dml.z) dmin = dml;
		if (dmin.z > dmc.z) dmin = dmc;
		if (dmin.z > dmr.z) dmin = dmr;

		if (dmin.z > dbl.z) dmin = dbl;
		if (dmin.z > dbc.z) dmin = dbc;
		if (dmin.z > dbr.z) dmin = dbr;

		return float3(uv + dd.xy * dmin.xy, dmin.z);
	}


	float2 sample_velocity_dilated(sampler2D tex, float2 uv, int support)
	{
		float2 du = float2(_CameraDepthTexture_TexelSize.x, 0.0);
		float2 dv = float2(0.0, _CameraDepthTexture_TexelSize.y);
		float2 mv = 0.0;
		float rmv = 0.0;

		int end = support + 1;
		for (int i = -support; i != end; i++)
		{
			for (int j = -support; j != end; j++)
			{
				float2 v = tex2D(tex, uv + i * dv + j * du).xy;
				float rv = dot(v, v);
				if (rv > rmv)
				{
					mv = v;
					rmv = rv;
				}
			}
		}

		return mv;
	}

	float4 sample_color_motion(sampler2D tex, float2 uv, float2 ss_vel)
	{
		const float2 v = 0.5 * ss_vel;
		const int taps = 3;// on either side!

		float srand = PDsrand(uv + _SinTime.xx);
		float2 vtap = v / taps;
		float2 pos0 = uv + vtap * (0.5 * srand);
		float4 accu = 0.0;
		float wsum = 0.0;

		for (int i = -taps; i <= taps; i++)
		{
			float w = 1.0f;// box
			accu += w * tex2D(tex, pos0 + i * vtap);
			wsum += w;
		}

		return accu / wsum;
	}

	float4 temporal_reprojection(float2 ss_txc, float2 ss_vel, float vs_dist)
	{

		float4 texel0 = tex2D(_MainTex, ss_txc);
		float4 texel1 = tex2D(_PrevTex, ss_txc - ss_vel);
		float2 uv = ss_txc;
		float2 du = float2(_MainTex_TexelSize.x, 0.0);
		float2 dv = float2(0.0, _MainTex_TexelSize.y);

		float4 ctl = tex2D(_MainTex, uv - dv - du);
		float4 ctc = tex2D(_MainTex, uv - dv);
		float4 ctr = tex2D(_MainTex, uv - dv + du);
		float4 cml = tex2D(_MainTex, uv - du);
		float4 cmc = tex2D(_MainTex, uv);
		float4 cmr = tex2D(_MainTex, uv + du);
		float4 cbl = tex2D(_MainTex, uv + dv - du);
		float4 cbc = tex2D(_MainTex, uv + dv);
		float4 cbr = tex2D(_MainTex, uv + dv + du);

		float4 cmin = min(ctl, min(ctc, min(ctr, min(cml, min(cmc, min(cmr, min(cbl, min(cbc, cbr))))))));
		float4 cmax = max(ctl, max(ctc, max(ctr, max(cml, max(cmc, max(cmr, max(cbl, max(cbc, cbr))))))));

		texel1 = clamp(texel1, cmin, cmax);

		float lum0 = Luminance(texel0.rgb);
		float lum1 = Luminance(texel1.rgb);

		float unbiased_diff = abs(lum0 - lum1) / max(lum0, max(lum1, 0.2));
		float unbiased_weight = 1.0 - unbiased_diff;
		float unbiased_weight_sqr = unbiased_weight * unbiased_weight;
		float k_feedback = lerp(_FeedbackMin, _FeedbackMax, unbiased_weight_sqr);

		return lerp(texel0, texel1, k_feedback);
	}

	struct f2rt
	{
		fixed4 buffer : SV_Target0;
		fixed4 screen : SV_Target1;
	};

	f2rt frag( v2f IN )
	{
		f2rt OUT;


		//TENKOKU Performance mask
		// Added to reduce pixel fill performance down to only sky areas
		float dpth = Linear01Depth(tex2D(_CameraDepthTexture, IN.ss_txc).x);
		if (dpth < 1.0){

			OUT.screen = tex2D(_MainTex, IN.ss_txc);
			OUT.buffer = tex2D(_MainTex, IN.ss_txc);

		} else {

			float2 uv = IN.ss_txc;
			float2 ss_vel = tex2D(_VelocityBuffer, uv).xy;
			float vs_dist = LinearEyeDepth(tex2D(_CameraDepthTexture, uv).x);

			// temporal resolve
			float4 color_temporal = temporal_reprojection(IN.ss_txc, ss_vel, vs_dist);

			// add noise
			float4 noise4 = PDsrand4(IN.ss_txc + _SinTime.x + 0.6959174) / 510.0;
			OUT.buffer = saturate(color_temporal + noise4);
			OUT.screen = saturate(color_temporal + noise4);

		}//End Tenkoku Performance Check

		// done
		return OUT;
	}

	//--- program end
	ENDCG

	SubShader
	{
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma only_renderers ps4 xboxone d3d11 d3d9 xbox360 opengl glcore
			#pragma target 3.0
			#pragma glsl
			
			ENDCG
		}
	}

	Fallback off
}
