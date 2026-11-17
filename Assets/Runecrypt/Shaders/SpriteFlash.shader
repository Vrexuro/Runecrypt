// Overlay kedipan untuk HitFlash: keluaran = warna vertex (SpriteRenderer.color) × alpha tekstur sprite.
// Unlit, jadi kedipan terlihat juga di gelap. Pass Universal2D supaya dirender 2D Renderer.
Shader "Runecrypt/SpriteFlash" {
  Properties { [PerRendererData] _MainTex ("Sprite", 2D) = "white" {} }
  SubShader {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
    Blend SrcAlpha OneMinusSrcAlpha  Cull Off  ZWrite Off
    Pass { Tags { "LightMode"="Universal2D" }
      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
      struct A { float4 pos : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
      struct V { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
      V vert (A i) { V o; o.pos = TransformObjectToHClip(i.pos.xyz); o.uv = i.uv; o.color = i.color; return o; }
      half4 frag (V i) : SV_Target { half a = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a; return half4(i.color.rgb, a * i.color.a); }
      ENDHLSL
    }
  }
}
