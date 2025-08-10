Shader "Custom/DistortedPixelBubble"
{
    Properties
    {
        _MainTex ("Bubble Texture", 2D) = "white" {}
        _WaveSpeed ("Wave Speed", Float) = 1
        _WaveAmount ("Wave Amount", Float) = 0.05
        _ShimmerSpeed ("Shimmer Speed", Float) = 5
        _ShimmerIntensity ("Shimmer Intensity", Float) = 0.2
        _Color ("Base Color", Color) = (1, 0.4, 0.7, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float _WaveSpeed;
            float _WaveAmount;
            float _ShimmerSpeed;
            float _ShimmerIntensity;
            float4 _MainTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y * _WaveSpeed;

                float waveX = sin(uv.y * 20 + time) * _WaveAmount;
                float waveY = cos(uv.x * 20 + time * 1.5) * _WaveAmount;

                uv += float2(waveX, waveY);

                fixed4 texColor = tex2D(_MainTex, uv);

                float2 center = float2(0.5, 0.5);
                float dist = distance(uv, center);
                float gradient = smoothstep(0.5, 0.45, dist);

                float shimmer = (hash(uv * _Time.y * _ShimmerSpeed) - 0.5) * _ShimmerIntensity;

                float alpha = gradient + shimmer;
                alpha = saturate(alpha);

                fixed4 finalColor = _Color * texColor;
                finalColor.a *= alpha;

                return finalColor;
            }
            ENDCG
        }
    }
}
