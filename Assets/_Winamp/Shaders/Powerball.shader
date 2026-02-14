Shader "Winamp/Visualizer/Powerball"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _AudioData ("Audio FFT Data", 2D) = "black" {}
        _ColorCenter ("Center Glow Color", Color) = (0.2, 0.7, 1.0, 1)
        _ColorOuter ("Outer Tendrils", Color) = (1.0, 0.2, 0.8, 1)
        _Sensitivity ("Overall Sensitivity", Range(1, 20)) = 10.0
        _BeatScale ("Beat Pulse Intensity", Range(0, 2)) = 1.0
        _RotationSpeed ("Rotation Speed", Range(0, 10)) = 2.0
        _Exposure ("Exposure/Glow", Range(1, 5)) = 2.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Blend One One // Additive

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _AudioData;
            float4 _ColorCenter;
            float4 _ColorOuter;
            float _Sensitivity;
            float _BeatScale;
            float _RotationSpeed;
            float _Exposure;
            float _BeatPulse; // Fed from C# peak detection

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            float2 ToPolar(float2 uv)
            {
                float2 dir = uv - 0.5;
                float dist = length(dir) * 2.0;
                float angle = atan2(dir.y, dir.x) / (2.0 * 3.14159) + 0.5;
                return float2(dist, angle);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 polar = ToPolar(i.texcoord);
                float dist = polar.x;
                float angle = polar.y;

                // Combine C# pulse logic with texture data for max impact
                float bass = tex2D(_AudioData, float2(0.02, 0.5)).r * _Sensitivity;
                float pulse = max(bass, _BeatPulse * _BeatScale);
                
                // Core dynamics
                float coreSize = 0.15 + pulse * 0.4;
                float coreGlow = pow(max(0, 1.0 - dist / coreSize), 4.0);
                float4 coreCol = _ColorCenter * coreGlow * (2.0 + pulse * 4.0);

                // Rotating Energy Tendrils
                float timeAngle = angle + _Time.y * _RotationSpeed * 0.1;
                float freqSample = tex2D(_AudioData, float2(frac(timeAngle), 0.5)).r * _Sensitivity;
                
                // Noise/Wobble simulation
                float noise = sin(angle * 20.0 + _Time.z) * 0.02;
                float distAdjusted = dist + noise;
                
                // Tendrils/Lightning logic
                float rayPos = coreSize + freqSample * 0.5;
                float rayGlow = exp(-abs(distAdjusted - rayPos) * 15.0);
                float4 rayCol = _ColorOuter * rayGlow * (1.0 + freqSample * 2.0);

                // Final Glow Mix
                float4 final = (coreCol + rayCol) * _Exposure;
                
                // Clipping at edges
                final *= smoothstep(1.1, 0.8, dist);
                
                return float4(final.rgb, 1.0);
            }
            ENDCG
        }
    }
}
