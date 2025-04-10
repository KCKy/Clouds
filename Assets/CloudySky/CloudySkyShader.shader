Shader "Team-Like Team/Clouds"
{
   Properties
   {
       _Color ("Test Color", Color) = (1, 1, 1, 1)
       _MaxSteps ("Max Steps", Integer) = 150
       _StepSize ("Step Size", Float) = 0.06
       _OriginDepth ("Origin Depth", Float) = 3
       [NoScaleOffset] _Noise ("Noise Texture", 3D) = ""
       _NoiseFrequency ("Noise Frequency", Float) = 0.1
       _NoiseOctaves ("Noise Octaves", Integer) = 3
       [ShowAsVector3] _NoiseOctaveOffset ("Noise Octave Offset", Vector) = (17.37, 48.51, 192.21)
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
           float _NoiseFrequency;
           float _NoiseOctaves;
           float4 _NoiseOctaveOffset;
          
           TEXTURE3D(_Noise);
           SAMPLER(sampler_Noise);

           float signed_distance_sphere(float3 position, float radius)
           {
               return length(position) - radius;
           }

           float noise(float3 x)
           {
               return SAMPLE_TEXTURE3D(_Noise, sampler_Noise, x * _NoiseFrequency).r * 2. - 1;
           }

           float fractalNoise(float3 x)
           {
               float res = 0;
               int freq = 1;
               for (int i = 0; i < _NoiseOctaves; i++) {     
                   res += noise(x * freq + _NoiseOctaveOffset * i) / freq;
                   freq *= 2;
               }
               return res;
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
                   float density = max(scene(p), 0) * fractalNoise(p);
                   if (density > 0.0)
                   {
                       float4 color = _Color;
                       color.a *= density;
                       color.rgb *= color.a;
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
