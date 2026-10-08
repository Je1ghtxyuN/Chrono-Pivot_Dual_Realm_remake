Shader "ChronoPivot/Low Poly Baked"
{
    Properties
    {
        _Color ("Interactive tint", Color) = (1,1,1,1)
        _BakeTint ("Original palette tint", Color) = (1,1,1,1)
        _MainTex ("Baked palette modulation", 2D) = "white" {}
        _DetailStrength ("Baked texture strength", Range(0,1)) = 0.35
        _OcclusionMap ("Baked local AO", 2D) = "white" {}
        _OcclusionStrength ("AO strength", Range(0,1)) = 0.65
        _MetallicGlossMap ("R metal / G roughness / A smoothness", 2D) = "white" {}
        _GlossMapScale ("Smoothness scale", Range(0,1)) = 1
        [Toggle(_SURFACE_DETAIL)] _UseSurfaceDetail ("精细表面", Float) = 0
        _SurfaceDetail ("细节 RGB / 粗糙度 A", 2D) = "white" {}
        [Normal] _SurfaceNormal ("细节法线", 2D) = "bump" {}
        _SurfaceScale ("每米细节重复次数", Range(0.1,8)) = 1.25
        _SurfaceStrength ("细节颜色强度", Range(0,1)) = 0.7
        _SurfaceNormalStrength ("细节法线强度", Range(0,1)) = 0.2
        _SurfaceRoughness ("粗糙度变化强度", Range(0,0.5)) = 0.1
        [HDR] _EmissionColor ("Emission", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.5
        #pragma multi_compile_instancing
        #pragma shader_feature_local _SURFACE_DETAIL
        sampler2D _MainTex, _OcclusionMap, _MetallicGlossMap;
        sampler2D _SurfaceDetail, _SurfaceNormal;
        fixed4 _Color;
        half4 _EmissionColor;
        half _OcclusionStrength, _GlossMapScale, _DetailStrength;
        half _SurfaceScale, _SurfaceStrength, _SurfaceNormalStrength, _SurfaceRoughness;
        struct Input
        {
            float2 uv_MainTex;
            float3 localPosition;
            float3 localNormal;
            float3 worldNormal;
            INTERNAL_DATA
        };
        void vert(inout appdata_full v,out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.localPosition=v.vertex.xyz;
            o.localNormal=v.normal;
        }
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Keep the original _Color contract used by switches and learning apparatus.
            o.Albedo=lerp(1,tex2D(_MainTex,IN.uv_MainTex).rgb,_DetailStrength)*_Color.rgb;
            fixed4 surface=tex2D(_MetallicGlossMap,IN.uv_MainTex);
            o.Metallic=surface.r;
            o.Smoothness=surface.a*_GlossMapScale;
            #ifdef _SURFACE_DETAIL
            // Project onto the dominant object-space face: detail follows moving props,
            // avoids tiny packed UV islands, and keeps the low-poly silhouette untouched.
            float3 n=normalize(IN.localNormal),a=abs(n),uAxis,vAxis;
            float2 detailUV;
            if(a.y>=a.x && a.y>=a.z)
            {detailUV=IN.localPosition.xz;uAxis=float3(1,0,0);vAxis=float3(0,0,1);}
            else if(a.x>=a.z)
            {detailUV=IN.localPosition.zy;uAxis=float3(0,0,1);vAxis=float3(0,1,0);}
            else
            {detailUV=IN.localPosition.xy;uAxis=float3(1,0,0);vAxis=float3(0,1,0);}
            detailUV*=_SurfaceScale;
            fixed4 detail=tex2D(_SurfaceDetail,detailUV);
            o.Albedo*=lerp(1,detail.rgb*1.08,_SurfaceStrength);
            o.Smoothness=saturate(o.Smoothness-(detail.a-.5)*_SurfaceRoughness);
            float3 bump=UnpackNormal(tex2D(_SurfaceNormal,detailUV));
            float3 offset=uAxis*bump.x+vAxis*bump.y;
            offset-=n*dot(n,offset);
            float3 worldBump=UnityObjectToWorldNormal(normalize(n+offset*_SurfaceNormalStrength));
            o.Normal=normalize(float3(
                dot(worldBump,normalize(WorldNormalVector(IN,float3(1,0,0)))),
                dot(worldBump,normalize(WorldNormalVector(IN,float3(0,1,0)))),
                dot(worldBump,normalize(WorldNormalVector(IN,float3(0,0,1))))));
            #endif
            o.Occlusion=lerp(1,tex2D(_OcclusionMap,IN.uv_MainTex).g,_OcclusionStrength);
            o.Emission=_EmissionColor.rgb;
            o.Alpha=1;
        }
        ENDCG
    }
    FallBack "Standard"
}
