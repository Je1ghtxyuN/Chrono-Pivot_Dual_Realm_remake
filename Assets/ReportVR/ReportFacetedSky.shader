Shader "ChronoPivot/Faceted Sky"
{
    Properties
    {
        _Zenith ("天顶", Color) = (.29,.65,.84,1)
        _Horizon ("地平线", Color) = (.93,.86,.72,1)
        _CloudColor ("云层", Color) = (1,.98,.9,1)
        _CloudCover ("云量", Range(0,1)) = .45
        _Stars ("星光", Range(0,1)) = 0
        _SkyRotation ("星空旋转", Vector) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos : SV_POSITION; float3 direction : TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };
            float4 _Zenith, _Horizon, _CloudColor, _SkyRotation;
            float _CloudCover, _Stars;
            v2f vert(appdata v)
            {
                v2f o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos=UnityObjectToClipPos(v.vertex); o.direction=v.vertex.xyz; return o;
            }
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float cloudLobe(float2 p, float2 center, float2 radius)
            {
                float2 v=abs((p-center)/radius);
                return max(.8660254*v.x+.5*v.y,v.y)-1;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float3 d=normalize(i.direction);
                float3 skyDirection=d+2*cross(_SkyRotation.xyz,cross(_SkyRotation.xyz,d)+_SkyRotation.w*d);
                float h=saturate(d.y);
                float3 color=lerp(_Horizon.rgb,_Zenith.rgb,pow(h,.58));
                // Longitude/elevation placement keeps the clouds upright on the horizon.
                // Four overlapping faceted lobes form each broad, flat-bottomed cloud.
                // Cell hashes only choose whole static clouds, never noisy surface detail.
                float2 cloudGrid=float2(atan2(skyDirection.x,skyDirection.z)*(12.0/6.28318530718),asin(clamp(skyDirection.y,-1,1))*(4.0/1.57079632679));
                cloudGrid.x+=floor(cloudGrid.y)*.38;
                float2 cell=floor(cloudGrid), p=frac(cloudGrid)-.5;
                p.x/=.86+.14*hash(cell+float2(4,7));
                p.y-=.03*(hash(cell+float2(9,2))-.5);
                float shape=cloudLobe(p,float2(-.27,-.02),float2(.17,.10));
                shape=min(shape,cloudLobe(p,float2(-.08,.06),float2(.20,.17)));
                shape=min(shape,cloudLobe(p,float2(.12,.09),float2(.19,.20)));
                shape=min(shape,cloudLobe(p,float2(.32,-.02),float2(.12,.11)));
                float cloud=(1-smoothstep(-.025,.025,shape))*smoothstep(-.13,-.115,p.y)*step(hash(cell),_CloudCover);
                cloud*=smoothstep(.04,.14,d.y)*(1-smoothstep(.67,.82,d.y));
                float3 cloudShade=lerp(_CloudColor.rgb*.90+_Zenith.rgb*.10,_CloudColor.rgb,step(-.025+.08*p.x,p.y));
                color=lerp(color,cloudShade,cloud*.82);
                // An integer number of longitude cells closes the sky seam.
                float2 starGrid=float2(atan2(skyDirection.x,skyDirection.z)*(288.0/6.28318530718),skyDirection.y*110);
                float2 starCell=floor(starGrid), starLocal=frac(starGrid)-.5;
                float star=(1-smoothstep(.025,.075,length(starLocal)))*step(.985,hash(starCell));
                color+=star*_Stars*smoothstep(.05,.3,d.y)*(1-smoothstep(.94,.995,d.y))*float3(.75,.84,1);
                return fixed4(color,1);
            }
            ENDCG
        }
    }
    Fallback Off
}
