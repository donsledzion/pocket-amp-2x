using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SoftAware.PocketAmp.Editor
{
    public class DemoSkinManifestGenerator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
             Debug.Log("[PocketAmp] Pre-processing build: Generating demo skins manifest...");
             GenerateManifest();
        }

        [MenuItem("PocketAmp/Generate Demo Skins Manifest")]
        public static void GenerateManifest()
        {
            try
            {
                string streamingAssetsPath = Application.streamingAssetsPath;
                string demoSkinsPath = Path.Combine(streamingAssetsPath, "demo_skins");

                if (!Directory.Exists(demoSkinsPath))
                {
                    Debug.LogWarning($"[DemoSkinManifestGenerator] 'demo_skins' folder not found at {demoSkinsPath}. Creating it.");
                    Directory.CreateDirectory(demoSkinsPath);
                    // AssetDatabase.Refresh(); // Don't refresh inside a build callback if not needed
                }

                // Get all skin files (.wsz, .zip)
                var skinFiles = Directory.GetFiles(demoSkinsPath, "*.*")
                    .Where(f => f.EndsWith(".wsz") || f.EndsWith(".zip"))
                    .Select(Path.GetFileName)
                    .ToArray();

                string manifestPath = Path.Combine(demoSkinsPath, "manifest.txt");
                File.WriteAllLines(manifestPath, skinFiles);

                Debug.Log($"[DemoSkinManifestGenerator] Manifest generated at {manifestPath} with {skinFiles.Length} skins.");
                
                // Only refresh if explicitly called via Menu Item or if outside build pipeline context (checking unlikely scenario for safety)
                if (!BuildPipeline.isBuildingPlayer) 
                {
                    AssetDatabase.Refresh();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DemoSkinManifestGenerator] Error generating manifest: {ex.Message}");
                if (BuildPipeline.isBuildingPlayer) throw; // Fail build on error
            }
        }
    }
}
