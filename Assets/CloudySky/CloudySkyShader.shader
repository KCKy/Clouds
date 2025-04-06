Shader "Team-Like Team/Clouds"
{
   Properties
   {
       _Color ("Test Color", Color) = (0, 0, 0, 0)
   }
   SubShader
   {
       Tags { "RenderPipeline" = "UniversalPipeline"}
       Pass
       {
           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

           #pragma vertex Vert
           #pragma fragment frag

           float4 _Color;

           float4 frag(Varyings input) : SV_Target0
           {
               return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, input.texcoord.xy, _BlitMipLevel) + _Color;
           }

           ENDHLSL
       }
   }
}
