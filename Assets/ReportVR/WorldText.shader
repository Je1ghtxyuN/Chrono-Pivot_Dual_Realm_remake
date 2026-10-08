Shader "ReportVR/WorldText"
{
    Properties
    {
        _MainTex ("Font Atlas", 2D) = "white" {}
        _OutlineColor ("Outline", Color) = (.025,.065,.08,1)
        _OutlineWidth ("Outline width in atlas pixels", Range(0,3)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest LEqual
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; UNITY_VERTEX_OUTPUT_STEREO };
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            v2f vert(appdata v)
            {
                v2f o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float2 d=_MainTex_TexelSize.xy*_OutlineWidth;
                fixed glyph=tex2D(_MainTex,i.uv).a;
                fixed edge=max(max(tex2D(_MainTex,i.uv+float2(d.x,0)).a,tex2D(_MainTex,i.uv-float2(d.x,0)).a),
                               max(tex2D(_MainTex,i.uv+float2(0,d.y)).a,tex2D(_MainTex,i.uv-float2(0,d.y)).a));
                fixed alpha=max(glyph,edge*_OutlineColor.a);
                return fixed4(lerp(_OutlineColor.rgb,i.color.rgb,glyph/max(alpha,.001)),alpha*i.color.a);
            }
            ENDCG
        }
    }
}
