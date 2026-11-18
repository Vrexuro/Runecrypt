// Garis tepi 1 piksel untuk ExposedOutline: piksel transparan yang bertetangga (4 arah) dengan piksel sprite
// diberi _OutlineColor, sisanya transparan. Unlit, jadi garis tepi tetap terbaca di ruang expo yang terang.
// Butuh sprite dengan mesh FullRect (PixelArtImporter) supaya piksel di luar bentuk sprite ikut tergambar.
// Struktur vertex sama dengan SpriteFlash (unity_SpriteColor dan unity_SpriteProps di Unity 6).
Shader "Runecrypt/SpriteOutline" {
  Properties {
    [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
    _OutlineColor ("Outline", Color) = (1, 0.5412, 0.2392, 1)
  }
  SubShader {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
    Blend SrcAlpha OneMinusSrcAlpha  Cull Off  ZWrite Off
    Pass { Tags { "LightMode"="Universal2D" }
      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
      TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
      float4 _MainTex_TexelSize;
      half4 _OutlineColor;
      struct A { float4 pos : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
      struct V { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
      V vert (A i)
      {
        V o;
        SetUpSpriteInstanceProperties();
        float3 p = UnityFlipSprite(i.pos.xyz, unity_SpriteProps.xy);
        o.pos = TransformObjectToHClip(p);
        o.uv = i.uv;
        o.color = i.color * unity_SpriteColor;
        return o;
      }
      half A_(float2 uv) { return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a; }
      half4 frag (V i) : SV_Target
      {
        float2 t = _MainTex_TexelSize.xy;
        half self = A_(i.uv);
        half n = max(max(A_(i.uv + float2(t.x, 0)), A_(i.uv - float2(t.x, 0))),
                     max(A_(i.uv + float2(0, t.y)), A_(i.uv - float2(0, t.y))));
        half edge = (self < 0.01h && n > 0.01h) ? 1.0h : 0.0h;
        return half4(_OutlineColor.rgb, edge * _OutlineColor.a * i.color.a);
      }
      ENDHLSL
    }
  }
}
