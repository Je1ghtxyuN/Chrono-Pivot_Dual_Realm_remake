Shader "Report VR/Ch46 Palette"
{
 Properties { _MainTex("Face and fabric detail",2D)="white" {} }
 SubShader
 {
  Tags { "RenderType"="Opaque" }
  LOD 200
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows addshadow
  #pragma target 3.0
  sampler2D _MainTex;
  struct Input { float2 uv_MainTex; float4 color:COLOR; };
  void surf(Input IN,inout SurfaceOutputStandard o)
  {
   float3 original=tex2D(_MainTex,IN.uv_MainTex).rgb;
   float detail=.65+.35*dot(original,float3(.2126,.7152,.0722));
   // Vertex masks keep skin and facial features; cyan hair becomes ink teal.
   float cyan=step(original.r*1.25,original.g)*step(original.r*1.25,original.b)*step(.12,original.g);
   float3 face=lerp(original,float3(.065,.12,.15)*detail,cyan);
   o.Albedo=lerp(face,IN.color.rgb*detail,IN.color.a);
   o.Metallic=0;o.Smoothness=.12;o.Alpha=1;
  }
  ENDCG
 }
 Fallback "Standard"
}
