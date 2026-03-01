Shader "AS811/Star/CoronaFlareURP"
{
    Properties
    {
        _CoreColor("核心颜色", Color) = (1.0, 0.45, 0.12, 1.0)
        _CoronaColor("日冕颜色", Color) = (1.0, 0.75, 0.28, 1.0)
        _SnakeColor("火蛇颜色", Color) = (1.0, 0.95, 0.6, 1.0)

        _Intensity("整体强度", Range(0, 20)) = 3.5
        _Alpha("整体透明度", Range(0, 1)) = 0.55

        _RimPower("边缘锐度", Range(0.5, 12)) = 3.2
        _RimBoost("边缘增强", Range(0, 8)) = 1.8

        _NoiseScale("噪声尺度", Range(1, 80)) = 18
        _NoiseSpeed("噪声流动速度", Range(0, 8)) = 1.4
        _WarpStrength("流动扭曲强度", Range(0, 1)) = 0.22

        _SnakeCount("火蛇数量", Range(1, 24)) = 8
        _SnakeWidth("火蛇宽度", Range(0.01, 0.5)) = 0.12
        _SnakeSpeed("火蛇速度", Range(0, 8)) = 1.7
        _SnakeTwist("火蛇扭曲", Range(0, 8)) = 2.5
        _SnakePulse("火蛇脉动", Range(0, 2)) = 0.6
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Name "ForwardTransparent"
            Tags { "LightMode"="UniversalForward" }

            Cull Back
            ZWrite Off
            Blend One One

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _CoreColor;
                half4 _CoronaColor;
                half4 _SnakeColor;

                half _Intensity;
                half _Alpha;

                half _RimPower;
                half _RimBoost;

                half _NoiseScale;
                half _NoiseSpeed;
                half _WarpStrength;

                half _SnakeCount;
                half _SnakeWidth;
                half _SnakeSpeed;
                half _SnakeTwist;
                half _SnakePulse;
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
                float3 normalWS : TEXCOORD1;
            };

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 34.123);
                return frac(p.x * p.y);
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

            float2 SphereUV(float3 n)
            {
                n = normalize(n);
                float u = atan2(n.z, n.x) * (0.15915494309) + 0.5;
                float v = asin(n.y) * (0.31830988618) + 0.5;
                return float2(u, v);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionHCS = posInput.positionCS;
                output.positionWS = posInput.positionWS;
                output.normalWS = normalize(normalInput.normalWS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 n = normalize(input.normalWS);
                float3 v = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                float ndv = saturate(dot(n, v));

                float rim = pow(saturate(1.0 - ndv), _RimPower);

                float2 uv = SphereUV(n);
                float t = _Time.y;

                float2 flowUV = uv * _NoiseScale;
                flowUV += float2(t * _NoiseSpeed, -t * _NoiseSpeed * 0.37);

                float warpN = ValueNoise(flowUV * 0.9);
                float2 warpedUV = flowUV + (warpN - 0.5) * (_WarpStrength * 8.0);
                float detailN = ValueNoise(warpedUV * 1.35 + float2(1.7, 5.1));

                float phi = uv.x * 6.2831853;
                float theta = (uv.y - 0.5) * 3.1415926;

                float snakePhase = phi * _SnakeCount + t * _SnakeSpeed + sin(theta * _SnakeTwist + t * 0.7);
                float snakeWave = abs(sin(snakePhase));
                float snakeBody = smoothstep(1.0 - _SnakeWidth, 1.0, snakeWave);

                float snakeBreak = ValueNoise(float2(phi * 2.0, theta * 3.0) + t * 0.6);
                snakeBody *= smoothstep(0.2, 1.0, snakeBreak);

                float snakePulse = 0.5 + 0.5 * sin(t * (_SnakeSpeed + 0.8) + phi * 2.0 + detailN * 4.0);
                snakeBody *= lerp(0.7, 1.0 + _SnakePulse, snakePulse);

                float coronaMask = saturate(rim * (1.0 + detailN * 0.8 + warpN * 0.6));
                float coreMask = saturate((1.0 - rim) * (0.45 + detailN * 0.55));

                half3 color = 0;
                color += _CoreColor.rgb * coreMask * 0.35;
                color += _CoronaColor.rgb * coronaMask * (1.0 + _RimBoost * rim);
                color += _SnakeColor.rgb * snakeBody * coronaMask * 1.4;

                color *= _Intensity;
                float alpha = saturate(_Alpha * (coronaMask * 1.25 + snakeBody * 0.5));

                return half4(color * alpha, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
