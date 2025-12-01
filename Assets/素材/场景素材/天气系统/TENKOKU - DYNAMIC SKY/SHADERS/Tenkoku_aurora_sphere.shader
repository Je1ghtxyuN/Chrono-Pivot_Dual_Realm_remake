// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TENKOKU/aurora_sphere"
{
    Properties
    {

        _AuroraTex("Aurora Texture", 2D) = ""{}
        _AuroraTexNormal("Aurora Normal Texture", 2D) = ""{}

        [Space]
        _Altitude4("Altitude 4 (bottom)", Float) = 5500
        _Altitude5("Altitude 5 (top)", Float) = 6000
        _FarDist("Far Distance", Float) = 30000

        [Space]
        aurSpd ("Aurora Speed", Range(0,1)) = 0.15
        aurScale ("Aurora Scale", Range(0,1)) = 0.394
        aurDefScale ("Aurora Deform Scale", Range(0,4)) = 1.53
        aurNormScale ("Aurora Normal Scale", Range(0,1)) = 0.884
        aurRepeat ("Aurora Repeat", Int) = 32
        aurSep ("Aurora Separation", Float) = 0.02
    }



    

    SubShader
    {

        Tags {"Queue"="Background+1600"}
        Cull Front
        Fog {Mode Off}
        Offset 1,80000
        Blend One One


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
            float2 uv : TEXCOORD0;
            float3 rayDir : TEXCOORD1;
            float4 projPos : TEXCOORD2;
        };

        #include "UnityCG.cginc"
        #include "Lighting.cginc"



        v2f vert(appdata_t v){
            float4 p = UnityObjectToClipPos(v.vertex);

            v2f o;

            o.vertex = p;
            o.uv = (p.xy / p.w + 1) * 0.5;

            float3 ray = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);

            ray = normalize(ray);
            
            o.rayDir = -ray;

            o.projPos = ComputeScreenPos(p);

            return o;
        }



        sampler2D _AuroraTex;
        sampler2D _AuroraTexNormal;

        float _Altitude4;
        float _Altitude5;
        float _FarDist;
        float _cS;
        float _tenkokuTimer;
        float _tenkokuNoiseTimer;

        float _Tenkoku_AuroraSpd;
        float _Tenkoku_AuroraAmt;
        float aurSpd;
        float aurScale;
        float aurDefScale;
        float aurNormScale;
        int aurRepeat;
        float aurSep;




        float UVRandom(float2 uv){
            float f = dot(float2(_tenkokuNoiseTimer, _tenkokuNoiseTimer), uv);
            return frac(43758.5453 * sin(f));
        }



        fixed4 frag(v2f i) : SV_Target{

            float3 acc = float3(0,0,0);


            //Set Base Settingd
            float3 ray = -i.rayDir;
            float2 uv = i.uv + _tenkokuTimer;
            half3 wscPos = _WorldSpaceCameraPos;
            wscPos.y = 0;
            half alph;// = depth;
            float3 retCol;// = sky;
            float dist0 = _Altitude4 / ray.y;
            float dist1 = _Altitude5 / ray.y;
            float offs = UVRandom(uv) * (dist1 - dist0) / 50;
            float3 auroraCol = float3(0,0,0);
            float ht;
            float4 uvwAN;
            float3 aN;
            float4 uvwA;
            float4 a1;
            float aurFac;
            aurSpd = aurSpd * _Tenkoku_AuroraSpd;


            //Early Return
            if (ray.y < 0.01 || (0 / ray.y) >= _FarDist) return fixed4(0,0,0,0);


            //Build Aurora buffer
            for (int ax = 0; ax < aurRepeat; ax++){

                ht = 2 - (aurSep * ax);
                uvwAN = float4((wscPos + ray * (dist0 + offs)) * 1e-5, -0.1) * _cS * ht;
                uvwAN.y = uvwAN.z;
                uvwAN.xy -= float2(0.0,_Time.x*aurSpd);
                uvwAN.xy *= aurNormScale;
                aN = (tex2Dlod(_AuroraTexNormal, uvwAN).rgb);

                uvwA = float4((wscPos + ray * (dist0 + offs)) * 1e-5, -0.1) * _cS * ht;
                uvwA.y = uvwA.z;
                uvwA.x += (aurDefScale * aN.x);
                uvwA.xy *= aurScale;
                uvwA.xy += float2(_Time.x*aurSpd*0.1,_Time.x*aurSpd);
                a1 = tex2Dlod(_AuroraTex, uvwA);

                a1.rgb = lerp(a1.rgb, half3(0,0,0), saturate((1900.0 / ray.y) / _FarDist) );

                aurFac = ((1.0/aurRepeat)*ax);
                auroraCol.rgb += (a1.rgb * aurFac);

            }
            //------------------------------

            float aurAlph = max(max(auroraCol.r, auroraCol.g), auroraCol.b);
            retCol.rgb = (auroraCol) * 2 * _Tenkoku_AuroraAmt;
            alph = 1.0;

        

            //Final result
            return half4(retCol, alph);
        }



        ENDCG
        }

    }
}
