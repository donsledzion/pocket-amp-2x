Shader "Winamp/Visualizer/Heartball"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _AudioData ("Audio FFT Data", 2D) = "black" {}
        _ColorCenter ("Center Color", Color) = (1.0, 0.1, 0.3, 1) // Deep Red/Pink
        _ColorOuter ("Outer Color", Color) = (1.0, 0.4, 0.6, 1)  // Hot Pink
        _Sensitivity ("Sensitivity", Range(1, 50)) = 25.0
        _Exposure ("Glow Intensity", Range(1, 15)) = 1.0
        _GlobalScale ("Global Scale", Range(0.1, 2.0)) = 0.65
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
            float _Sensitivity, _Exposure, _Aspect, _BeatPulse, _GlobalScale;

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenUV = v.texcoord;
                return o;
            }

            // IQ's Heart SDF - Slightly modified for easier use here
            float sdHeart(float2 p)
            {
                p.x = abs(p.x);
                if( p.y+p.x>1.0 )
                    return sqrt(dot(p-float2(0.25,0.75),p-float2(0.25,0.75))) - sqrt(2.0)/4.0;
                return sqrt(min(dot(p-float2(0.00,1.00),p-float2(0.00,1.00)),
                                dot(p-float2(0.50,0.50),p-float2(0.50,0.50)))) * sign(p.x-p.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Correct Aspect Ratio & Transform
                float2 uv = (i.screenUV - 0.5);
                uv.x *= _Aspect;
                
                // 2. Stable Heart Deformation (More robust than quadrant-based SDF)
                float2 p = (uv * 2.8) / _GlobalScale;
                p.y += 0.15; // Centering shift
                
                // Deform the coordinate space into a heart shape
                // The "sqrt(abs(p.x))" creates the classic heart top and bottom tip
                float heartWarp = sqrt(abs(p.x)) * 0.5;
                float2 pHeart = p;
                pHeart.y -= heartWarp; 
                
                float dist = length(pHeart) - 0.7; // Base distance to deformed circle
                
                // 3. Dynamic Angle for FFT (Centered on the heart body)
                float angle = atan2(pHeart.y, pHeart.x) / (2.0 * 3.14159) + 0.5;

                // 4. QUAD MIRRORING (Left/Right Symmetry)
                float quadAngle = abs(frac(angle) - 0.5) * 2.0;
                
                // 5. SEAMLESS FFT SAMPLING
                // Ensure we skip the very first (muted) bins and spread across the texture
                float basePos = 0.02 + quadAngle * 0.8; 
                float s0 = tex2D(_AudioData, float2(basePos, 0.5)).r;
                float s1 = tex2D(_AudioData, float2(max(0, basePos - 0.02), 0.5)).r;
                float s2 = tex2D(_AudioData, float2(min(1, basePos + 0.02), 0.5)).r;
                float fftSample = ((s0 * 2.0) + s1 + s2) * 0.25 * _Sensitivity;
                
                // Noise gate
                fftSample = max(0, fftSample - 0.005); 
                
                float pulse = _BeatPulse * _Sensitivity * 0.25;
                float coreRadius = -0.1 + pulse * 0.15;

                // 6. LAYER 1: THE HEART CORE
                // FIX: Use exp() which is naturally bounded and has a better falloff for glow
                // dist <= 0 is deep inside. pulse moves the radius.
                float coreGlow = exp(-abs(dist - coreRadius) * 8.0);
                
                // Keep peak color in check. Additive blend (One One) means 
                // we should aim for ~0.8 max to avoid immediate white-out.
                float coreBrightness = 0.5 + pulse * 2.0;
                float4 coreCol = _ColorCenter * coreGlow * coreBrightness;

                // 7. LAYER 2: HARMONIC WAVES
                float waveFreq = 35.0;
                float wave = sin(angle * waveFreq + _Time.z * 4.0) * 0.5 + 0.5;
                
                float energy = sqrt(fftSample) * 0.6; 
                float rayPos = coreRadius + (wave * energy * 0.5);
                
                // Rays following heart contour
                float rayGlow = exp(-abs(dist - rayPos) * 20.0); 
                float rayGlow2 = exp(-abs(dist - (rayPos*0.85)) * 12.0) * 0.3;

                float4 rayCol = _ColorOuter * (rayGlow + rayGlow2) * (0.6 + energy * 2.5);

                // 8. LAYER 3: OUTER DIFFUSION & MASK
                float diffusion = smoothstep(1.5, -1.0, dist);
                
                // Final Mix
                float4 final = (coreCol + rayCol) * diffusion * _Exposure;
                
                // SOFT CLAMP / TONEMAP: Prevent white supernova by saturating colors
                // This keeps the ratios of RGB intact instead of just clipping them to (1,1,1)
                final.rgb = 1.0 - exp(-final.rgb * 1.5);
                
                // Extra safety: Final color should not exceed the target palette's max brightness
                final.rgb = min(final.rgb, 1.2 * max(_ColorCenter.rgb, _ColorOuter.rgb));

                // Soft Vignette Mask
                final *= smoothstep(1.4, 0.4, length(uv * 1.8));

                return float4(final.rgb, 1.0);
            }
            ENDCG
        }
    }
}
