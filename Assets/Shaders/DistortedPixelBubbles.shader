Shader "Custom/PsychedelicBubble"
{
    Properties
    {
        _MainTex ("Bubble Texture", 2D) = "white" {}
        _WaveSpeed ("Wave Speed", Float) = 2.5
        _WaveAmount ("Wave Amount", Float) = 0.08
        _ShimmerSpeed ("Shimmer Speed", Float) = 8
        _ShimmerIntensity ("Shimmer Intensity", Float) = 0.4
        _Color ("Base Color", Color) = (1, 0.4, 0.7, 1)
        _RainbowIntensity ("Rainbow Intensity", Float) = 1.5
        _RainbowSpeed ("Rainbow Speed", Float) = 3
        _SpiralSpeed ("Spiral Speed", Float) = 1.2
        _SpiralAmount ("Spiral Amount", Float) = 0.15
        _PulseSpeed ("Pulse Speed", Float) = 2
        _PulseAmount ("Pulse Amount", Float) = 0.3
        _NoiseScale ("Noise Scale", Float) = 15
        _ChromaticAberration ("Chromatic Aberration", Float) = 0.02
        _GlitchIntensity ("Glitch Intensity", Float) = 0.1
        _InstanceSeed ("Instance Seed", Float) = 0
        _DistortionVariation ("Distortion Variation", Float) = 1
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
            float _RainbowIntensity;
            float _RainbowSpeed;
            float _SpiralSpeed;
            float _SpiralAmount;
            float _PulseSpeed;
            float _PulseAmount;
            float _NoiseScale;
            float _ChromaticAberration;
            float _GlitchIntensity;
            float _InstanceSeed;
            float _DistortionVariation;
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
                float4 worldPos : TEXCOORD1;
            };
            
            // Enhanced noise functions
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }
            
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i + float2(0,0)), hash(i + float2(1,0)), f.x),
                           lerp(hash(i + float2(0,1)), hash(i + float2(1,1)), f.x), f.y);
            }
            
            float fbm(float2 p)
            {
                float f = 0;
                f += 0.5000 * noise(p); p *= 2.02;
                f += 0.2500 * noise(p); p *= 2.03;
                f += 0.1250 * noise(p); p *= 2.01;
                f += 0.0625 * noise(p);
                return f / 0.9375;
            }
            
            // HSV to RGB conversion for rainbow effects
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y;
                
                // Create unique variations per instance using the seed
                float seedOffset = _InstanceSeed * 12.9898;
                float timeOffset = _InstanceSeed * 7.233;
                float effectiveTime = time + timeOffset;
                
                // Create spiral distortion with instance variation
                float2 center = float2(0.5, 0.5);
                float2 toCenter = uv - center;
                float dist = length(toCenter);
                float angle = atan2(toCenter.y, toCenter.x);
                
                // Spiral effect with unique parameters per instance
                float spiralMultiplier = 10.0 + sin(_InstanceSeed * 3.14159) * _DistortionVariation * 5.0;
                angle += dist * spiralMultiplier + effectiveTime * _SpiralSpeed;
                float spiralWave = sin(angle * (5 + _InstanceSeed * 2) + effectiveTime * 2) * _SpiralAmount * (0.5 - dist);
                
                // Multiple wave layers with instance variations
                float waveFreq1 = 25 + sin(seedOffset) * _DistortionVariation * 10;
                float waveFreq2 = 40 + cos(seedOffset * 1.2) * _DistortionVariation * 15;
                float waveFreq3 = 15 + sin(seedOffset * 1.7) * _DistortionVariation * 8;
                
                float waveX = sin(uv.y * waveFreq1 + effectiveTime * _WaveSpeed) * _WaveAmount;
                waveX += sin(uv.y * waveFreq2 + effectiveTime * _WaveSpeed * 1.7) * _WaveAmount * 0.5;
                waveX += cos(uv.x * waveFreq3 + effectiveTime * _WaveSpeed * 0.8) * _WaveAmount * 0.3;
                
                float waveFreq4 = 30 + sin(seedOffset * 2.1) * _DistortionVariation * 12;
                float waveFreq5 = 50 + cos(seedOffset * 2.8) * _DistortionVariation * 20;
                float waveFreq6 = 20 + sin(seedOffset * 3.3) * _DistortionVariation * 10;
                
                float waveY = cos(uv.x * waveFreq4 + effectiveTime * _WaveSpeed * 1.5) * _WaveAmount;
                waveY += sin(uv.x * waveFreq5 + effectiveTime * _WaveSpeed * 2.1) * _WaveAmount * 0.4;
                waveY += sin(uv.y * waveFreq6 + effectiveTime * _WaveSpeed * 1.2) * _WaveAmount * 0.6;
                
                // Add noise-based distortion with instance variation
                float noiseScale = _NoiseScale + sin(_InstanceSeed * 4.7) * _DistortionVariation * 5;
                float noiseDistort = fbm(uv * noiseScale + effectiveTime * 0.5) * 0.05;
                
                // Glitch effect with instance-specific timing
                float glitchTime = effectiveTime * (10 + _InstanceSeed * 3);
                float glitch = step(0.99, noise(float2(glitchTime, floor(uv.y * 20)))) * _GlitchIntensity;
                waveX += glitch * sin(effectiveTime * 50);
                
                // Apply all distortions
                uv += float2(waveX + spiralWave + noiseDistort, waveY + spiralWave * 0.5 + noiseDistort);
                
                // Pulsing effect with unique phase
                float pulsePhase = _InstanceSeed * 3.14159 * 2;
                float pulse = sin(effectiveTime * _PulseSpeed + pulsePhase) * _PulseAmount + 1.0;
                uv = (uv - center) * pulse + center;
                
                // Chromatic aberration with slight instance variation
                float aberrationAmount = _ChromaticAberration * (1.0 + sin(_InstanceSeed * 5.2) * 0.5);
                float r = tex2D(_MainTex, uv + float2(aberrationAmount, 0)).r;
                float g = tex2D(_MainTex, uv).g;
                float b = tex2D(_MainTex, uv - float2(aberrationAmount, 0)).b;
                fixed4 texColor = fixed4(r, g, b, tex2D(_MainTex, uv).a);
                
                // Rainbow color cycling with instance offset
                float hueOffset = _InstanceSeed * 0.618033; // Golden ratio for nice distribution
                float hue = fmod(dist * 3 + effectiveTime * _RainbowSpeed + angle * 0.3 + hueOffset, 1.0);
                float3 rainbowColor = hsv2rgb(float3(hue, 0.8, 1.0));
                
                // Complex gradient with multiple layers
                float gradient1 = smoothstep(0.6, 0.3, dist);
                float gradient2 = smoothstep(0.4, 0.1, dist);
                float gradient3 = 1.0 - smoothstep(0.0, 0.8, dist);
                
                // Shimmer with instance-specific frequencies
                float shimmerFreq1 = _ShimmerSpeed * (1.0 + sin(_InstanceSeed * 6.1) * 0.5);
                float shimmerFreq2 = _ShimmerSpeed * 2.3 * (1.0 + cos(_InstanceSeed * 7.4) * 0.3);
                float shimmerFreq3 = _ShimmerSpeed * 0.7 * (1.0 + sin(_InstanceSeed * 8.9) * 0.4);
                
                float shimmer1 = (noise(uv * effectiveTime * shimmerFreq1) - 0.5) * _ShimmerIntensity;
                float shimmer2 = (noise(uv * effectiveTime * shimmerFreq2 + 100) - 0.5) * _ShimmerIntensity * 0.5;
                float shimmer3 = (noise(uv * effectiveTime * shimmerFreq3 + 200) - 0.5) * _ShimmerIntensity * 0.3;
                float totalShimmer = shimmer1 + shimmer2 + shimmer3;
                
                // Kaleidoscope effect with instance variation
                float kaleidoscopeFreq = 6 + sin(_InstanceSeed * 9.7) * _DistortionVariation * 3;
                float kaleidoscope = sin(angle * kaleidoscopeFreq + effectiveTime * 2) * 0.5 + 0.5;
                kaleidoscope *= sin(dist * (20 + _InstanceSeed * 10) + effectiveTime * 3) * 0.5 + 0.5;
                
                // Combine all effects
                float alpha = (gradient1 + gradient2 * 0.5 + gradient3 * 0.3) + totalShimmer;
                alpha *= kaleidoscope * 0.5 + 0.5;
                alpha = saturate(alpha);
                
                // Color mixing with instance variation
                float rainbowMix = _RainbowIntensity * gradient2 * (1.0 + sin(_InstanceSeed * 10.3) * 0.3);
                float3 finalColorRGB = lerp(_Color.rgb, rainbowColor, rainbowMix);
                finalColorRGB = lerp(finalColorRGB, rainbowColor * 1.5, kaleidoscope * 0.3);
                
                // Add some color cycling with instance phase
                float colorCyclePhase = _InstanceSeed * 2.718281; // e for distribution
                finalColorRGB *= (sin(effectiveTime * 2 + dist * 10 + colorCyclePhase) * 0.2 + 1.0);
                
                // Psychedelic color shifts with unique phases
                finalColorRGB.r *= sin(effectiveTime * 1.7 + uv.x * 5 + _InstanceSeed) * 0.3 + 1.0;
                finalColorRGB.g *= cos(effectiveTime * 2.1 + uv.y * 7 + _InstanceSeed * 1.3) * 0.3 + 1.0;
                finalColorRGB.b *= sin(effectiveTime * 1.3 + dist * 8 + _InstanceSeed * 1.7) * 0.3 + 1.0;
                
                fixed4 finalColor = fixed4(finalColorRGB * texColor.rgb, alpha * texColor.a * _Color.a);
                
                // Add some glow
                finalColor.rgb += pow(gradient2, 3) * rainbowColor * 0.5;
                
                return finalColor;
            }
            ENDCG
        }
    }
}