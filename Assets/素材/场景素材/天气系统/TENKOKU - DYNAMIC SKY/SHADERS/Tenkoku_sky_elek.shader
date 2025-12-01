// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


// This shader was adapted by Justin Kellis / Tanuki Digital, using open source
// shader based on the Oskar Elek sky model implementation, adapted for Unity by
// Michael Skalsky.  Orginal disclamer is reproduced below:

//  Copyright(c) 2016, Michal Skalsky
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification,
//  are permitted provided that the following conditions are met:
//
//  1. Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//
//  2. Redistributions in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//
//  3. Neither the name of the copyright holder nor the names of its contributors
//     may be used to endorse or promote products derived from this software without
//     specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
//  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
//  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT
//  SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
//  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
//  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
//  TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
//  EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


Shader "TENKOKU/Tenkoku_Sky_Elek"
{


	Properties {
		//_SunSize ("Sun size", Range(0,1)) = 0.04
		//_AtmosphereThickness ("Atmoshpere Thickness", Range(0,5)) = 1.0
		//_SkyTint ("Sky Tint", Color) = (.5, .5, .5, 1)
		//_GroundColor ("Ground", Color) = (.369, .349, .341, 1)
		//_MieColor ("Mie Color", Color) = (1,1,1,1)
		_NightColor ("Night Color", Color) = (1,1,1,1)
		//_MoonColor ("Moon Mie Color", Color) = (1,1,1,1)
		//_Exposure("Exposure", Range(0, 8)) = 1.3
	}


	SubShader
	{
		Tags{ "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
		Cull Off ZWrite Off

		
		

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"
			#include "AtmosphericScattering.cginc"

			float3 _TenkokuCameraPos;
			float4 Tenkoku_Vec_SunFwd;
			float4 Tenkoku_Vec_MoonFwd;
			float _Tenkoku_Ambient;
			float _Tenkoku_AmbientGI;
			float4 tenkoku_globalTintColor;
			float4 tenkoku_globalSkyColor;
			float _Tenkoku_SkyBright;

			float4 _NightColor;
			//float4 _GroundColor;

			float _Tenkoku_NightBright;
			float4 _Tenkoku_overcastColor;
			float _Tenkoku_overcastAmt;
			float _Tenkoku_MnMieAmt;
			float _Tenkoku_MnIntensity;
			float _tenkokuIsLinear;


		float4 _testPosition;



			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4	pos		: SV_POSITION;
				float3	vertex	: TEXCOORD0;	
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.vertex = v.vertex;
				return o;
			}


			fixed4 frag (v2f i) : SV_Target
			{

			float3 lightVec = Tenkoku_Vec_SunFwd;
			_DensityScaleHeight = float4(10000.0, 5000.0, 0, 0);
			//_DensityScaleHeight = float4(7994.0f, 1200.0f, 0, 0);

			_IncomingLight = float4(3.6,3.9,6,4);



				float3 rayStart = _TenkokuCameraPos;
				float3 rayDir = normalize(mul((float3x3)unity_ObjectToWorld, i.vertex));

				float3 lightDir = -lightVec.xyz;

				float3 planetCenter = _TenkokuCameraPos;
				planetCenter = float3(0, -_PlanetRadius, 0);

				//REMOVED (use static ray length instead)
				//float2 intersection = RaySphereIntersection(rayStart, rayDir, planetCenter, _PlanetRadius + _AtmosphereHeight);		
				//float rayLength = intersection.y;
				//intersection = RaySphereIntersection(rayStart, rayDir, planetCenter, _PlanetRadius);
				//if (intersection.x > 0)
				//	rayLength = min(rayLength, intersection.x);

				float rayLength = 100000.0;

				float4 extinction;
				float4 inscattering = IntegrateInscattering(rayStart, rayDir, rayLength, planetCenter, 1, lightDir, 16, extinction);
			

			//Tenkoku Ambient Scatter
			float3 ambColor = float3(0.06,0.069,0.067) * 0.7 * _Tenkoku_Ambient;
			inscattering.rgb = max(ambColor, inscattering.rgb);


			//Tenkoku Eclipse Darkening
			float eclFac = saturate(lerp(0.1,1,_Tenkoku_EclipseFactor));
			float3 eclipseScattering = inscattering.rgb;
			float moonDiscFac = saturate(dot(rayDir,-lightDir)-0.25);
			float horizFac = saturate(saturate(dot(rayDir,half3(0,1,0))+0.15)*(1-moonDiscFac));
			eclipseScattering.rgb = lerp(half3(1.9,1+horizFac,0),inscattering.rgb,horizFac+moonDiscFac+0.25);
			eclipseScattering.rgb *= lerp(0.04,0.001, moonDiscFac);
			inscattering.rgb = lerp(half3(eclipseScattering.rgb), half3(inscattering.rgb), eclFac);

			//Tenkoky Final Tinting
			inscattering.rgb = inscattering.rgb * (_Tenkoku_SkyBright * 0.1);
			inscattering.rgb = lerp(inscattering.rgb, inscattering.rgb * tenkoku_globalSkyColor.rgb, tenkoku_globalSkyColor.a);
			inscattering.rgb = lerp(inscattering.rgb, inscattering.rgb * tenkoku_globalTintColor.rgb, tenkoku_globalTintColor.a);



			//Moon Mie
			float mmS = lerp(0.0,0.0075,_Tenkoku_MnMieAmt);
			float dotMoon = dot(float3(rayDir), float3(normalize(Tenkoku_Vec_MoonFwd.xyz)))+mmS;
			float3 moonMie = (dotMoon-0.9995) * 1.0;
			//if (_Tenkoku_AmbientGI < 0.1){
			moonMie += (dotMoon-0.999) * 1.0;
			moonMie += (dotMoon-0.997) * 1.0;
			moonMie += (dotMoon-0.990) * 0.75;
			moonMie += (dotMoon-0.97) * 0.5;
			inscattering.rgb += saturate(moonMie *  half3(0.2,0.28,0.4) * saturate(lerp(1.5,1.0,_Tenkoku_MnMieAmt))) * max(_Tenkoku_MnIntensity,0.01);
			//}




			//Night Brightening
			half3 nBright = half3(1.0,1.0,1.0);
			#if defined(UNITY_COLORSPACE_GAMMA)
				nBright = half3(0.027,0.02,0.025);
			#endif
			inscattering.rgb = max(inscattering.rgb, _NightColor.rgb * _Tenkoku_NightBright * nBright);


			//Night Horizon Brightening
			horizFac = saturate(lerp(0.0,2.0,dot(half3(0,-1,0), normalize(rayDir.xyz)-lerp(0.2,0.45,_Tenkoku_NightBright))));
			inscattering.rgb = max(inscattering.rgb,inscattering.rgb + half3(0.07,0.06,0.06)*horizFac*saturate(lerp(0.0,0.25,_Tenkoku_NightBright)));


			//ground color
			//half groundFac = saturate(lerp(0.0,8.0, dot(half3(0,-1,0), normalize(rayDir.xyz))));
			//inscattering.rgb = lerp(inscattering.rgb,inscattering.rgb*_GroundColor.rgb,groundFac*_GroundColor.a);

			//Overcast Color
			inscattering.rgb = lerp(inscattering.rgb, max(max(inscattering.r,inscattering.g),inscattering.b)*0.1, saturate(_Tenkoku_overcastAmt*3));





			//Gamma Shift
			if (_tenkokuIsLinear == 0.0){
				inscattering.rgb = inscattering.rgb * 2.2;
			}


			return float4(inscattering.xyz, 1);

			}
			ENDCG
		}
	}
}
