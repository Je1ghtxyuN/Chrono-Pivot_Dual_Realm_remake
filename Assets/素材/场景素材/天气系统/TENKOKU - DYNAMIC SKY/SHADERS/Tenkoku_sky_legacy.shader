// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TENKOKU/Tenkoku_Sky_Legacy" {
Properties {
	_SunSize ("Sun size", Range(0,1)) = 0.04
	_AtmosphereThickness ("Atmoshpere Thickness", Range(0,5)) = 1.0
	_SkyTint ("Sky Tint", Color) = (.5, .5, .5, 1)
	_GroundColor ("Ground", Color) = (.369, .349, .341, 1)

	_MieColor ("Mie Color", Color) = (1,1,1,1)
	_NightColor ("Night Color", Color) = (1,1,1,1)

	_MoonColor ("Moon Mie Color", Color) = (1,1,1,1)


	_Exposure("Exposure", Range(0, 8)) = 1.3
}

SubShader {
	Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
	Cull Off ZWrite Off

	Pass {
		
		CGPROGRAM
		#pragma target 3.0
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"
		#include "Lighting.cginc"

		#pragma multi_compile __ UNITY_COLORSPACE_GAMMA

		uniform half _Exposure;
		uniform half4 _GroundColor;
		uniform half _SunSize;
		uniform half3 _SkyTint;
		uniform half3 _MieColor;
		uniform half3 _MoonColor;
		uniform half3 _NightColor;
		uniform half _AtmosphereThickness;

float _Tenkoku_SkyBright;
float _Tenkoku_NightBright;
float4 _TenkokuAmbientColor;
float4 _TenkokuSunColor;
float _Tenkoku_SunSize;
float4 tenkoku_globalTintColor;
float4 tenkoku_globalSkyColor;
float _Tenkoku_AtmosphereDensity;
float _Tenkoku_HorizonDensity;
float _Tenkoku_HorizonHeight;
float4 _Tenkoku_overcastColor;
float _Tenkoku_overcastAmt;
float _TenkokuExposureFac;
float _TenkokuColorFac;
float4 _TenkokuSkyColor;
float _Tenkoku_Ambient;
float _Tenkoku_AmbientGI;
float4 _Tenkoku_SkyHorizonColor;

sampler2D _Tenkoku_SkyTex;

sampler2D _GTex;



		#if defined(UNITY_COLORSPACE_GAMMA)
		#define GAMMA 2
		#define COLOR_2_GAMMA(color) color
		#define COLOR_2_LINEAR(color) color*color
		#define LINEAR_2_OUTPUT(color) sqrt(color)
		#else
		#define GAMMA 2.2
		// HACK: to get gfx-tests in Gamma mode to agree until UNITY_ACTIVE_COLORSPACE_IS_GAMMA is working properly
		#define COLOR_2_GAMMA(color) ((unity_ColorSpaceDouble.r>2.0) ? pow(color,1.0/GAMMA) : color)
		#define COLOR_2_LINEAR(color) color
		#define LINEAR_2_LINEAR(color) color
		#endif

		// RGB wavelengths
		static const float3 kDefaultScatteringWavelength = float3(.65, .57, .475);
		static const float3 kVariableRangeForScatteringWavelength = float3(.15, .15, .15);

		#define OUTER_RADIUS 1.025
		static const float kOuterRadius = OUTER_RADIUS;
		static const float kOuterRadius2 = OUTER_RADIUS*OUTER_RADIUS;
		static const float kInnerRadius = 1.0;
		static const float kInnerRadius2 = 1.0;
		static const float kCameraHeight = 0.0001;
		#define kMIE 0.0010      		// Mie constant
		#define kSUN_BRIGHTNESS 20.0 	// Sun brightness

		#define kMAX_SCATTER 50.0 // Maximum scattering value, to prevent math overflows on Adrenos

		static const half kSunScale = 100.0 * kSUN_BRIGHTNESS;
		static const float kKmESun = kMIE * kSUN_BRIGHTNESS;
		static const float kKm4PI = kMIE * 4.0 * 3.14159265;
		static const float kScale = 1.0 / (OUTER_RADIUS - 1.0);
		static const float kScaleDepth = 0.25;
		static const float kSamples = 2.0; // THIS IS UNROLLED MANUALLY, DON'T TOUCH

		#define MIE_G (-0.990)
		#define MIE_G2 0.9801


float _Tenkoku_MieAmt;
float _Tenkoku_MnMieAmt;

float4 Tenkoku_Vec_SunFwd;
float4 Tenkoku_Vec_MoonFwd;
float _tenkoku_rainbowFac1;

//float4 Tenkoku_Vec_LightningFwd;
//float Tenkoku_LightningIntensity;
float _Tenkoku_EclipseFactor;

		struct appdata_t {
			float4 vertex : POSITION;
		};

		struct v2f {
				float4 pos : SV_POSITION;
				half3 rayDir : TEXCOORD0;	// Vector for incoming ray, normalized ( == -eyeRay )
				half3 cIn : TEXCOORD1; 		// In-scatter coefficient
				half3 cOut : TEXCOORD2;		// Out-scatter coefficient
				half3 texcoord : TEXCOORD3;
   		}; 

		float scale(float inCos)
		{
			float x = 1.0 - inCos;
			return 0.25 * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
		}

		v2f vert (appdata_t v)
		{
			v2f OUT;
			UNITY_INITIALIZE_OUTPUT(v2f, OUT)
			OUT.pos = UnityObjectToClipPos(v.vertex);

			float3 kSkyTintInGammaSpace = COLOR_2_GAMMA(_SkyTint); // convert tint from Linear back to Gamma
			float3 kScatteringWavelength = lerp (
				kDefaultScatteringWavelength-kVariableRangeForScatteringWavelength,
				kDefaultScatteringWavelength+kVariableRangeForScatteringWavelength,
				half3(1,1,1) - kSkyTintInGammaSpace); // using Tint in sRGB gamma allows for more visually linear interpolation and to keep (.5) at (128, gray in sRGB) point
			float3 kInvWavelength = 1.0 / pow(kScatteringWavelength, 4);

			float3 cameraPos = float3(0,kInnerRadius + kCameraHeight,0); 	// The camera's current position
		
			// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
			float3 eyeRay = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));

			OUT.rayDir = half3(-eyeRay);
			

			half atFac = saturate( lerp( half(_Tenkoku_HorizonHeight), 1.0, half(dot(half3(0,3,0), (-eyeRay))) ));
			atFac = lerp(saturate(lerp(half(_Tenkoku_HorizonHeight),1.0,half(dot(lerp(half3(0,3,0),Tenkoku_Vec_SunFwd+half3(0.1,2.8,-0.1),_Tenkoku_Ambient), (-eyeRay))))), atFac, _Tenkoku_Ambient);

			_Tenkoku_AtmosphereDensity = lerp(_Tenkoku_AtmosphereDensity,_Tenkoku_AtmosphereDensity+_Tenkoku_HorizonDensity,atFac);
			float kRAYLEIGH = lerp(0.0, 0.0025, float(pow(_Tenkoku_AtmosphereDensity, 2.5)));
			float kScaleOverScaleDepth = (1.0 / (OUTER_RADIUS - 1.0)) / 0.075;


			float kKrESun = kRAYLEIGH * kSUN_BRIGHTNESS;
			float kKr4PI = kRAYLEIGH * 4.0 * 3.14159265;


			float far = 0.0;
			if(eyeRay.y >= 0.0)
			{
				// Sky
				// Calculate the length of the "atmosphere"
				far = sqrt(kOuterRadius2 + kInnerRadius2 * eyeRay.y * eyeRay.y - kInnerRadius2) - kInnerRadius * eyeRay.y;

				float3 pos = cameraPos + far * eyeRay;
				
				// Calculate the ray's starting position, then calculate its scattering offset
				float height = kInnerRadius + kCameraHeight;
				float depth = exp(kScaleOverScaleDepth * (-kCameraHeight));
				float startAngle = dot(eyeRay, cameraPos) / height;
				float startOffset = depth*scale(startAngle);
				
			
				// Initialize the scattering loop variables
				float sampleLength = far / kSamples;
				float scaledLength = sampleLength * kScale;
				float3 sampleRay = eyeRay * sampleLength;
				float3 samplePoint = cameraPos + sampleRay * 0.5;

				// Now loop through the sample rays
				float3 frontColor = float3(0.0, 0.0, 0.0);
				// Weird workaround: WP8 and desktop FL_9_1 do not like the for loop here
				// (but an almost identical loop is perfectly fine in the ground calculations below)
				// Just unrolling this manually seems to make everything fine again.
				// for(int i=0; i<int(kSamples); i++)
				{
					float height = length(samplePoint);
					float depth = exp(kScaleOverScaleDepth * (kInnerRadius - height));
					//float lightAngle = dot(_WorldSpaceLightPos0.xyz, samplePoint) / height;
					float lightAngle = dot(Tenkoku_Vec_SunFwd.xyz, samplePoint) / height;
					

					float cameraAngle = dot(eyeRay, samplePoint) / height;
					float scatter = (startOffset + depth*(scale(lightAngle) - scale(cameraAngle)));
					float3 attenuate = exp(-clamp(scatter, 0.0, kMAX_SCATTER) * (kInvWavelength * kKr4PI + kKm4PI));

					frontColor += attenuate * (depth * scaledLength);
					samplePoint += sampleRay;
				}
				{
					float height = length(samplePoint);
					float depth = exp(kScaleOverScaleDepth * (kInnerRadius - height));
					//float lightAngle = dot(_WorldSpaceLightPos0.xyz, samplePoint) / height;
					float lightAngle = dot(Tenkoku_Vec_SunFwd.xyz, samplePoint) / height;
					float cameraAngle = dot(eyeRay, samplePoint) / height;
					float scatter = (startOffset + depth*(scale(lightAngle) - scale(cameraAngle)));
					float3 attenuate = exp(-clamp(scatter, 0.0, kMAX_SCATTER) * (kInvWavelength * kKr4PI + kKm4PI));

					frontColor += attenuate * (depth * scaledLength);
					samplePoint += sampleRay;
				}



				// Finally, scale the Mie and Rayleigh colors and set up the varying variables for the pixel shader
				OUT.cIn.xyz = frontColor * (kInvWavelength * kKrESun);
				OUT.cOut = frontColor * kKmESun;
			}
			else
			{
				// Ground
				far = (-kCameraHeight) / (min(-0.001, eyeRay.y));

				float3 pos = cameraPos + far * eyeRay;

				// Calculate the ray's starting position, then calculate its scattering offset
				float depth = exp((-kCameraHeight) * (1.0/kScaleDepth));
				float cameraAngle = dot(-eyeRay, pos);
				float lightAngle = dot(Tenkoku_Vec_SunFwd.xyz, pos);
				float cameraScale = scale(cameraAngle);
				float lightScale = scale(lightAngle);
				float cameraOffset = depth*cameraScale;
				float temp = (lightScale + cameraScale);
				
				// Initialize the scattering loop variables
				float sampleLength = far / kSamples;
				float scaledLength = sampleLength * kScale;
				float3 sampleRay = eyeRay * sampleLength;
				float3 samplePoint = cameraPos + sampleRay * 0.5;
				
				// Now loop through the sample rays
				float3 frontColor = float3(0.0, 0.0, 0.0);
				float3 attenuate;
				{
					float height = length(samplePoint);
					float depth = exp(kScaleOverScaleDepth * (kInnerRadius - height));
					float scatter = depth*temp - cameraOffset;
					attenuate = exp(-clamp(scatter, 0.0, kMAX_SCATTER) * (kInvWavelength * kKr4PI + kKm4PI));
					frontColor += attenuate * (depth * scaledLength);
					samplePoint += sampleRay;
				}
			
				OUT.cIn.xyz = frontColor * (kInvWavelength * kKrESun + kKmESun);
				OUT.cOut.xyz = clamp(attenuate, 0.0, 1.0);


			}

			return OUT;

		}


		// Calculates the Mie phase function
		half getMiePhase(half eyeCos, half eyeCos2)
		{
			half temp = 1.0 + MIE_G2 - 2.0 * MIE_G * eyeCos;
			temp = smoothstep(0.0, 0.01, temp) * temp;
			temp = max(temp,1.0e-4); // prevent division by zero, esp. in half precision
			return 1.5 * ((1.0 - MIE_G2) / (2.0 + MIE_G2)) * (1.0 + eyeCos2) / temp;
		}

		// Calculates the Rayleigh phase function
		half getRayleighPhase(half eyeCos2)
		{
			return 0.75 + 0.75*eyeCos2;
		}

		half calcSunSpot(half3 vec1, half3 vec2, half size)
		{
			half3 delta = vec1 - vec2;
			half dist = length(delta);
			half spot = 1.0 - smoothstep(0.0, 0.02 * 10.0 * size, dist);
			return kSunScale * spot * spot;
		}



		half4 frag (v2f IN) : SV_Target
		{
			half3 col = half3(0.0, 0.0, 0.0);

			//Adjust color from HDR
			col *= (_Exposure);


			//custom
			col *= 1.0-(dot(Tenkoku_Vec_SunFwd.xyz, normalize(IN.rayDir.xyz))*0.7);

			col = lerp(col,half3(half3(col.r,col.r,col.r)*_Tenkoku_Ambient),saturate(_Tenkoku_overcastColor.a*10.0));

			//sky brightness
			col *= _Tenkoku_SkyBright;

			//adjust horizon atmosphere
			half atFac = saturate(lerp(half(0.2),half(0.6),dot(half3(0,1,0), normalize(IN.rayDir.xyz))))*1.0;
			_Tenkoku_AtmosphereDensity = lerp(float(1.0),(lerp(float(0.0),float(160.0),atFac)),atFac);


			half3 ray = normalize(IN.rayDir.xyz);
			half eyeCos = dot(Tenkoku_Vec_SunFwd.xyz, ray);
			half eyeCos2 = eyeCos * eyeCos;
			half mie = 0;

			if(IN.rayDir.y < 0.02){

					//custom sun
					mie = calcSunSpot(Tenkoku_Vec_SunFwd.xyz, -ray, 6.0)*0.006;
					//mie += calcSunSpot(Tenkoku_Vec_SunFwd.xyz, -ray, 0.08)*10.0;
					//mie += calcSunSpot(Tenkoku_Vec_SunFwd.xyz, -ray, 0.05)*10.0;

					mie += calcSunSpot(Tenkoku_Vec_SunFwd.xyz, -ray, 4.0)*0.008;
					mie += calcSunSpot(Tenkoku_Vec_SunFwd.xyz, -ray, 2.0)*0.02;
					mie += calcSunSpot(Tenkoku_Vec_SunFwd.xyz, -ray, 1.0)*0.05;
					mie += calcSunSpot(Tenkoku_Vec_SunFwd.xyz, -ray, 1.7)*0.001;
					mie *= _Tenkoku_AtmosphereDensity;
					mie += calcSunSpot(Tenkoku_Vec_SunFwd.xyz, -ray, 0.6)*0.001;
					mie = mie * lerp(half(0.0),half(1.0),_Tenkoku_EclipseFactor);


					//custom moon
					half Mmie = calcSunSpot(Tenkoku_Vec_MoonFwd.xyz, -ray, 4.0)*0.02;
					Mmie += calcSunSpot(Tenkoku_Vec_MoonFwd.xyz, -ray, 2.0)*0.03;
					Mmie *= _Tenkoku_AtmosphereDensity;


					mie = mie * lerp(1.0,0.0,_Tenkoku_overcastColor.a);

					float3 cN = IN.cIn.xyz;

					col = getRayleighPhase(eyeCos2) * (cN) * 1.0;

					half3 bcol = getRayleighPhase(eyeCos2) * IN.cIn.xyz;
					half nC = max(max(bcol.r,bcol.g),bcol.b);
					col = lerp(col, half3(nC, nC, nC), saturate(lerp(float(-0.5),float(1.0),tenkoku_globalSkyColor.a)));

					half skycoef = (lerp(half(-0.2), half(1.0), tenkoku_globalSkyColor.a));
					col = lerp(col, half3(nC, nC, nC), skycoef);

					half skyFac = saturate(lerp(half(1.0), half(0.15), dot(half3(0,1,0), normalize(IN.rayDir.xyz))))*2.0;
					col = lerp(col, half3(col*tenkoku_globalSkyColor.rgb), skyFac*tenkoku_globalSkyColor.a);

					_TenkokuSunColor.rgb = lerp(_TenkokuSunColor.rgb, float3(1,0.25,0), atFac);
					col = col * saturate(lerp(half(-1.0), half(2.0), _Tenkoku_Ambient));
					//custom lightning
					//half Lmie = saturate(calcSunSpot(Tenkoku_Vec_LightningFwd.xyz, -ray, 6.0)*0.0005);
					//half Lmie2 = (calcSunSpot(Tenkoku_Vec_LightningFwd.xyz, -ray, 0.1)*0.5);

					// If over horizon, add sun spot. otherwise lerp with ground
					if(IN.rayDir.y < 0.5){
						col += (mie * _Tenkoku_MieAmt) * _MieColor * _TenkokuSunColor * 2 * IN.cOut;

						#if defined(UNITY_COLORSPACE_GAMMA)
							col += (Mmie * _Tenkoku_MnMieAmt) * _MoonColor * (1.0-_TenkokuAmbientColor.r) * (_Tenkoku_NightBright*0.1) * 0.02;
						#else
							col += (Mmie * _Tenkoku_MnMieAmt) * _MoonColor * (1.0-_TenkokuAmbientColor.r) * (_Tenkoku_NightBright*0.1) * 0.5;
						#endif


					} else {
						half3 groundColor = IN.cIn.xyz + COLOR_2_LINEAR(_GroundColor) * IN.cOut;
						col = lerp(col, groundColor, IN.rayDir.y / 0.02);
					}
				}
				else
				{
					col = IN.cIn.xyz + COLOR_2_LINEAR(_GroundColor) * IN.cOut;
				}



			half3 nBright = half3(1.0,1.0,1.0);
			#if defined(UNITY_COLORSPACE_GAMMA)
				nBright = half3(0.027,0.02,0.025);
			#endif

			//night brightness
			col = max(col, _NightColor.rgb * _Tenkoku_NightBright * nBright);
			
			//overall tint
			col = lerp(col, half3(col * tenkoku_globalTintColor.rgb), tenkoku_globalTintColor.a);

			//tint sky during day
			col = lerp(col, half3(col * _TenkokuSkyColor.rgb), _Tenkoku_Ambient);


			//test horizon atmosphere
			half atmosFac = saturate(lerp(half(0.15), half(1.0), dot(half3(0,1,0), normalize(IN.rayDir.xyz))))*2.0;
			half3 atmosCol = lerp(col, half3(col*_Tenkoku_SkyHorizonColor.rgb*lerp(1.0,4.0,_TenkokuAmbientColor.r)), atmosFac);
			col = atmosCol;

			//night brightness horizon
			half3 temp = half3(_NightColor.rgb * 4.0) * saturate(atmosFac * saturate(lerp(half(0.0), half(4.0), _Tenkoku_NightBright))) * (1.0 - _Tenkoku_Ambient) * nBright;
			col += half3(lerp(half(0.0), temp.r, 1.0 - _Tenkoku_Ambient), lerp(half(0.0), temp.g, 1.0 - _Tenkoku_Ambient), lerp(0.0, temp.b, 1.0 - _Tenkoku_Ambient));

			//brightness control
			col = col * lerp(half(1.0), half(1.2*_Tenkoku_SkyBright), _Tenkoku_Ambient);

			//reset ground color
			half groundFac = saturate(lerp(half(0.0), half(8.0), dot(half3(0,1,0), normalize(IN.rayDir.xyz))));
			col = lerp(col,col*_GroundColor.rgb,groundFac*_GroundColor.a);

			//overcast tint
			half mC = half(max(max(col.r,col.g),col.b)*0.1);
			col = lerp(col, half3(mC,mC,mC), saturate(_Tenkoku_overcastAmt*2));

			//Add Rainbows
			half3 origCol = col;
			half3 rainCol = half3(0,0,0);
			half rS = 30.0;
			half rVec = saturate(dot(Tenkoku_Vec_SunFwd.xyz, normalize(IN.rayDir.xyz)) - 0.2);
			rainCol = lerp(rainCol, half3(2,0,0), saturate(lerp(0.0-rS,rS, rVec * 1.32))); //red
			rainCol = lerp(rainCol, half3(0,2,0), saturate(lerp(0.0-rS-0.2,rS, rVec * 1.21))); //green
			rainCol = lerp(rainCol, half3(0,0,2), saturate(lerp(0.0-rS-0.3,rS, rVec * 1.1))); //green
			rainCol = lerp(rainCol, half3(0,0,0), saturate(lerp(0.0-rS-0.4,rS, rVec * 1.01)));

			//overall lighting during eclipse
			col = col * saturate(lerp(half(-0.045), half(1.0), _Tenkoku_EclipseFactor));


			#if defined(UNITY_COLORSPACE_GAMMA)
				col = LINEAR_2_OUTPUT(col*5.0);
			#endif
 

			return half4(col,1.0);
		}
		ENDCG 
	}
} 	


Fallback Off

}
