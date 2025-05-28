Shader "Team-Like Team/Blur"
{
    Properties
    {
        [MainTexture] [NoScaleOffset] _MainTex ("Main Tex", 2D) = "white"
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
            #pragma vertex vert
            #pragma fragment frag

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            struct vert_out
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            vert_out vert(float4 vertex : POSITION, float2 uv : TEXCOORD0)
            {
                vert_out output;
                output.pos = mul(UNITY_MATRIX_MVP, vertex);
                output.uv = uv;
                return output;
            }

            float4 frag(float4 position : SV_Position, float2 uv : TEXCOORD0) : SV_TARGET
            {
                float2 texelSize = _MainTex_TexelSize.xy;

                float kernel[4] = { 0.05, 0.25, 0.4, 0.25 };

                float4 result = float4(0, 0, 0, 0);
                float totalWeight = 0.0;

                for (int j = 0; j < 4; j++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float2 offset = float2(i - 1, j - 1);
                        float2 coord = uv + offset * texelSize;
                        float w = kernel[i] * kernel[j];
                        result += _MainTex.SampleLevel(sampler_MainTex, coord, 0) * w;
                        totalWeight += w;
                    }
                }

                return result / totalWeight;
            }


            ENDHLSL
        }
    }
}