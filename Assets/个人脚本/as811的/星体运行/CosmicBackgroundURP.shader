Shader "AS811/Space/CosmicBackgroundURP"
{
    Properties
    {
        _TopColor("顶部背景色", Color) = (0.02, 0.04, 0.12, 1)
        _BottomColor("底部背景色", Color) = (0.005, 0.008, 0.02, 1)

        _NebulaColorA("星云颜色A", Color) = (0.28, 0.18, 0.45, 1)
        _NebulaColorB("星云颜色B", Color) = (0.12, 0.34, 0.48, 1)
        _NebulaScale("星云尺度", Range(0.5, 20)) = 4.5
        _NebulaIntensity("星云强度", Range(0, 4)) = 1.0
        _FlowSpeed("星云流动速度", Range(0, 2)) = 0.08

        _StarColor("恒星颜色", Color) = (1, 1, 1, 1)
        _StarDensity("恒星密度", Range(0.001, 0.2)) = 0.045
        _StarSize("恒星大小", Range(0.02, 0.6)) = 0.2
        _StarTwinkleSpeed("闪烁速度", Range(0, 10)) = 1.8

        _Exposure("整体亮度", Range(0, 5)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Background"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            Cull Front
            ZWrite Off
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor;
                half4 _BottomColor;

                half4 _NebulaColorA;
                half4 _NebulaColorB;
                half _NebulaScale;
                half _NebulaIntensity;
                half _FlowSpeed;

                half4 _StarColor;
                half _StarDensity;
                half _StarSize;
                half _StarTwinkleSpeed;

                half _Exposure;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float2 Hash22(float2 p)
            {
                float n = Hash21(p);
                float m = Hash21(p + n + 17.0);
                return frac(float2(n, m));
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = Hash21(i + float2(0.0, 0.0));
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float FBM(float2 p)
            {
                float value = 0.0;
                float amp = 0.5;

                value += ValueNoise(p) * amp;
                p = p * 2.02 + 13.1;
                amp *= 0.5;

                value += ValueNoise(p) * amp;
                p = p * 2.03 + 17.7;
                amp *= 0.5;

                value += ValueNoise(p) * amp;
                p = p * 2.01 + 11.3;
                amp *= 0.5;

                value += ValueNoise(p) * amp;

                return value;
            }

            float2 SphereUV(float3 dir)
            {
                dir = normalize(dir);
                float u = atan2(dir.z, dir.x) * (0.15915494309) + 0.5;
                float v = asin(dir.y) * (0.31830988618) + 0.5;
                return float2(u, v);
            }

            float StarLayer(float2 uv, float scale, float density, float size, float twinkleSpeed, float time)
            {
                float2 p = uv * scale;
                float2 cell = floor(p);
                float2 local = frac(p) - 0.5;

                float rnd = Hash21(cell);
                if (rnd > density)
                {
                    return 0.0;
                }

                float2 jitter = (Hash22(cell + 3.7) - 0.5) * 0.7;
                float2 d = local - jitter;
                float dist = length(d);

                float core = smoothstep(size, 0.0, dist);
                float halo = smoothstep(size * 2.2, 0.0, dist) * 0.35;
                float twinkle = 0.75 + 0.25 * sin(time * twinkleSpeed + rnd * 6.2831853);

                return (core + halo) * twinkle;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 viewDir = normalize(input.positionWS - _WorldSpaceCameraPos);
                float2 uv = SphereUV(viewDir);

                float t = _Time.y;
                float2 flow = float2(t * _FlowSpeed, -t * _FlowSpeed * 0.37);

                float2 nebulaUV = uv * _NebulaScale + flow;
                float n1 = FBM(nebulaUV);
                float n2 = FBM(nebulaUV * 1.9 + 21.3);
                float nebulaMask = saturate(n1 * 1.25 + n2 * 0.55 - 0.55);
                nebulaMask = pow(nebulaMask, 1.35);

                half3 bg = lerp(_BottomColor.rgb, _TopColor.rgb, saturate(uv.y));
                half3 nebulaColor = lerp(_NebulaColorA.rgb, _NebulaColorB.rgb, saturate(n2));
                half3 nebula = nebulaColor * nebulaMask * _NebulaIntensity;

                float stars = 0.0;
                stars += StarLayer(uv + float2(0.17, 0.08), 120.0, _StarDensity, _StarSize * 0.20, _StarTwinkleSpeed * 1.5, t);
                stars += StarLayer(uv + float2(0.33, 0.41), 200.0, _StarDensity * 0.75, _StarSize * 0.15, _StarTwinkleSpeed * 1.1, t);
                stars += StarLayer(uv + float2(0.57, 0.63), 320.0, _StarDensity * 0.50, _StarSize * 0.10, _StarTwinkleSpeed * 0.8, t);

                half3 color = bg + nebula + _StarColor.rgb * stars;
                color *= _Exposure;

                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
