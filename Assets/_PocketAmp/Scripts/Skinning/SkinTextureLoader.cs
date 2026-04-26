using UnityEngine;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

namespace SoftAware
{
    /// <summary>
    /// Handles loading textures from disk and processing them (e.g. transparency).
    /// </summary>
    public class SkinTextureLoader
    {
        public async Task<Texture2D> LoadTextureAsync(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;

            // Run on background thread if possible, but Texture creation must be on main thread.
            // File reading can be async.
            byte[] fileData = await File.ReadAllBytesAsync(path);
            
            string extension = Path.GetExtension(path).ToLower();
            Texture2D tex = null;

            if (extension == ".bmp")
            {
                // Assuming BMPLoader is a static utility available in the project
                // Note: BMPLoader might be synchronous. 
                tex = BMPLoader.LoadBMP(path);
            }
            else
            {
                tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!ImageConversion.LoadImage(tex, fileData))
                {
                    Object.Destroy(tex);
                    return null;
                }
            }

            if (tex != null)
            {
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Clamp;
                RemoveMagenta(tex);
            }

            return tex;
        }

        /// <summary>
        /// Removes magenta color (#FF00FF) by making it transparent.
        /// </summary>
        private void RemoveMagenta(Texture2D tex)
        {
            if (tex == null) return;
            
            Color[] pixels = tex.GetPixels();
            bool modified = false;
            const float tolerance = 0.05f;

            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];
                if (Mathf.Abs(pixel.r - 1f) < tolerance && 
                    pixel.g < tolerance && 
                    Mathf.Abs(pixel.b - 1f) < tolerance)
                {
                    pixels[i] = new Color(pixel.r, pixel.g, pixel.b, 0f); // Transparent
                    modified = true;
                }
            }

            if (modified)
            {
                tex.SetPixels(pixels);
                tex.Apply();
            }
        }
    }
}
