using UnityEngine;
using System.IO;

namespace SoftAware
{
    /// <summary>
    /// Simple BMP decoder for Winamp skins
    /// Supports 24-bit and 32-bit uncompressed BMP files
    /// </summary>
    public static class BMPLoader
    {
        public static Texture2D LoadBMP(string filePath)
        {
            byte[] data = File.ReadAllBytes(filePath);
            return LoadBMP(data);
        }
        
        public static Texture2D LoadBMP(byte[] data)
        {
            // BMP Header validation
            if (data[0] != 'B' || data[1] != 'M')
            {
                Debug.LogError("Invalid BMP file - missing BM signature");
                return null;
            }
            
            // Read header information
            int dataOffset = ReadInt(data, 10);
            int headerSize = ReadInt(data, 14);
            int width = ReadInt(data, 18);
            int height = ReadInt(data, 22);
            int bitsPerPixel = ReadShort(data, 28);
            int compression = ReadInt(data, 30);
            
            // Validate format
            if (compression != 0)
            {
                Debug.LogError($"Compressed BMP not supported (compression type: {compression})");
                return null;
            }
            
            if (bitsPerPixel != 24 && bitsPerPixel != 32)
            {
                Debug.LogError($"Only 24-bit and 32-bit BMP supported (got {bitsPerPixel}-bit)");
                return null;
            }
            
            // Create texture
            Texture2D texture = new Texture2D(width, Mathf.Abs(height), TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[width * Mathf.Abs(height)];
            
            // Calculate row size (must be multiple of 4 bytes)
            int bytesPerPixel = bitsPerPixel / 8;
            int rowSize = ((width * bitsPerPixel + 31) / 32) * 4;
            
                // BMP stores pixels bottom-to-top by default (unless height is negative)
                bool bottomUp = height > 0;
                int absHeight = Mathf.Abs(height);
                
                // Read pixel data
                for (int y = 0; y < absHeight; y++)
                {
                    int rowStart = dataOffset + y * rowSize;
                    
                    for (int x = 0; x < width; x++)
                    {
                        int pixelOffset = rowStart + x * bytesPerPixel;
                        
                        // BMP stores pixels as BGR or BGRA
                        byte b = data[pixelOffset];
                        byte g = data[pixelOffset + 1];
                        byte r = data[pixelOffset + 2];
                        byte a = (bitsPerPixel == 32) ? data[pixelOffset + 3] : (byte)255;
                        
                        // Unity Texture2D expects pixels from bottom-left to top-right
                        // Standard BMP (bottomUp) stores lines from bottom to top.
                        // So we can copy lines directly without flipping Y.
                        // If BMP is top-down (!bottomUp), we need to flip Y.
                        
                        int targetY = bottomUp ? y : (absHeight - 1 - y);
                        int pixelIndex = targetY * width + x;
                        
                        // Use Color32 for performance and direct mapping
                        pixels[pixelIndex] = new Color32(r, g, b, a);
                    }
                }
                
                texture.SetPixels32(pixels);
            texture.Apply();
            
            // Set pixel-perfect settings
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            
            return texture;
        }
        
        private static int ReadInt(byte[] data, int offset)
        {
            return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);
        }
        
        private static int ReadShort(byte[] data, int offset)
        {
            return data[offset] | (data[offset + 1] << 8);
        }
    }
}
