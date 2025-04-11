Shader "Team-Like Team/Clouds"
{
   Properties
   {
       [MainColor] [HDR] _BaseColor ("Base Color", Color) = (0.66, 0.66, 0.825, 1)
       [HDR] _Sunlight ("Sunlight", Color) = (0.8, 0.48, 0.24, 1)
       [HDR] _AmbientLight ("Ambient Light", Color) = (0.1, 0.1, 0.1, 1)
       _MaxSteps ("Max Steps", Integer) = 150
       _StepSize ("Step Size", Float) = 0.06
       _PosOffset ("Cloud Pattern Offset", Vector) = (0, 0, 0, 0)
       _BallScale ("Ball Scale", Vector) = (1, 1, 1, 0)
       _DensityMultiplier ("Density Multiplier", Float) = 5
       [NoScaleOffset] _Noise ("Noise Texture", 3D) = ""
       [NoScaleOffset] _Dither ("Dither Texture", 2D) = ""
       _NoiseFrequency ("Noise Frequency", Float) = 0.1
       _NoiseOctaves ("Noise Octaves", Integer) = 3
       _NoiseOctaveOffset ("Noise Offset per Octave", Vector) = (0, 0, 0, 0)
       _SunDirection ("Sun Direction", Vector) = (0, 5, 1, 0)
       _MaxSunSteps ("Max Steps Towards Sun", Integer) = 10
       _SunSampleStep ("Sun Sampling Step", Float) = 0.3
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
           
           float4 _BaseColor;
           float4 _Sunlight;
           float4 _AmbientLight;
           int _MaxSteps;
           float _StepSize;
           float4 _PosOffset;
           float4 _BallScale;
           float _DensityMultiplier;
           float _NoiseFrequency;
           float _NoiseOctaves;
           float4 _NoiseOctaveOffset;
           float4 _SunDirection;
           int _MaxSunSteps;
           float _SunSampleStep;
          
           // TEXTURE2D(_BlitTexture); Already defined
           SAMPLER(sampler_BlitTexture);

           TEXTURE2D(_CameraDepthTexture);
           SAMPLER(sampler_CameraDepthTexture);
           
           TEXTURE3D(_Noise);
           SAMPLER(sampler_Noise);

           TEXTURE2D(_Dither);
           SAMPLER(sampler_Dither);

           float signed_distance_sphere(float3 position, float radius)
           {
               return length(position) - radius;
           }

           float noise(float3 x)
           {
               return _Noise.SampleLevel(sampler_Noise, x * _NoiseFrequency, 0).r * 2. - 1;
           }

           float fractalNoise(float3 x)
           {
               float res = 0;
               float factor = 2.02;
               float amplitude = 0.5;
               for (int i = 0; i < _NoiseOctaves; i++) {     
                   res += amplitude * noise(x + _NoiseOctaveOffset * i);
                   x *= factor;
                   factor += 0.21;
                   amplitude *= 0.5;
               }
               return res;
           }

           float scene(float3 position)
           {
               return fractalNoise(position + _PosOffset.xyz) - signed_distance_sphere(position / _BallScale, 1);
           }

           float raymarchTransmittedLight(float3 destination, float initialDensity) {
               float3 sunDir = normalize(_SunDirection.xyz);
               float depth = 0;
               float lastDensity = initialDensity;
               float intensity = 1;

               for (int i = 0; i < _MaxSteps; i++)
               {
                    float3 p = destination + depth * sunDir;
                    float step = _StepSize;
                    float density = scene(p);
                    float lineDensity = (density + lastDensity) * step * _DensityMultiplier;
                    if (lineDensity > 0.0)
                    {
                        float transparency = exp(-lineDensity);
                        intensity *= transparency;
                    }

                    depth += _StepSize;
                    lastDensity = density;
               }

               return intensity;
           }

           float3 getColor(float3 position, float density) {
               float light = raymarchTransmittedLight(position, density);
               return _BaseColor + _AmbientLight * clamp(1 - density, 0, 1) + _Sunlight * light;
           }

           // returns premultiplied color
           float4 raymarch(float3 origin, float3 direction, float startDepth, float maxDepth)
           {
               float3 result = 0.0;
               float depth = startDepth;
               float lastDensity = 0;
               float intensity = 1;

               for (int i = 0; i < _MaxSteps; i++)
               {
                    float3 p = origin + depth * direction;
                    float step = max(min(depth, maxDepth) - depth + _StepSize, 0);
                    float density = scene(p);
                    float lineDensity = (density + lastDensity) * step * _DensityMultiplier;
                    if (lineDensity > 0.0)
                    {
                        float transparency = exp(-lineDensity);
                        float3 color = getColor(p, density);
                        result += color * (1 - transparency) * intensity;
                        intensity *= transparency;
                    }

                    depth += _StepSize;
                    lastDensity = density;
               }

               return float4(result, 1 - intensity);
           }

           float4 frag(Varyings input) : SV_Target
           {
               float2 uv = input.texcoord.xy;
               float2 ndcuv = uv * 2.0 - 1.0; // NDC [-1, 1]

               float4 worldNear = mul(UNITY_MATRIX_I_VP, float4(ndcuv.x, -ndcuv.y, 1.0, 1.0));
               worldNear.xyz /= worldNear.w;

               float4 worldFar = mul(UNITY_MATRIX_I_VP, float4(ndcuv.x, -ndcuv.y, 0.0, 1.0));
               worldFar.xyz /= worldFar.w;

               float3 origin = worldNear.xyz;
               float3 direction = normalize(worldFar.xyz - worldNear.xyz);

               float rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
               float depth = LinearEyeDepth(rawDepth, _ZBufferParams);

               float startDepth = _StepSize * SAMPLE_TEXTURE2D(_Dither, sampler_Dither, uv * _ScreenParams.xy / 1024).r;

               float4 clouds = raymarch(origin, direction, startDepth, depth);
               float4 blit = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv);

               float3 color = blit.rgb * (1 - clouds.a) + clouds.rgb;

               return float4(color, 1.0);
           }

           ENDHLSL
       }
   }
}
