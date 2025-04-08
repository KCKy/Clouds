Shader "Team-Like Team/Clouds"
{
   Properties
   {
       _Color ("Test Color", Color) = (1, 1, 1, 1)
       _MaxSteps ("Max Steps", Integer) = 150
       _StepSize ("Step Size", Float) = 0.06
       _OriginDepth ("Origin Depth", Float) = 3
       [NoScaleOffset] _Noise ("Noise Texture", 3D) = ""
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
           int _MaxSteps;
           float _StepSize;
          
           TEXTURE3D(_Noise);
           SAMPLER(sampler_Noise);

           float signed_distance_sphere(float3 position, float radius)
           {
               return length(position) - radius;
           }

           float noise(float3 x)
           {
               return SAMPLE_TEXTURE3D(_Noise, sampler_Noise, x).r * 2. - 1;
           }

           float scene(float3 position)
           {
               return -signed_distance_sphere(position, 1);
           }

           float4 raymarch(float3 origin, float3 direction)
           {
               float4 result = 0.0;
               float depth = 0.0;

               for (int i = 0; i < _MaxSteps; i++)
               {
                   float3 p = origin + depth * direction;
                   float density = scene(p);
                   if (density > 0.0)
                   {
                       float4 color = SAMPLE_TEXTURE3D(_Noise, sampler_Noise, p) * density;
                       color.rgb *= _Color.a;
                       result += color * (1.0 - result.a);
                   }

                   depth += _StepSize;
               }

               return result;
           }

           float4 frag(Varyings input) : SV_Target0
           {
               float2 uv = input.texcoord.xy - 0.5f;
               uv.x *= _ScreenParams.x / _ScreenParams.y;
               float3 origin = float3(0, 0, 3);
               float3 direction = float3(uv, -1);
               return float4(raymarch(origin, direction).rgb, 1);
           }

           ENDHLSL
       }
   }
}
