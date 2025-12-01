// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'



Shader "TENKOKU/cloud_sphere"
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

            //float3 ray = mul((float3x3)_Object2World, v.vertex);
            float3 ray = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);

            ray = normalize(ray);
            
            o.rayDir = -ray;

            o.projPos = ComputeScreenPos(p);

            return o;
        }


        float _SampleCount0;
        float _SampleCount1;
        int _SampleCountL;

        sampler3D _NoiseTex1;
        sampler3D _NoiseTex2;
        sampler2D _CloudTex1;

        float _NoiseFreq1;
        float _NoiseFreq2;
        float _NoiseAmp1;
        float _NoiseAmp2;
        float _NoiseBias;
        float _NoiseBias2;
        float _NoiseBias3;

        float3 _Scroll1;
        float3 _Scroll2;

        float _Altitude0;
        float _Altitude1;
        float _Altitude2;
        float _Altitude3;
        float _Altitude4;
        float _Altitude5;
        float _FarDist;

        float _Scatter;
        float _HGCoeff;
        float _Extinct;
        float _Edge;
        float _Darkness;

        float _brightness;
        float _overBright;
        float quality;
        float _humid;

 sampler2D _Tenkoku_SkyBox, _Tenkoku_SkyTex;
 float4 Tenkoku_Vec_SunFwd;
 float4 Tenkoku_Vec_MoonFwd;
 float4 Tenkoku_Vec_LightningFwd;
 float Tenkoku_LightningLightIntensity;
 float4 Tenkoku_LightningColor;
 float4 _TenkokuSunColor;
 float4 Tenkoku_MoonLightColor;
 float4 _TenkokuCloudColor;
 float4 _TenkokuCloudAmbientColor;
 float _Tenkoku_Ambient;
 float _Tenkoku_AmbientGI;
 float4 _Tenkoku_overcastColor;
 float _Tenkoku_overcastAmt;
 float _cS;
 float _tenkokuTimer;
 float _tenkokuNoiseTimer;
 float _Tenkoku_UseElek;


        float UVRandom(float2 uv){
            float f = float(dot(float2(_tenkokuNoiseTimer, _tenkokuNoiseTimer), uv));
            return frac(43758.5453 * sin(f));
        }



        //cirro-stratus
        float SampleNoise3(float3 uvw){
            const float baseFreq = 1e-5;

            float4 uvw0 = float4(uvw * 3 * baseFreq, 0) * _cS;
            float4 uvw1 = float4(uvw * 2 * baseFreq, 0) * _cS;
            float4 uvw2 = float4(uvw * 1 * baseFreq, 0) * _cS;

            float3 scroll =  float3(0.9,0,0.15) * _tenkokuTimer * 0.25;
            uvw0.xyz += _Scroll1.xyz * scroll;
            uvw1.xyz += _Scroll1.xyz * scroll;
            uvw2.xyz += _Scroll1.xyz * scroll;

            uvw0.y = uvw0.z;
            uvw1.y = uvw1.z;
            uvw2.y = uvw2.z;

            float n0 = tex2Dlod(_CloudTex1, uvw0).b;
            float n1 = tex2Dlod(_CloudTex1, uvw1).g;
            float n2 = tex2Dlod(_CloudTex1, uvw2).a;
            float n =  n1 + n2 - n0 * 1.2;

            n = saturate(n + lerp(-0.1,1.25,_NoiseBias3));

            //float y = uvw.y - _Altitude4;
            //float h = _Altitude5 - _Altitude4;
            //n *= smoothstep(0, h * 0.1, y);
            //n *= smoothstep(0, h * 0.4, h - y);

            return n;
        }



        //alto-cumulus
        float SampleNoise2(float3 uvw){
            const float baseFreq = 1e-5;

            float4 uvw0 = float4(uvw * lerp(0.2,1.5,_NoiseBias2) * baseFreq, 0) * _cS;
            float4 uvw1 = float4(uvw * lerp(0.1,5,_NoiseBias2) * baseFreq, 0) * _cS;
            float4 uvw2 = float4(uvw * lerp(20,44,_NoiseBias2) * baseFreq, 0) * _cS;

            float3 scroll =  float3(1,0,0.25) * _tenkokuTimer * 1.4;
            uvw0.xyz += _Scroll1.xyz * scroll;
            uvw1.xyz += _Scroll1.xyz * scroll;
            uvw2.xyz += _Scroll2.xyz * scroll;

            float n0 = tex3Dlod(_NoiseTex1, uvw0 * float4(1,4,1,1)).a;
            float n1 = tex3Dlod(_NoiseTex2, uvw1).a;
            float n2 = tex3Dlod(_NoiseTex2, uvw2).a;
            float n =  n1 * _NoiseAmp1 + n2 * _NoiseAmp2 - n0 * 0.8;

            n = saturate(n + lerp(0.3,2.8,_NoiseBias2));

            float y = uvw.y - _Altitude2;
            float h = _Altitude3 - _Altitude2;
            n *= smoothstep(0, h * 0.1, y);
            n *= smoothstep(0, h * 0.4, h - y);



            float4 uvwX = float4(uvw * 3 * baseFreq, 0);
            float4 uvwX2 = float4(uvw * 2 * baseFreq, 0);
            uvwX.xyz += _Scroll1.xyz * scroll * 0.5;
            uvwX2.xyz += _Scroll1.xyz * scroll * 0.5;
            uvwX.y = uvwX.z;
            uvwX2.y = uvwX2.z;

            float nX = tex2Dlod(_CloudTex1, uvwX).b;
            float nX2 = tex2Dlod(_CloudTex1, uvwX2).g;
            //nX = saturate(nX + lerp(-0.1,1.25,_NoiseBias3));

            n = n * (nX - nX2);

            return n;
        }


        //cumulus
        float SampleNoise(float3 uvw){
            const float baseFreq = 1e-5;

            float4 uvw1 = float4(uvw * _NoiseFreq1 * baseFreq, 0) * _cS;
            float4 uvw2 = float4(uvw * _NoiseFreq2 * baseFreq, 0) * _cS;

            uvw1.xyz += _Scroll1.xyz * _tenkokuTimer;
            uvw2.xyz += _Scroll2.xyz * _tenkokuTimer;

            float n1 = tex3Dlod(_NoiseTex1, uvw1).a;
            float n2 = tex3Dlod(_NoiseTex2, uvw2).a;
            float n = n1 * _NoiseAmp1 + n2 * _NoiseAmp2;

            n = saturate(n + lerp(-0.4,2.15,_NoiseBias));

            float y = uvw.y - _Altitude0;
            float h = _Altitude1 - _Altitude0;
            n *= smoothstep(0, h * 0.1, y);
            n *= smoothstep(0, h * 0.4, h - y);

            return n;
        }



        float HenyeyGreenstein(float cosine){
            float g2 = _HGCoeff * _HGCoeff;
            return 0.5 * (1 - g2) / pow(1 + g2 - 1 * _HGCoeff * cosine, 2.0);
        }
        float Beer(float depth){
            return exp(-_Extinct * depth);
        }
        float BeerPowder(float depth){
            return exp(-_Extinct * depth) * (1 - exp(-_Extinct * 0.75 * depth));
        }



        float Trace3(float3 pos, float rand){
            float3 light = Tenkoku_Vec_SunFwd;//_WorldSpaceLightPos0.xyz;
            float stride = (_Altitude5 - pos.y) / (light.y);

            pos += light * stride * rand;

            float depth = 0;
            //UNITY_LOOP for (int s = 0; s < 1; s++)
            //{
                depth += SampleNoise3(pos) * stride;
                pos += light * stride;
           // }

            //prevent from negative
            if (depth <= 0.1){
                depth = 0.0;
            }

            return BeerPowder(depth);
        }



        float Trace2(float3 pos, float rand){
            float3 light = Tenkoku_Vec_SunFwd;//_WorldSpaceLightPos0.xyz;
            float stride = (_Altitude3 - pos.y) / (light.y);

            pos += light * stride * rand;

            float depth = 0;
            //UNITY_LOOP for (int s = 0; s < 1; s++)
            //{
                depth += SampleNoise2(pos) * stride;
                pos += light * stride;
           // }

            //prevent from negative
            if (depth <= 0.1){
                depth = 0.0;
            }

            return BeerPowder(depth);
        }



        float Trace(float3 pos, float rand){
            float3 light = Tenkoku_Vec_SunFwd;//_WorldSpaceLightPos0.xyz;
            float stride = (_Altitude1 - pos.y) / (light.y * _SampleCountL);

            pos += light * stride * rand;

            float depth = 0;
            UNITY_LOOP for (int s = 0; s < _SampleCountL; s++)
            {
                depth += SampleNoise(pos) * stride;
                pos += light * stride;
            }

            //prevent from negative
            if (depth <= 0.1){
                depth = 0.0;
            }

            return BeerPowder(depth);
        }



        float TraceDown(float3 pos, float rand){
            float3 up = half3(0,1,0);
            float stride = (_Altitude1 - pos.y) / (up.y * _SampleCountL);

            pos += up * stride * rand;

            float depth = 0;
            UNITY_LOOP for (int s = 0; s < _SampleCountL; s++)
            {
                depth += SampleNoise(pos) * stride;
                pos += up * stride;
            }

            //prevent from negative
            if (depth <= 0.1){
                depth = 0.0;
            }

            return BeerPowder(depth);
        }










        fixed4 frag(v2f i) : SV_Target{

            float3 acc = float3(0,0,0);


            //Set Base Settingd
            float3 light = Tenkoku_Vec_SunFwd.xyz;//_WorldSpaceLightPos0.xyz;
            float3 ray = -i.rayDir;
            int samples = lerp(int(_SampleCount1), int(_SampleCount0), ray.y);
            float2 uv = i.uv + _tenkokuTimer;


            //get sky color
            float4 uv0 = i.projPos; uv0.xy;
            uv0 = float4(max(0.001f, uv0.x),max(0.001f, uv0.y),max(0.001f, uv0.z),max(0.001f, uv0.w));

            float3 sky = float3(1,1,1);
            if (_Tenkoku_UseElek == 0.0){
                sky = tex2Dproj(_Tenkoku_SkyTex, UNITY_PROJ_COORD(uv0)).rgb;
            }
            if (_Tenkoku_UseElek == 1.0){
                sky = tex2Dproj(_Tenkoku_SkyBox, UNITY_PROJ_COORD(uv0)).rgb;
            }


            sky = sky * 0.8;


            //Set Distribution 2
            float dist0;
            float dist1;
            float stride;
            float hg;
            float offs;
            float3 pos;
            half3 wscPos = _WorldSpaceCameraPos;
            wscPos.y = 0;

            acc = lerp(float3(sky),float3(0,0,0),_Darkness);
            float3 acc2 = acc;
            float3 acc3 = acc;

            float underCloud = 0;


            if (ray.y < 0.01 || (0 / ray.y) >= _FarDist) return fixed4(sky,0);

            //RAY TRACE CUMULUS
            float depth = 0;
            float depth2 = 0;
            float depth3 = 0;
            float rand = UVRandom(uv);
            dist0 = _Altitude0 / ray.y;
            dist1 = _Altitude1 / ray.y;
            stride = (dist1 - dist0) / samples;
            offs = UVRandom(uv) * (dist1 - dist0) / samples;
            hg = HenyeyGreenstein(dot(ray, Tenkoku_Vec_SunFwd.xyz));
            hg += saturate(lerp(float(-3),float(3),hg));
            pos = wscPos + ray * (dist0 + offs);

            if (_Tenkoku_overcastAmt > 0.275) samples = 4;
                
            if (_NoiseBias > 0){
                UNITY_LOOP for (int ss = 0; ss < samples; ss++)
                {
                    float n = SampleNoise(pos);
                    if (n > _Edge){
                        float density = n * stride;
                        float rand = UVRandom(uv + ss);
                        float scatter = density * _Scatter * hg * Trace(pos, rand);
                        acc += _TenkokuCloudColor * (scatter) * BeerPowder(depth);

                            float scatterDn = density * _Scatter * hg * TraceDown(pos, rand);
                            underCloud += (scatterDn) * BeerPowder(depth);

                        depth += density;
                    }
                    pos += ray * stride;
                }
            }
            depth = Beer(depth*1.25);
            acc += (depth) * sky;

 

            //RAY TRACE ALTO-CUMULUS
            dist0 = _Altitude2 / ray.y;
            dist1 = _Altitude3 / ray.y;
            stride = (dist1 - dist0) / 20;
            offs = UVRandom(uv) * (dist1 - dist0) / 20;
            pos = wscPos + ray * (dist0 + offs);
            if (_NoiseBias2 > 0){
                UNITY_LOOP for (int s = 0; s < 4; s++)
                {
                    float n = SampleNoise2(pos);
                    if (n > _Edge){
                        float density = n * stride;
                        float scatter = density * Trace2(pos, rand);
                        depth2 += density;
                    }
                    pos += ray * stride;
                }
            }
            depth2 = Beer(depth2*1);
            acc2 += depth2 * sky;




            //RAY TRACE ALTO-CUMULUS
            dist0 = _Altitude4 / ray.y;
            dist1 = _Altitude5 / ray.y;
            stride = (dist1 - dist0) / 50;
            offs = UVRandom(uv) * (dist1 - dist0) / 50;
            pos = wscPos + ray * (dist0 + offs);
            if (_NoiseBias3 > 0){
                UNITY_LOOP for (int sss = 0; sss < 4; sss++){
                    float n = SampleNoise3(pos);
                    float density = n * stride;
                    float scatter = density * Trace3(pos, rand);
                    depth3 += density;
                    pos += ray * stride;
                }
            }
            depth3 = Beer(depth3*2);
            //acc3 += depth3 * sky;


           



            //CLOUD EDITS--------------------------------------------------------------------------
            half alph = saturate(depth * 4);
            float3 retCol = sky;
            float sunDot = float(dot(float3(normalize(ray)), float3(Tenkoku_Vec_SunFwd.xyz)));
            float moonDot = float(dot(float3(normalize(ray)), float3(Tenkoku_Vec_MoonFwd.xyz)));
            float lightFac = saturate(lerp(float(-0.75),float(1.25), sunDot) );
            float lightFac2 = saturate(lerp(float(-8.75),float(1.25), sunDot) );
            float lightFac3 = saturate(lerp(float(0),float(1), sunDot+0.9) );
            float moonFac = saturate(lerp(float(-1),float(1), moonDot-0.4) );
            float moonFac2 = saturate(lerp(float(-1),float(1), moonDot-0.1) );
            float moonFac3 = saturate(lerp(float(-1),float(1), moonDot-0.01) );

            float dayAmt = 1-max(max(_TenkokuSunColor.r,_TenkokuSunColor.g),_TenkokuSunColor.b);








            //Edit Cumulus Clouds
            depth = saturate(lerp(float(0),float(8.0),depth)); //darken depth
            float tColMult = lerp(float(0.6),float(0.45),_NoiseBias);
            acc = lerp(float3(_TenkokuCloudColor.xyz * float3(tColMult,tColMult,tColMult)), float3(_TenkokuCloudColor.xyz), acc.r)*_Tenkoku_AmbientGI; //tint cloud
            
            //acc = lerp( acc, acc + _TenkokuSunColor * 1.4, depth * 2 * lightFac); //brighten front clouds
            //acc = lerp( acc, _TenkokuCloudColor*2.2, depth * 2 * lightFac2); //brighten front clouds2

            acc = lerp(float3(acc), float3(acc*_Tenkoku_overcastColor.rgb), _Tenkoku_overcastAmt);
                acc = acc + (Tenkoku_MoonLightColor * underCloud * 0.015 * moonFac2 * dayAmt); //moon edge glow
                acc = acc + (Tenkoku_MoonLightColor * depth * 0.5 * moonFac2 * dayAmt); //moon edge glow

            float tcM2 = lerp(float(0.7),float(0.4),_NoiseBias);
            acc = lerp(float3(acc * float3(tcM2,tcM2,tcM2)), float3(acc), underCloud); //darken underside
            
            float3 tcM3 = lerp(float3(_TenkokuCloudAmbientColor.xyz)*float3(0.5,0.5,0.5)*float3(_Tenkoku_Ambient,_Tenkoku_Ambient,_Tenkoku_Ambient), float3(acc), saturate(underCloud + (1.0-lightFac3)));
            acc = lerp(float3(tcM3), float3(acc), _Tenkoku_AmbientGI); //under lighting (sunset)

            acc = acc * 1.4;



            //Edit AltoCumulus Clouds
            acc2 = lerp(float3(acc2), float3(_TenkokuCloudAmbientColor.xyz)*float3(2.5,2.5,2.5), lerp(float(1),float(0.7),_TenkokuCloudAmbientColor.a) )*_Tenkoku_AmbientGI;
            acc2 = lerp(float3(acc2), float3(sky), 0.5 ); //blend clouds
            acc2 = lerp(float3(acc2), float3(_TenkokuCloudAmbientColor.xyz)*float3(2.2,2.2,2.2), depth2 * 1.5 * lightFac); //brighten front clouds
            acc2 = lerp(float3(acc2), float3(_TenkokuCloudAmbientColor.xyz)*float3(5.4,5.4,5.4), depth2 * lightFac2); //brighten front clouds2
            acc2 = acc2 + (depth2 * Tenkoku_MoonLightColor * 0.5 * moonFac * dayAmt); //moon edge glow
            //acc2 = lerp(acc2 * _overBright, sky, saturate((2000.0 / ray.y) / _FarDist) ); //blend distant sky
            acc2 = acc2 * 1.2;            



            //Edit CirroStratus Clouds
            acc3 = float3(1,1,1);
            acc3 = acc3 * _TenkokuCloudAmbientColor*_Tenkoku_AmbientGI;
            acc3 = acc3 + (depth3 * Tenkoku_MoonLightColor * 0.1 * moonFac2 * dayAmt); //moon edge glow
            acc3 = acc3 * 2.4;


            //COMBINE CLOUD PASSES ---------
            depth = saturate(lerp(float(0.0),float(3.0),depth));
            depth2 = saturate(lerp(float(-1.0),float(1.0),depth2));

            depth3 = saturate(depth3);
            depth2 = saturate(depth2);
            depth = saturate(depth);

            retCol = lerp(float3(acc3), float3(retCol), depth3);
            retCol = lerp(float3(acc2), float3(retCol), depth2);
            retCol = lerp(float3(acc), float3(retCol), depth);
            //-----------------------------


            //COMBINE ALPHA PASSES --------
            depth3 = saturate(depth3 + lerp(float(1.0-_NoiseBias3),1.0-(depth*1),_NoiseBias3));
            alph = lerp(float(0.0),float(1.0), depth3);
            alph = lerp(float(0.1),alph,depth2);
            alph = lerp(float(0.0),alph,depth * float(1));
            alph = saturate(alph);
            //-----------------------------




            //COMBINE OVERCAST -------------
            float4 uvw1 = float4((wscPos + ray * (dist0 + offs)) * 1e-5, 0) * _cS * 3;
            float3 scroll = _tenkokuTimer * 2.0;

            uvw1.xyz += _Scroll1.xyz * scroll;
            uvw1.y = uvw1.z;

            float n = tex2Dlod(_CloudTex1, uvw1).r;
            float3 n1 = float3(n,n,n);

            float tcM4 = max(max(_TenkokuCloudColor.r,_TenkokuCloudColor.g),_TenkokuCloudColor.b);
            float3 ocColor = lerp(float3(_TenkokuCloudColor.rgb), float3(tcM4,tcM4,tcM4), 0.65);
            float tcM5 = lerp(float(0.6),float(0.45),_NoiseBias);
            n1 = lerp(float3(ocColor.xyz) * float3(tcM5,tcM5,tcM5), float3(ocColor.xyz), saturate(lerp(float(0.0),float(1),n1.r))); //tint cloud
            //n1 = lerp(ocColor * lerp(0.6,0.45,_NoiseBias), ocColor, saturate(lerp(-0.5,1,n1.r))); //tint cloud
           
            float tcM6 = lerp(float(1),float(0.5),_Tenkoku_overcastAmt);
            n1 = lerp(float3(n1), float3(n1)*float3(_Tenkoku_overcastColor.rgb)*float3(tcM6,tcM6,tcM6), saturate(_Tenkoku_overcastAmt * 2));
            n1 = lerp(float3(n1), float3(sky), saturate((1900.0 / ray.y) / _FarDist) ); //blend distant sky
            
            n1 = n1 + (Tenkoku_MoonLightColor * 0.015 * moonFac3 * dayAmt); //moon edge glow

            retCol = lerp(float3(retCol), float3(n1), saturate(_Tenkoku_overcastAmt*2));
            


           

            //------------------------------



            //lightning tint
            if (Tenkoku_LightningLightIntensity > 0){
                float lDot = float(saturate(dot(float3(Tenkoku_Vec_LightningFwd.xyz), float3(normalize(ray)))));
                float lVec = saturate(lDot - 0.3);
                float lVec2 = saturate(lDot - 0.65);
                float lVec3 = saturate(lDot - 0.9999);
                retCol += Tenkoku_LightningColor * Tenkoku_LightningLightIntensity * 0.25f;
                retCol += Tenkoku_LightningColor * (half(lVec*0.25) + half(lVec2*0.5) + half(lVec3 * 3000)) * Tenkoku_LightningLightIntensity;
            
            }

            //-------------------------------------------------------------------------------

//final fog overlay
float upDot = float(dot(float3(normalize(ray)), float3(0,1,0)));
float fogFac = saturate(lerp(float(0), float(3), upDot-0.025) );
retCol = lerp(float3(sky), float3(retCol), fogFac);





        //overlay humidity and fog
        retCol = lerp(float3(retCol), float3(sky), saturate(lerp(-1, 1, _humid)));


        //clamp brightness
        //retCol = clamp(retCol, float3(0,0,0), float3(_brightness,_brightness,_brightness));




        retCol = retCol * 1.25;






            //Final result
            return half4(retCol, 1-alph);
        }



        ENDCG
        }

    }
}
