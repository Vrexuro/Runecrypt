// Overlay kedipan untuk HitFlash: keluaran = SpriteRenderer.color × alpha tekstur sprite.
// Unlit, jadi kedipan terlihat juga di gelap. Pass Universal2D supaya dirender 2D Renderer.
// Di Unity 6 warna SpriteRenderer dikirim lewat unity_SpriteColor (bukan warna vertex) dan flip lewat
// unity_SpriteProps, sama seperti Sprite-Unlit-Default bawaan URP.
Shader "Runecrypt/SpriteFlash" {
  Properties { [PerRendererData] _MainTex ("Sprite", 2D) = "white" {} }
  SubShader {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
    Blend SrcAlpha OneMinusSrcAlpha  Cull Off  ZWrite Off
    Pass { Tags { "LightMode"="Universal2D" }
      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
      TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
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
      half4 frag (V i) : SV_Target { half a = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a; return half4(i.color.rgb, a * i.color.a); }
      ENDHLSL
    }
  }
}
