Shader "Winamp/Visualizer/Powerball"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _AudioData ("Audio FFT Data", 2D) = "black" {}
        _ColorCenter ("Center Color", Color) = (0.3, 0.6, 1.0, 1)
        _ColorOuter ("Outer Color", Color) = (1.0, 0.2, 0.6, 1)
        _Sensitivity ("Sensitivity", Range(1, 30)) = 15.0
        _Exposure ("Glow Intensity", Range(1, 15)) = 6.0
        _Aspect ("Aspect Ratio (W/H)", Float) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100
        ZWrite Off
        Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 screenUV : TEXCOORD1;
            };

            sampler2D _AudioData;
            float4 _ColorCenter, _ColorOuter;
            float _Sensitivity, _Exposure, _Aspect, _BeatPulse;

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenUV = v.texcoord;
                return o;
            }

            // Pseudo-noise for organic feel
            float noise(float2 p) {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Correct Aspect Ratio & Polar Space
                float2 uv = i.screenUV - 0.5;
                uv.x *= _Aspect;
                float dist = length(uv) * 2.0;
                float angle = atan2(uv.y, uv.x) / (2.0 * 3.14159) + 0.5;

                // 2. QUAD MIRRORING (Perfect Symmetry, No Seams)
                // Maps 0-1 to 0-1-0-1 in 4 quadrants. Seamless on all axes.
                float quadAngle = abs(frac(angle * 2.0) - 0.5) * 2.0;
                
                // 3. FFT Sampling (Smooth & Shared)
                float fftSample = tex2D(_AudioData, float2(pow(quadAngle, 1.2) * 0.45, 0.5)).r * _Sensitivity;
                float pulse = _BeatPulse * _Sensitivity * 0.4;
                float coreRadius = 0.2 + pulse * 0.25;

                // 4. LAYER 1: THE CORE (Volumetric Glow)
                float coreGlow = pow(max(0, 1.0 - dist / coreRadius), 10.0);
                float4 coreCol = _ColorCenter * coreGlow * (3.0 + pulse * 12.0);

                // 5. LAYER 2: DEFINED WAVES (Distinct energy tendrils)
                // Use sine-waves modulated by FFT to create "spikes"
                float waveFreq = 40.0;
                float wave = sin(angle * waveFreq + _Time.z * 5.0) * 0.5 + 0.5;
                wave *= fftSample; // Modulate by music
                
                float rayPos = coreRadius + wave * 0.6;
                // Sharper energy falloff for "clarity"
                float rayGlow = exp(-abs(dist - rayPos) * 25.0); 
                
                // Add secondary wave layer for "living" feel
                float wave2 = cos(angle * 25.0 - _Time.y * 3.0) * 0.5 + 0.5;
                float rayPos2 = coreRadius + wave2 * fftSample * 0.4;
                float rayGlow2 = exp(-abs(dist - rayPos2) * 35.0);

                float4 rayCol = _ColorOuter * (rayGlow + rayGlow2 * 0.5) * (2.0 + fftSample * 8.0);

                // 6. LAYER 3: OUTER DIFFUSION
                float diffusion = smoothstep(1.3, 0.3, dist) * 0.5;
                
                // Final Mix
                float4 final = (coreCol + rayCol) * diffusion * _Exposure;
                
                // Hard mask at edges
                final *= step(dist, 1.2);

                return float4(final.rgb, 1.0);
            }
            ENDCG
        }
    }
}
