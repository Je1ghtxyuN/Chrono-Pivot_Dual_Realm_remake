Shader "AS811/Star/SunURP"
{
    Properties
    {
        _HotColor("高温颜色", Color) = (1.0, 0.78, 0.32, 1.0)
        _CoolColor("低温颜色", Color) = (1.0, 0.34, 0.05, 1.0)
        _RimColor("边缘辉光颜色", Color) = (1.0, 0.55, 0.15, 1.0)

        _EmissionIntensity("发光强度", Range(0, 30)) = 6.0
        _RimIntensity("边缘辉光强度", Range(0, 10)) = 1.2

        _NoiseScaleA("噪声尺度A", Range(1, 40)) = 10
        _NoiseScaleB("噪声尺度B", Range(1, 60)) = 24
        _FlowSpeedA("流动速度A", Range(0, 5)) = 0.6
        _FlowSpeedB("流动速度B", Range(0, 5)) = 1.1

        _PulseSpeed("脉动速度", Range(0, 10)) = 2.2
        _PulseStrength("脉动强度", Range(0, 1)) = 0.12

        _RimPower("边缘辉光锐度", Range(0.5, 12)) = 4.5
        _LimbDarkening("边缘压暗", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            Cull Back
            ZWrite On
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _HotColor;
                half4 _CoolColor;
                half4 _RimColor;
                half _EmissionIntensity;
                half _RimIntensity;
                half _NoiseScaleA;
                half _NoiseScaleB;
                half _FlowSpeedA;
                half _FlowSpeedB;
                half _PulseSpeed;
                half _PulseStrength;
                half _RimPower;
                half _LimbDarkening;
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
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
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
                float v = asin(saturate(n.y)) * (0.31830988618) + 0.5;

                if (n.y < 0.0)
                {
                    v = asin(n.y) * (0.31830988618) + 0.5;
                }

                return float2(u, v);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs nInput = GetVertexNormalInputs(input.normalOS);

                output.positionHCS = posInput.positionCS;
                output.positionWS = posInput.positionWS;
                output.normalWS = normalize(nInput.normalWS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));

                float2 uv = SphereUV(normalWS);
                float time = _Time.y;

                float2 flowA = uv * _NoiseScaleA + float2(time * _FlowSpeedA, time * _FlowSpeedA * 0.37);
                float2 flowB = uv * _NoiseScaleB + float2(-time * _FlowSpeedB * 0.23, time * _FlowSpeedB);

                float nA = ValueNoise(flowA);
                float nB = ValueNoise(flowB);
                float plasma = saturate(nA * 0.62 + nB * 0.38);

                float pulse = sin(time * _PulseSpeed + plasma * 6.28318) * _PulseStrength;
                float temperature = saturate(plasma + pulse);

                half3 surfaceColor = lerp(_CoolColor.rgb, _HotColor.rgb, temperature);

                float ndv = saturate(dot(normalWS, viewDirWS));
                float limb = lerp(1.0 - _LimbDarkening, 1.0, ndv);

                float rim = pow(saturate(1.0 - ndv), _RimPower) * _RimIntensity;
                half3 emission = surfaceColor * (_EmissionIntensity * limb) + _RimColor.rgb * rim;

                return half4(emission, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
