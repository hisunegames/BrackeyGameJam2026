Shader "Hidden/PostProcessing/Posterize"
{
    Properties
    {
        _PosterizeSteps ("Posterize Steps", Float) = 6.0
        [Toggle(_POSTERIZE_ON)] _PosterizeOn ("Enable Posterize", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off Cull Off

        Pass
        {
            Name "PosterizePass"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_local _POSTERIZE_ON __

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _PosterizeSteps;

            float4 Frag(Varyings input) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
                
                #if defined(_POSTERIZE_ON)
                col.rgb = floor(col.rgb * _PosterizeSteps) / _PosterizeSteps;
                #endif
                
                return col;
            }
            ENDHLSL
        }
    }
}