Shader "Unlit/SkyboxProcUpdate"
{
    Properties
    {
        [Header(Stars Settings)]
        _Stars("Stars Texture", 2D) = "black"{}
        _StarsCutoff("Stars Cutoff", Range(0, 1)) = 0.08
        _StarsSpeed("Stars Move Speed", Range(0, 1)) = 0.3
        [HDR]_StarsSkyColor("Stars Sky Color", Color) = (0.0, 0.2, 0.1, 1)

        [Header(Horizon Settings)]
        _OffsetHorizon("Horizon Offset", Range(-1, 1)) = 0
        _HorizonWidth("Horizon Intensity", Range(0, 10)) = 3.3
        [HDR]_HorizonColorDay("Day Horizon Color", Color) = (0, 0.8, 1, 1)
        [HDR]_HorizonColorNight("Night Horizon Color", Color) = (0, 0.8, 1, 1)
        _HorizonCloudsFade("Fade at horizon", Vector) = (.25, .5, 0, 0)

        [Header(Sun Settings)]
        [HDR]_SunColor("Sun Color", Color) = (1, 1, 1, 1)
        _SunRadius("Sun Radius", Range(0, 2)) = 0.1

        [Header(Moon Settings)]
        [HDR]_MoonColor("Moon Color", Color) = (1, 1, 1, 1)
        _MoonRadius("Moon Radius", Range(0, 2)) = 0.15
        _MoonOffset("Moon Crescent", Vector) = (.25, .5, .5, 0)

      

        [Header(Main Cloud Settings)]
        _BaseNoise("Base Noise", 2D) = "black"{}
        _BaseNoiseSpeed("Base Noise Speed", Vector) = (.25, .5, 0, 0)
        _Distort("Distort", 2D) = "black"{}
        _SecNoise("Secondary Noise", 2D) = "black"{}
        _BaseNoiseScale("Base Noise Scale", Range(0, 1)) = 0.2
        _DistortScale("Distort Noise Scale", Range(0, 1)) = 0.06
        _SecNoiseScale("Secondary Noise Scale", Range(0, 1)) = 0.05
        _Distortion(" Distortion", Range(0, 1)) = 0.1
        _CloudsLayerSpeed("Movement Speed", Vector) = (.25, .5, 0, 0)
        _CloudCutoff("Cloud Cutoff", Range(0, 1)) = 0.3
        _Fuzziness("Cloud Fuzziness", Range(0, 1)) = 0.04

        [Header(Secondary Cloud Settings)]
        _CloudCutoff2("Cloud Cutoff Secondary", Range(0, 1)) = 0.3
        _Fuzziness2("Cloud Fuzziness Secondary", Range(0, 1)) = 0.04
        _SecNoiseScale("Secondary Noise Scale", Range(0, 1)) = 0.05
        _OpacitySec ("Secondary Layer Opacity", Range(0, 1)) = 0.04

        [Header(Cloud Color StretchOffset)]
        _ColorStretch("Color Stretch", Range(-10, 10)) = 0.01
        _ColorOffset("Color Offset", Range(-10, 10)) = 0.04

          [Header(Day Sky Settings)]
        [HDR]_DayTopColor("Day Sky Color Top", Color) = (0.4, 1, 1, 1)
        [HDR]_DayBottomColor("Day Sky Color Bottom", Color) = (0, 0.8, 1, 1)

        [Header(Day Clouds Settings)]
        [HDR]_CloudColorDayEdge("Clouds Edge Day", Color) = (1, 1, 1, 1)
        [HDR]_CloudColorDayMain("Clouds Main Day", Color) = (0.8, 0.9, 0.8, 1)
        
        
        [Header(Night Sky Settings)]
        [HDR]_NightTopColor("Night Sky Color Top", Color) = (0, 0, 0, 1)
        [HDR]_NightBottomColor("Night Sky Color Bottom", Color) = (0, 0, 0.2, 1)

        [Header(Night Clouds Settings)]
        [HDR]_CloudColorNightEdge("Clouds Edge Night", Color) = (0, 1, 1, 1)
        [HDR] _CloudColorNightMain("Clouds Main Night", Color) = (0, 0.2, 0.8, 1)
     
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
                // make fog work
          //  #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
               // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
            };

            sampler2D _Stars, _BaseNoise, _Distort, _SecNoise;

            float _SunRadius, _MoonRadius, _OffsetHorizon;
            float3 _MoonOffset;
            float4 _SunColor, _MoonColor;
            float4 _DayTopColor, _DayBottomColor, _NightBottomColor, _NightTopColor;
            float4 _HorizonColorDay, _HorizonColorNight;
            float _StarsCutoff, _StarsSpeed, _HorizonWidth;
            float _BaseNoiseScale, _DistortScale, _SecNoiseScale, _Distortion;
            float _CloudCutoff, _Fuzziness;
            float4 _CloudColorDayEdge, _CloudColorDayMain;
            float4 _CloudColorNightEdge, _CloudColorNightMain,  _StarsSkyColor;

            float _CloudCutoff2, _Fuzziness2;

            float2 _BaseNoiseSpeed, _CloudsLayerSpeed;

            float _ColorStretch, _ColorOffset;

            float2 _HorizonCloudsFade;

            float _OpacitySec;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
              //  UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            float4 remap(float4 In, float2 InMinMax, float2 OutMinMax)
            {
                return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            fixed4 frag(v2f i) : SV_Target
            {

                    // sun and moon section
                    // sun
                float sun = distance(i.uv.xyz, _WorldSpaceLightPos0);
                float sunDisc = 1 - (sun / _SunRadius);
                sunDisc = saturate(sunDisc * 50);
                float4 sunColored = sunDisc * _SunColor;

                    // (crescent) moon
                float moon = distance(i.uv.xyz, -_WorldSpaceLightPos0);
                float moonDisc = 1 - (moon / _MoonRadius);
                moonDisc = saturate(moonDisc * 50);

                float crescentMoon = distance(normalize(i.uv.xyz + _MoonOffset), -_WorldSpaceLightPos0);
                float crescentMoonDisc = 1 - (crescentMoon / _MoonRadius);
                crescentMoonDisc = saturate(crescentMoonDisc * 50);
                crescentMoonDisc = saturate(moonDisc - crescentMoonDisc);

                float4 moonColored = crescentMoonDisc * _MoonColor;

                float4 sunAndMoonColored = moonColored + sunColored;

                    // day night lerp
                float daynightLerp = saturate(_WorldSpaceLightPos0.y + 0.5);

                    // horizon
                float horizon = saturate(1 - abs((i.uv.y * _HorizonWidth) - _OffsetHorizon));
                float4 horizonColors = lerp(_HorizonColorNight, _HorizonColorDay, daynightLerp);

                    // top bottom sky lerp
                float topBottomSkyLerp = saturate(i.uv.y);

                    // day night sky colors

                float4 nightSkyColors = lerp(_NightBottomColor, _NightTopColor, topBottomSkyLerp);
                float4 daySkyColors = lerp(_DayBottomColor, _DayTopColor, topBottomSkyLerp);

                float4 dayNightColorsTogether = lerp(nightSkyColors, daySkyColors, daynightLerp);

                    // sky and horizon lerp
                float4 skyAndHorizonCombined = lerp(dayNightColorsTogether, horizonColors, horizon);

                    // sky/horizon and sun/moon
                float4 fullSkyCombined = skyAndHorizonCombined + sunAndMoonColored;

                    // sky uv for clouds
                float2 skyUV = i.worldPos.xz / abs(i.worldPos.y);

                    // baseNoise for distortion
                float baseNoise = tex2D(_BaseNoise, (skyUV + (_Time.x * _BaseNoiseSpeed)) * _BaseNoiseScale).r;

                    // clouds layer 1
                float clouds1 = tex2D(_Distort, ((skyUV + (baseNoise * _Distortion)) + (_Time.x * _CloudsLayerSpeed)) * _DistortScale).r;

                    // clouds Cutoff
                float cloudsCutoff = saturate(smoothstep(_CloudCutoff, _CloudCutoff + _Fuzziness, clouds1));

                    // stretch offset clouds color
                float cloudsStretch = saturate((clouds1 * _ColorStretch) + _ColorOffset);

                    // Clouds colors
                float4 cloudColorsDay = lerp(_CloudColorDayEdge, _CloudColorDayMain, cloudsStretch);
                float4 cloudColorsNight = lerp(_CloudColorNightEdge, _CloudColorNightMain, cloudsStretch);
                float4 cloudsColorLayer1 = lerp(cloudColorsNight, cloudColorsDay, daynightLerp);

                    // clouds layer 2
                float clouds2 = tex2D(_SecNoise, (skyUV + clouds1 + (_Time.x * _CloudsLayerSpeed * 0.5)) * _SecNoiseScale).r;

                    // clouds Cutoff 2
                float cloudsCutoff2 = saturate(smoothstep(_CloudCutoff2, _CloudCutoff2 + _Fuzziness2, clouds2)) * _OpacitySec;

                    // fade clouds in depth
                float fadeHorizon = remap(abs(i.uv.y), _HorizonCloudsFade, float2(0, 1));

                    // lerp value clouds
                float cloudsLerp = saturate((cloudsCutoff2 * fadeHorizon) + (cloudsCutoff * fadeHorizon * 2));

                    // stars
                float4 stars = tex2D(_Stars, (skyUV + (_WorldSpaceLightPos0.xz * 0.5)) * _StarsSpeed);
                stars *= saturate(-_WorldSpaceLightPos0.y * 5);
                stars = step(_StarsCutoff, stars);
                stars *= _StarsSkyColor;
                stars *= (1 - moonDisc);

                    // lerp clouds and sky
                float4 cloudsAndSky = lerp(fullSkyCombined + stars, cloudsColorLayer1, cloudsLerp);

                //UNITY_APPLY_FOG(i.fogCoord, cloudsAndSky);
                return (cloudsAndSky);

            }
            ENDCG
        }
    }
}
