using UnityEngine;
using System.IO;

namespace SoftAware
{
    /// <summary>
    /// Simple BMP decoder for Skins
    /// Supports 24-bit and 32-bit uncompressed BMP files
    /// </summary>
    public static class BMPLoader
    {
        public static Texture2D LoadBMP(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            byte[] data = File.ReadAllBytes(filePath);
            return LoadBMP(data);
        }
        
        public static Texture2D LoadBMP(byte[] data)
        {
            // BMP Header validation
            if (data.Length < 54 || data[0] != 'B' || data[1] != 'M')
            {
                Debug.LogError("Invalid BMP file - missing BM signature or too short");
                return null;
            }
            
            // Read header information
            int dataOffset = ReadInt(data, 10);
            int headerSize = ReadInt(data, 14);
            int width = ReadInt(data, 18);
            int height = ReadInt(data, 22);
            int planes = ReadShort(data, 26);
            int bitsPerPixel = ReadShort(data, 28);
            int compression = ReadInt(data, 30);
            int imageSize = ReadInt(data, 34);
            int colorsUsed = ReadInt(data, 46);

            Debug.Log($"[BMPLoader] Loading BMP. HeaderSize: {headerSize}, Size: {width}x{height}, BPP: {bitsPerPixel}, Compression: {compression}, DataOffset: {dataOffset}, ColorsUsed: {colorsUsed}, ImageSize: {imageSize}");

            bool bottomUp = height > 0;
            int absHeight = Mathf.Abs(height);

            // 1 = RLE8, 0 = RGB (Uncompressed)
            if (compression != 0 && compression != 1)
            {
                Debug.LogError($"Compressed BMP not supported (compression type: {compression}). Only RGB (0) and RLE8 (1) are supported.");
                return null;
            }
            
            // Support 8, 24, 32 bits
            if (bitsPerPixel != 8 && bitsPerPixel != 24 && bitsPerPixel != 32)
            {
                Debug.LogError($"Unsupported bit depth: {bitsPerPixel}. Only 8, 24, and 32-bit BMPs are supported.");
                return null;
            }

            // Read Palette for 8-bit
            Color32[] palette = null;
            if (bitsPerPixel == 8)
            {
                int paletteOffset = 14 + headerSize;
                int entriesInFile = (colorsUsed == 0) ? 256 : colorsUsed;
                
                // Palette entries are usually BGRA (4 bytes)
                // Check if we have enough data
                if (data.Length < paletteOffset + entriesInFile * 4)
                {
                    Debug.LogError("BMP file too short for palette.");
                    return null;
                }

                // Always allocate 256 to be safe against bad indices
                palette = new Color32[256];
                
                for (int i = 0; i < entriesInFile; i++)
                {
                    if (i >= 256) break; // Should not happen for 8-bit but safety first
                    
                    int ptr = paletteOffset + i * 4;
                    byte b = data[ptr];
                    byte g = data[ptr + 1];
                    byte r = data[ptr + 2];
                    byte a = 255; // Alpha is reserved/0 usually, force opaque
                    palette[i] = new Color32(r, g, b, a);
                }
                
                // Fill remaining if any (default to black or error color?)
                for (int i = entriesInFile; i < 256; i++)
                {
                    palette[i] = new Color32(0, 0, 0, 255);
                }
            }

            // Create texture
            Texture2D texture = new Texture2D(width, absHeight, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[width * absHeight];
            
            try 
            {
                if (bitsPerPixel == 8 && compression == 1)
                {
                    // RLE8 Decoding
                    DecodeRLE8(data, dataOffset, width, absHeight, palette, pixels, bottomUp);
                }
                else if (bitsPerPixel == 8 && compression == 0)
                {
                    // 8-bit Uncompressed
                    Read8BitRGB(data, dataOffset, width, absHeight, palette, pixels, bottomUp);
                }
                else
                {
                    // 24/32-bit Uncompressed
                    ReadTrueColor(data, dataOffset, width, absHeight, bitsPerPixel, pixels, bottomUp);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error decoding BMP: {ex.Message}");
                return null;
            }
            
            texture.SetPixels32(pixels);
            texture.Apply();
            
            return texture;
        }

        private static void DecodeRLE8(byte[] data, int offset, int width, int height, Color32[] palette, Color32[] pixels, bool bottomUp)
        {
            int x = 0;
            int y = 0;
            int ptr = offset;
            int length = data.Length;
            int pixelsPainted = 0;
            int eolCount = 0;
            
            // Debug.Log($"[BMPLoader] Start RLE8 Decode. Offset: {offset}, Length: {length}");

            while (ptr < length - 1) 
            {
                byte b1 = data[ptr++];
                byte b2 = data[ptr++];

                if (b1 == 0) // Escape
                {
                    if (b2 == 0) // End of Line
                    {
                        x = 0;
                        y++;
                        eolCount++;
                    }
                    else if (b2 == 1) // End of Bitmap
                    {
                        // Debug.Log("[BMPLoader] EOB Reached");
                        break;
                    }
                    else if (b2 == 2) // Delta
                    {
                        if (ptr + 2 > length) break; // Error
                        byte dx = data[ptr++];
                        byte dy = data[ptr++];
                        x += dx;
                        y += dy;
                        // Debug.Log($"[BMPLoader] Delta {dx},{dy} -> Pos: {x},{y}");
                    }
                    else // Absolute mode
                    {
                        int count = b2;
                        for (int i = 0; i < count; i++)
                        {
                            if (ptr >= length) break;
                            byte colorIndex = data[ptr++];
                            SetPixel(pixels, width, height, x, y, palette[colorIndex], bottomUp);
                            x++;
                            pixelsPainted++;
                        }
                        // Padding to word boundary
                        if ((count % 2) != 0) 
                        {
                           if (ptr < length) ptr++; 
                        }
                    }
                }
                else // Encoded run
                {
                    int count = b1;
                    byte colorIndex = b2;
                    for (int i = 0; i < count; i++)
                    {
                        SetPixel(pixels, width, height, x, y, palette[colorIndex], bottomUp);
                        x++;
                        pixelsPainted++;
                    }
                }
                
                if (y >= height) 
                {
                     // Debug.LogWarning($"[BMPLoader] Y overflow during RLE: {y} >= {height}");
                     break;
                }
            }
            Debug.Log($"[BMPLoader] RLE8 Finished. Drawn: {pixelsPainted} pixels. EOLs: {eolCount}. Final Pos: {x},{y}");
        }

        private static void Read8BitRGB(byte[] data, int offset, int width, int height, Color32[] palette, Color32[] pixels, bool bottomUp)
        {
            // Rows are padded to 4 bytes
            int rowSize = ((width * 8 + 31) / 32) * 4;
            
            for (int y = 0; y < height; y++)
            {
                int rowStart = offset + y * rowSize;
                for (int x = 0; x < width; x++)
                {
                    if (rowStart + x >= data.Length) break;
                    byte colorIndex = data[rowStart + x];
                    SetPixel(pixels, width, height, x, y, palette[colorIndex], bottomUp);
                }
            }
        }

        private static void ReadTrueColor(byte[] data, int offset, int width, int height, int bitsPerPixel, Color32[] pixels, bool bottomUp)
        {
            int bytesPerPixel = bitsPerPixel / 8;
            int rowSize = ((width * bitsPerPixel + 31) / 32) * 4;

            for (int y = 0; y < height; y++)
            {
                int rowStart = offset + y * rowSize;
                for (int x = 0; x < width; x++)
                {
                    int pixelOffset = rowStart + x * bytesPerPixel;
                    if (pixelOffset + 2 >= data.Length) break;

                    byte b = data[pixelOffset];
                    byte g = data[pixelOffset + 1];
                    byte r = data[pixelOffset + 2];
                    byte a = (bitsPerPixel == 32 && pixelOffset + 3 < data.Length) ? data[pixelOffset + 3] : (byte)255;
                    
                    SetPixel(pixels, width, height, x, y, new Color32(r, g, b, a), bottomUp);
                }
            }
        }

        private static void SetPixel(Color32[] pixels, int width, int height, int x, int y, Color32 color, bool bottomUp)
        {
            if (x >= width || y >= height || x < 0 || y < 0) return;
            
            // Standard BMP stores bottom-to-top.
            // If bottomUp is true, line 0 is at bottom.
            // Unity texture is also bottom-to-top.
            // So if `y` is the row index from file (0..height-1):
            // In standard BMP, y=0 is bottom row. Unity wants bottom row at index 0. So targetY = y.
            // Wait, previous implementation had this:
            // int targetY = bottomUp ? y : (absHeight - 1 - y);
            // This assumes iterating y from 0 to height.
            // For RLE, y increments from top usually?
            
            // MSDN: "The bitmap is a bottom-up DIB... The first byte of the bitmap data corresponds to the bottom-left pixel."
            // BUT RLE bitmaps are usually Top-Down? No, headers define height sign.
            // "If the height is positive, the bitmap is a bottom-up DIB... If the height is negative, the bitmap is a top-down DIB... Height cannot be negative for BI_RLE8."
            // So RLE8 is always Bottom-Up?
            // "However, if the bitmap is RLE compressed, the height in the header is positive, but the decoding usually starts at the bottom line and works up?"
            // Actually, RLE decoding logic typically fills from scanline 0 (bottom) up to height-1 (top).
            
            // If we assume `y` in DecodeRLE8 starts at 0 and increments:
            // Does `y=0` correspond to bottom line or top line?
            // RLE8 stream decodes line by line. First line decoded is line 0.
            // In Bottom-Up BMP, line 0 is at the bottom.
            // So targetY = y. (Assuming Unity's (0,0) is bottom-left).
            
            int targetY = bottomUp ? y : (height - 1 - y);
            pixels[targetY * width + x] = color;
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
