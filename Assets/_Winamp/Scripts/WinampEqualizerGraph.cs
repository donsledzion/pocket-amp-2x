using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SoftAware
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class WinampEqualizerGraph : MaskableGraphic
    {
        [Header("Assets")]
        [SerializeField] private Sprite colorsSprite;
        [SerializeField] private Sprite preampSprite;

        private float preamp = 0f;
        private float[] bands = new float[10];

        // Winamp EQ graph is exactly 113 pixels wide and 19 pixels high
        private const int GraphWidth = 113;
        private const int GraphHeight = 19;

        // Band positions in the 113px width (approximate Winamp 2.x layout)
        private static readonly int[] BandXPositions = { 2, 14, 26, 38, 50, 62, 74, 86, 98, 110 };

        public override Texture mainTexture => colorsSprite != null ? colorsSprite.texture : base.mainTexture;

        public void SetGains(float preampValue, float[] bandValues)
        {
            preamp = preampValue;
            if (bandValues != null && bandValues.Length == 10)
            {
                System.Array.Copy(bandValues, bands, 10);
            }
            SetAllDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (colorsSprite == null) return;

            Rect rect = GetPixelAdjustedRect();

            // 1. Interpolate gains for all 113 columns
            float[] interpolatedGains = InterpolateBands();

            // 2. Draw 113 vertical segments (1px wide each)
            float colWidth = rect.width / GraphWidth;
            
            for (int x = 0; x < GraphWidth; x++)
            {
                int level = MapGainToLevel(interpolatedGains[x]);
                DrawGraphColumn(vh, x, level, rect, colWidth);
            }

            // 3. Draw Preamp Line
            DrawPreampLine(vh, rect);
        }

        private float[] InterpolateBands()
        {
            float[] result = new float[GraphWidth];
            
            // Linear interpolation between the fixed band positions
            for (int x = 0; x < GraphWidth; x++)
            {
                if (x <= BandXPositions[0])
                {
                    result[x] = bands[0];
                }
                else if (x >= BandXPositions[9])
                {
                    result[x] = bands[9];
                }
                else
                {
                    // Find which two bands we are between
                    int i = 0;
                    while (i < 9 && x > BandXPositions[i + 1]) i++;
                    
                    float t = (float)(x - BandXPositions[i]) / (BandXPositions[i+1] - BandXPositions[i]);
                    result[x] = Mathf.Lerp(bands[i], bands[i+1], t);
                }
            }
            return result;
        }

        private void DrawGraphColumn(VertexHelper vh, int x, int level, Rect rect, float colWidth)
        {
            // level 0 is bottom (-12dB), level 18 is top (+12dB)
            float xPos = rect.xMin + x * colWidth;
            
            float segmentY = rect.yMin + (level / (float)(GraphHeight - 1)) * rect.height;
            float segmentHeight = rect.height / GraphHeight;

            Rect uvRect = colorsSprite.rect;
            Texture tex = colorsSprite.texture;
            
            // Sample color from the 1x19 texture
            float uvX = (uvRect.x + 0.5f) / tex.width;
            float uvY = (uvRect.y + level + 0.5f) / tex.height;

            DrawQuad(vh, 
                new Vector2(xPos, segmentY), 
                new Vector2(colWidth, segmentHeight), 
                new Vector2(uvX, uvY), 
                Color.white);
        }

        private void DrawPreampLine(VertexHelper vh, Rect rect)
        {
            if (preampSprite == null) return;

            // Inverted: +20dB moves line DOWN, -20dB moves line UP.
            float t = 1f - (preamp + 20f) / 40f;
            float y = rect.yMin + Mathf.Clamp01(t) * rect.height;

            Rect uvRect = preampSprite.rect;
            Texture tex = preampSprite.texture;
            
            // Map texture height (pixels) to UI space 1:1 based on graph scaling
            float spriteHeightInUI = uvRect.height * (rect.height / GraphHeight);

            Vector2 uv = new Vector2((uvRect.x + uvRect.width * 0.5f) / tex.width, 
                                     (uvRect.y + uvRect.height * 0.5f) / tex.height);

            DrawQuad(vh, 
                new Vector2(rect.xMin, y - spriteHeightInUI * 0.5f), 
                new Vector2(rect.width, spriteHeightInUI), 
                uv, 
                Color.white);
        }

        private void DrawQuad(VertexHelper vh, Vector2 pos, Vector2 size, Vector2 uv, Color color)
        {
            int baseIndex = vh.currentVertCount;
            UIVertex v = UIVertex.simpleVert;
            v.color = color;
            v.uv0 = uv;

            v.position = new Vector3(pos.x, pos.y);
            vh.AddVert(v);
            v.position = new Vector3(pos.x + size.x, pos.y);
            vh.AddVert(v);
            v.position = new Vector3(pos.x + size.x, pos.y + size.y);
            vh.AddVert(v);
            v.position = new Vector3(pos.x, pos.y + size.y);
            vh.AddVert(v);

            vh.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
            vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
        }

        private int MapGainToLevel(float gain)
        {
            // -20 to +20 dB -> 0 to 18
            float normalized = (gain + 20f) / 40f;
            return Mathf.Clamp(Mathf.RoundToInt(normalized * (GraphHeight - 1)), 0, GraphHeight - 1);
        }
    }
}
