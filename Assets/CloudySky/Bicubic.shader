Shader "Team-Like Team/Bicubic"
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
            
            float3 frag(float4 position : SV_Position, float2 uv : TexCoord0) : SV_TARGET
            {
                return float4(_MainTex.SampleLevel(sampler_MainTex, uv, 0));
            }
            ENDHLSL
        }
    }
}