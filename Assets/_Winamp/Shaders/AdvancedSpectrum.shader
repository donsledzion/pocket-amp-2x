Shader "Winamp/Visualizer/AdvancedSpectrum"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _AudioData ("Audio FFT Data", 2D) = "black" {}
        _ColorLow ("Low Frequency Color", Color) = (0, 1, 0, 1)
        _ColorHigh ("High Frequency Color", Color) = (1, 0, 0, 1)
        _Sensitivity ("Sensitivity", Range(0.1, 5.0)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
            float4 _ColorLow;
            float4 _ColorHigh;
            float _Sensitivity;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the FFT data (x is frequency 0-1)
                float fft = tex2D(_AudioData, float2(i.texcoord.x, 0.5)).r;
                
                // Amplify and clamp
                fft *= _Sensitivity;
                
                // Vertical bar logic
                float bar = step(i.texcoord.y, fft);
                
                // Color gradient based on frequency
                float4 col = lerp(_ColorLow, _ColorHigh, i.texcoord.x);
                
                // Add some glow/vibrancy
                col.rgb += fft * 0.5;
                
                return col * bar;
            }
            ENDCG
        }
    }
}
