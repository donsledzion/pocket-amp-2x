using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using SoftAware.Winamp.SystemMenus.Core;

namespace SoftAware.Winamp.SystemMenus.Skins
{
    public class SkinService : IService
    {
        private string SkinsDirectory => Path.Combine(Application.persistentDataPath, "skins");

        public SkinService()
        {
            if (!Directory.Exists(SkinsDirectory))
            {
                Directory.CreateDirectory(SkinsDirectory);
            }
        }

        public Task<List<string>> GetAvailableSkinsAsync()
        {
            if (!Directory.Exists(SkinsDirectory)) return Task.FromResult(new List<string>());

            // Get both .wsz and .zip files
            var files = Directory.GetFiles(SkinsDirectory, "*.*")
                .Where(s => s.ToLower().EndsWith(".wsz") || s.ToLower().EndsWith(".zip"))
                .Select(Path.GetFileName)
                .OrderBy(n => n)
                .ToList();
            
            return Task.FromResult(files);
        }

        public async Task ImportSkinAsync(string sourcePath)
        {
            if (!File.Exists(sourcePath)) throw new FileNotFoundException("Source skin file not found", sourcePath);

            var fileName = Path.GetFileName(sourcePath);
            var destPath = Path.Combine(SkinsDirectory, fileName);

            await Task.Run(() =>
            {
                File.Copy(sourcePath, destPath, true);
            });
        }

        public async Task DeleteSkinAsync(string skinName)
        {
            var path = Path.Combine(SkinsDirectory, skinName);
            if (File.Exists(path))
            {
                await Task.Run(() => File.Delete(path));
            }
        }

        public async Task<bool> LoadSkin(string skinName)
        {
            var path = Path.Combine(SkinsDirectory, skinName);
            if (File.Exists(path) && WinampSkinManager.Instance != null)
            {
                return await WinampSkinManager.Instance.LoadSkin(path);
            }
            return false;
        }
    }
}
