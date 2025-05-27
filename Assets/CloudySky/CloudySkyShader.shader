Shader "Team-Like Team/Clouds"
{
    Properties
    {
        [HDR] _Sunlight ("Sunlight", Color) = (0.8, 0.48, 0.24, 1)
        [HDR] _AmbientLight ("Ambient Light", Color) = (0.1, 0.1, 0.1, 1)
        [NoScaleOffset] _StaticCloudMap ("Static Cloud Map", 2D) = "white"
        [NoScaleOffset] _CloudMap ("Cloud Map", 2D) = "white"
        _MaxSteps ("Max Steps", Integer) = 150
        _StepSize ("Base Step Size", Float) = 0.06
        _DensityMultiplier ("Density Multiplier", Float) = 5
        [NoScaleOffset] _Noise ("Noise Texture", 3D) = "white"
        [NoScaleOffset] _Dither ("Dither Texture", 2D) = "white"
        _NoiseFrequency ("Noise Frequency", Float) = 0.1
        _NoiseOctaves ("Maximum Noise Octaves", Integer) = 3
        _NoiseOctaveOffset ("Noise Offset per Octave", Vector) = (0, 0, 0, 0)
        _SunDirection ("Sun Direction", Vector) = (0, 5, 1, 0)
        _MaxSunSteps ("Max Steps Towards Sun", Integer) = 10
        _SunSampleStep ("Sun Sampling Step", Float) = 0.3
        _SunNoiseOctaves ("Sun Noise Octaves", Integer) = 2
        _UnitsPerTexel ("Units Per Texel", Float) = 1
        _StartDepth("Cloud Start Depth", float) = 1
        _CloudMapOffset("Cloud Map Offset", float) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }
        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            float3 _Sunlight;
            float3 _AmbientLight;
            int _MaxSteps;
            float _StepSize;
            float _DensityMultiplier;
            float _NoiseFrequency;
            float _NoiseOctaves;
            float _NoiseOctaveOffset;
            float3 _SunDirection;
            int _MaxSunSteps;
            float _SunSampleStep;
            int _SunNoiseOctaves;
            float _UnitsPerTexel;
            float _StartDepth;
            float _CloudMapOffset;

            // TEXTURE2D(_BlitTexture); Already defined
            SAMPLER(sampler_BlitTexture);

            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            TEXTURE3D(_Noise);
            SAMPLER(sampler_Noise);

            TEXTURE2D(_Dither);
            SAMPLER(sampler_Dither);
            float4 _Dither_TexelSize;

            TEXTURE2D(_CloudMap);
            SAMPLER(sampler_CloudMap);
            float4 _CloudMap_TexelSize;

            TEXTURE2D(_StaticCloudMap);
            SAMPLER(sampler_StaticCloudMap);

            float signed_distance_sphere(float3 position, float radius)
            {
                return length(position) - radius;
            }

            float fractalNoise(float3 x, int octaves)
            {
                float res = 0;
                float factor = 2.02;
                float amplitude = 0.5;
                for (int i = 0; i < octaves; i++)
                {
                    float3 pos = x + _NoiseOctaveOffset * i;
                    float noise = _Noise.SampleLevel(sampler_Noise, pos * _NoiseFrequency, 0).x * 2. - 1;
                    res += amplitude * noise;
                    x *= factor;
                    factor += 0.21;
                    amplitude *= 0.5;
                }
                return res;
            }

            float4 sample_cloud_map(float3 position)
            {
                float2 base = float2(position.x, position.y) / _UnitsPerTexel;
                float2 uv = base * _CloudMap_TexelSize.xy;
                float4 first = _CloudMap.SampleLevel(sampler_CloudMap, uv + float2(0, _CloudMapOffset), 0);
                float4 second = _StaticCloudMap.SampleLevel(sampler_StaticCloudMap, uv, 0);
                return float4(lerp(first, second, second.a / (first.a + second.a)));
            }

            float getDensity(float3 p, int quality)
            {
                return fractalNoise(p, quality) - 1 + sample_cloud_map(p).a;
            }

            float raymarchTransmittedLight(float3 destination, float initialDensity)
            {
                float3 sunDir = normalize(_SunDirection.xyz);
                float depth = 0;
                float lastDensity = initialDensity;
                float intensity = 1;
                float step = _SunSampleStep;
                for (int i = 0; i < _MaxSunSteps; i++)
                {
                    float3 p = destination + depth * sunDir;
                    float density = max(0, getDensity(p, _SunNoiseOctaves));
                    float lineDensity = (density + lastDensity) * step * _DensityMultiplier;
                    if (lineDensity > 0.0)
                    {
                        float transparency = exp(-lineDensity);
                        intensity *= transparency;
                    }
                    depth += step;
                    lastDensity = density;
                    step *= 2;
                }
                return intensity;
            }

            float3 getColor(float3 color, float3 position, float density)
            {
                float light = raymarchTransmittedLight(position, density);
                return color + _AmbientLight * clamp(1 - density, 0, 1) + _Sunlight * light;
            }

            float4 raymarch(float3 origin, float3 direction, float startDepth, float maxDepth)
            {
                float3 result = 0.0;
                float depth = startDepth;
                float lastDensity = 0;
                float intensity = 1;
                float totalDensity = 0;

                float maxStepSize = 0;

                for (int i = 0; i < _MaxSteps; i++)
                {
                    float3 p = origin + depth * direction;
                    float4 baseColor = sample_cloud_map(p);
                    float rawDensity = getDensity(p, _NoiseOctaves / (0.99 + totalDensity));
                    float density = max(0, rawDensity);
                    float step = max(min(depth, maxDepth) - depth + _StepSize, 0);
                    float lineDensity = (density + lastDensity) * step * _DensityMultiplier;
                    if (lineDensity > 0.0)
                    {
                        totalDensity += lineDensity;
                        float transparency = exp(-lineDensity);
                        float3 color = getColor(baseColor.rgb, p, density);
                        result += color * (1 - transparency) * intensity;
                        intensity *= transparency;
                    }

                    depth += _StepSize;
                    lastDensity = density;

                    maxStepSize = max(step, maxStepSize);
                }

                return float4(result, 1 - intensity);
            }

            struct ray
            {
                float3 origin;
                float3 direction;
            };

            ray get_ray(float2 uv)
            {
                float2 ndcuv = uv * 2.0 - 1.0; // NDC [-1, 1]

                float4 worldNear = mul(UNITY_MATRIX_I_VP, float4(ndcuv.x, -ndcuv.y, 1.0, 1.0));
                worldNear.xyz /= worldNear.w;

                float4 worldFar = mul(UNITY_MATRIX_I_VP, float4(ndcuv.x, -ndcuv.y, 0.0, 1.0));
                worldFar.xyz /= worldFar.w;

                ray result;
                result.origin = worldNear.xyz;
                result.direction = normalize(worldFar.xyz - worldNear.xyz);
                return result;
            }

            float get_linear_depth(float2 uv)
            {
                float rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
                return LinearEyeDepth(rawDepth, _ZBufferParams);
            }

            float get_start_depth(float2 uv)
            {
                return _StepSize * SAMPLE_TEXTURE2D(_Dither, sampler_Dither,
                    uv * _ScreenParams.xy * _Dither_TexelSize.xy).r + _StartDepth;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord.xy;
                ray ray = get_ray(uv);
                float depth = get_linear_depth(uv);
                float startDepth = get_start_depth(uv);

                float4 clouds = raymarch(ray.origin, ray.direction, startDepth, depth);

                float4 blit = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv);
                float3 color = blit.rgb * (1 - clouds.a) + clouds.rgb;

                return float4(color, 1.0);
            }
            ENDHLSL
        }
    }
}