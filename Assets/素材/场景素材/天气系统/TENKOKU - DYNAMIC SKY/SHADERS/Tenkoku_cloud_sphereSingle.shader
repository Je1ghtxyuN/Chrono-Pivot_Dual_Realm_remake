// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TENKOKU/cloud_sphereSingle"
{
    Properties
    {

        _overBright("OverBright", Range(0.0,4.0)) = 0.0
        [Space]

        _SampleCount0("Sample Count (min)", Float) = 30
        _SampleCount1("Sample Count (max)", Float) = 90
        _SampleCountL("Sample Count (light)", Int) = 16

        [Space]
        _NoiseTex1("Noise Volume", 3D) = ""{}
        _NoiseTex2("Noise Volume", 3D) = ""{}
        _CloudTex1("Cloud Texture", 2D) = ""{}

        _NoiseFreq1("Frequency 1", Float) = 3.1
        _NoiseFreq2("Frequency 2", Float) = 35.1
        _NoiseAmp1("Amplitude 1", Float) = 5
        _NoiseAmp2("Amplitude 2", Float) = 1
        _NoiseBias("Bias", Float) = -0.2
        _NoiseBias2("Bias 2", Float) = -0.2
        _NoiseBias3("Bias 3", Float) = -0.2

        [Space]
        _Scroll1("Scroll Speed 1", Vector) = (0.01, 0.08, 0.06, 0)
        _Scroll2("Scroll Speed 2", Vector) = (0.01, 0.05, 0.03, 0)

        [Space]
        _Altitude0("Altitude (bottom)", Float) = 1500
        _Altitude1("Altitude (top)", Float) = 3500
        _Altitude2("Altitude 2 (bottom)", Float) = 5500
        _Altitude3("Altitude 2 (top)", Float) = 6000
        _Altitude4("Altitude 4 (bottom)", Float) = 5500
        _Altitude5("Altitude 5 (top)", Float) = 6000
        _FarDist("Far Distance", Float) = 30000

        [Space]
        _Scatter("Scattering Coeff", Float) = 0.008
        _HGCoeff("Henyey-Greenstein", Float) = 0.5
        _Extinct("Extinction Coeff", Float) = 0.01

        [Space]
        _Edge("Edge", Range(0.0,1.0)) = 0.0
        _Darkness("Darkness", Range(0.0,1.0)) = 1.0

        [Space]
        _SunSize ("Sun Size", Range(0,1)) = 0.04
        _AtmosphereThickness ("Atmoshpere Thickness", Range(0,5)) = 1.0
        _SkyTint ("Sky Tint", Color) = (.5, .5, .5, 1)
        _GroundColor ("Ground", Color) = (.369, .349, .341, 1)
        _Exposure("Exposure", Range(0, 8)) = 1.3

    }

    //CGINCLUDE

    



    //ENDCG

    SubShader
    {

        Tags {"Queue"="Background+1605"}
        //Offset 1,993000

        //Tags { "Queue"="Transparent" "RenderType" = "Transparent"}
        Cull Front
         Fog {Mode Off}
        //ZWrite On
        Offset 1,80000
        Blend SrcAlpha OneMinusSrcAlpha



        Pass
        {

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 3.0


        struct appdata_t{
            float4 vertex : POSITION;
        };


        struct v2f{
            float4 vertex : SV_POSITION;
            float3 rayDir : TEXCOORD1;
        };

        #include "UnityCG.cginc"
        #include "Lighting.cginc"

        v2f vert(appdata_t v){
            float4 p = UnityObjectToClipPos(v.vertex);
            v2f o;
            o.vertex = p;
            float3 ray = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
            ray = normalize(ray);
            o.rayDir = ray;

            return o;
        }

        sampler2D _CloudTex1;

        fixed4 frag(v2f i) : SV_Target{

            //Coords
            half3 wscPos = _WorldSpaceCameraPos;
            wscPos.y = 0;

            //Sample Cloud Texture 0
            float depth0;
            float _Altitude0 = 7000.0;
            float _Extinct0 = -0.005;
            float4 uvw0 = float4( (wscPos + i.rayDir * (_Altitude0 / i.rayDir.y)) * 2 * (1e-5), 0);
            uvw0.y = uvw0.z;
            depth0 = tex2Dlod(_CloudTex1, uvw0).a * 100;
            depth0 = exp(_Extinct0 * (depth0*2));

            //CLOUD FINAL--------------------------------------------------------------------------
            float3 retCol = half3(0.8,0.8,0.8);
            half alph = saturate(lerp(float(0.0),float(1.0), depth0));

            //Final result
            return half4(retCol, 1-alph);
        }



        ENDCG
        }

    }
}
