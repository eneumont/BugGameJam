Shader "Custom/GlitchEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZTest Always Cull Off ZWrite Off

        Pass
        {
            Name "GlitchPass"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _Intensity;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 Frag (Varyings IN) : SV_Target
            {
                float glitchOffset = frac(sin(IN.uv.y * 100.0) * 43758.5453) * _Intensity * 0.1;
                float2 uv = IN.uv + float2(glitchOffset, 0);
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // RGB channel shift
                half r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(_Intensity * 0.02, 0)).r;
                half g = col.g;
                half b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - float2(_Intensity * 0.02, 0)).b;

                return half4(r, g, b, col.a);
            }
            ENDHLSL
        }
    }
}