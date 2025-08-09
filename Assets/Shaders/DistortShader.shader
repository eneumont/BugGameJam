Shader "Custom/PsychedelicEffect"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Distortion("Distortion", Range(0, 1)) = 0.3
        _Speed("Speed", Float) = 0.5
        _HueShiftSpeed("Hue Shift Speed", Float) = 0.5
        _WaveFrequency("Wave Frequency", Float) = 6.0
        _WaveAmplitude("Wave Amplitude", Float) = 0.07
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float _Distortion;
            float _Speed;
            float _HueShiftSpeed;
            float _WaveFrequency;
            float _WaveAmplitude;

            float3 RGBToHSV(float3 c)
            {
                float4 K = float4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
                float4 p = c.g < c.b ? float4(c.bg, K.wz) : float4(c.gb, K.xy);
                float4 q = c.r < p.x ? float4(p.xyw, c.r) : float4(c.r, p.yzx);
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)),
                              d / (q.x + e),
                              q.x);
            }

            float3 HSVToRGB(float3 c)
            {
                float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float time = _Time.y * _Speed;

                // Swirling wave distortion
                float2 uv = i.uv;
                uv.x += sin(uv.y * _WaveFrequency + time) * _WaveAmplitude;
                uv.y += cos(uv.x * _WaveFrequency + time) * _WaveAmplitude;

                // Extra distortion factor
                uv += sin(uv.xy * (_WaveFrequency * 0.5) + time) * _Distortion * 0.1;

                // Sample main texture
                fixed4 col = tex2D(_MainTex, uv);

                // Hue shift over time
                float3 hsv = RGBToHSV(col.rgb);
                hsv.x = frac(hsv.x + _Time.y * _HueShiftSpeed);

                col.rgb = HSVToRGB(hsv);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
