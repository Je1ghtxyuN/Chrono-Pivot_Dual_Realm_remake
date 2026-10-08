Shader "ChronoPivot/Water"
{
    Properties
    {
        _ShallowColor ("Shallow water", Color) = (0.16,0.46,0.43,1)
        _DeepColor ("Deep water", Color) = (0.025,0.17,0.18,1)
        _Environment ("Surrounding reflection", Cube) = "" {}
        _WaveAmplitude ("Wave height (m)", Float) = 0.008
        _Flow ("Flow x/z and speed", Vector) = (1,0,0.6,0)
        _Clarity ("Absorption distance (m)", Float) = 1.2
        _Opacity ("Base opacity", Range(0,1)) = 0.28
        _Ice ("Frozen surface", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthTexture);
            samplerCUBE _Environment;
            float4 _ShallowColor,_DeepColor,_Flow;
            float _WaveAmplitude,_Clarity,_Opacity,_Ice;
            float4 _Ripple0,_Ripple1,_Ripple2,_Ripple3;
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f
            {
                float4 pos:SV_POSITION;
                float3 world:TEXCOORD0;
                float3 normal:TEXCOORD1;
                float4 screen:TEXCOORD2;
                float eyeDepth:TEXCOORD3;
                UNITY_FOG_COORDS(4)
                UNITY_VERTEX_OUTPUT_STEREO
            };
            float ripple(float2 p,float4 r)
            {
                float age=_Time.y-r.z,d=length(p-r.xy);
                float envelope=exp(-max(age,0)*1.6)*exp(-pow(d-max(age,0)*0.8,2)*24);
                return r.w*sin(d*25-age*12)*envelope*step(0,age)*step(age,4);
            }
            float height(float2 p)
            {
                float t=_Time.y*_Flow.z;
                float2 drift=p-_Flow.xy*t;
                float waves=sin(dot(drift,float2(2.6,1.4))-t)+0.45*sin(dot(drift,float2(-3.4,4.2))+t*1.3);
                return (waves*_WaveAmplitude+ripple(p,_Ripple0)+ripple(p,_Ripple1)+ripple(p,_Ripple2)+ripple(p,_Ripple3))*(1-_Ice);
            }
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f,o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 world=mul(unity_ObjectToWorld,v.vertex).xyz;
                float3 n=UnityObjectToWorldNormal(v.normal);
                world.y+=height(world.xz)*saturate(abs(n.y));
                o.world=world;o.normal=n;
                o.pos=mul(UNITY_MATRIX_VP,float4(world,1));
                o.screen=ComputeScreenPos(o.pos);
                o.eyeDepth=-mul(UNITY_MATRIX_V,float4(world,1)).z;
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 uv=UnityStereoTransformScreenSpaceTex(i.screen.xy/i.screen.w);
                float depth=LinearEyeDepth(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraDepthTexture,uv).r);
                float thickness=max(0,depth-i.eyeDepth);
                float h=height(i.world.xz);
                float3 waveNormal=normalize(float3((h-height(i.world.xz+float2(.03,0)))/.03,1,(h-height(i.world.xz+float2(0,.03)))/.03));
                float3 n=normalize(lerp(i.normal,waveNormal*sign(i.normal.y),saturate(abs(i.normal.y))));
                float3 view=normalize(_WorldSpaceCameraPos-i.world);
                if(dot(n,view)<0)n=-n;
                float fresnel=.025+.975*pow(1-saturate(dot(view,n)),5);
                float absorption=1-exp(-thickness/max(.05,_Clarity));
                float3 color=lerp(_ShallowColor.rgb,_DeepColor.rgb,absorption);
                float3 reflected=texCUBE(_Environment,reflect(-view,n)).rgb;
                color=lerp(color,reflected,.12+.78*fresnel);
                float3 light=normalize(_WorldSpaceLightPos0.xyz);
                float spec=pow(saturate(dot(n,normalize(light+view))),lerp(180,85,_Ice));
                color+=_LightColor0.rgb*spec*.8;
                // Fine stable frost detail, separate from flowing liquid water.
                float crack=pow(saturate(sin(i.world.x*18+sin(i.world.z*7))*sin(i.world.z*23)),14)*_Ice;
                color=lerp(color,float3(.72,.88,.92),crack*.35);
                float alpha=saturate(_Opacity+absorption*.32+fresnel*.45+_Ice*.16);
                UNITY_APPLY_FOG(i.fogCoord,color);
                return float4(color,alpha);
            }
            ENDCG
        }
    }
    Fallback Off
}
