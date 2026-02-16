using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SoftAware.Winamp.Editor
{
    public class DemoSkinManifestGenerator
    {
        [MenuItem("Winamp/Generate Demo Skins Manifest")]
        public static void GenerateManifest()
        {
            string streamingAssetsPath = Application.streamingAssetsPath;
            string demoSkinsPath = Path.Combine(streamingAssetsPath, "demo_skins");

            if (!Directory.Exists(demoSkinsPath))
            {
                Debug.LogWarning($"[DemoSkinManifestGenerator] 'demo_skins' folder not found at {demoSkinsPath}. Creating it.");
                Directory.CreateDirectory(demoSkinsPath);
                AssetDatabase.Refresh();
                return;
            }

            // Get all skin files (.wsz, .zip)
            var skinFiles = Directory.GetFiles(demoSkinsPath, "*.*")
                .Where(f => f.EndsWith(".wsz") || f.EndsWith(".zip"))
                .Select(Path.GetFileName)
                .ToArray();

            string manifestPath = Path.Combine(demoSkinsPath, "manifest.txt");
            File.WriteAllLines(manifestPath, skinFiles);

            Debug.Log($"[DemoSkinManifestGenerator] Manifest generated at {manifestPath} with {skinFiles.Length} skins.");
            AssetDatabase.Refresh();
        }
    }
}
