using UnityEngine;
using System.Collections.Generic;

namespace SoftAware
{
    /// <summary>
    /// Handles parsing of text-based skin definitions (VISCOLOR.TXT, PLEDIT.TXT)
    /// </summary>
    public class SkinParser
    {
        public Color[] ParseVisColor(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            // Remove Byte Order Mark (BOM)
            text = text.Trim('\uFEFF', '\u200B');
            
            var colors = new List<Color>();
            string[] lines = text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || 
                    trimmed.StartsWith("//") || 
                    trimmed.StartsWith(";") || 
                    trimmed.StartsWith("#")) continue;

                // Expected format: R,G,B or R G B
                string[] parts = trimmed.Split(new[] { ',', ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[0], out int r) && 
                        int.TryParse(parts[1], out int g) && 
                        int.TryParse(parts[2], out int b))
                    {
                        colors.Add(new Color(r / 255f, g / 255f, b / 255f, 1f));
                    }
                }
                
                if (colors.Count >= 24) break; 
            }

            return colors.Count > 0 ? colors.ToArray() : null;
        }

        public void ParsePlEditTxt(string text, Skin skin)
        {
            if (string.IsNullOrEmpty(text) || skin == null) return;

            string[] lines = text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.Contains('='))
                {
                    string[] parts = trimmed.Split('=');
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim().ToLower();
                        string val = parts[1].Trim();
                        
                        // Remove comments after value (e.g. #FFFFFF ; white)
                        if (val.Contains(';')) val = val.Split(';')[0].Trim();
                        if (val.Contains("//")) val = val.Split(new[] { "//" }, System.StringSplitOptions.None)[0].Trim();

                        if (!val.StartsWith("#")) val = "#" + val;

                        if (ColorUtility.TryParseHtmlString(val, out Color col))
                        {
                            switch (key)
                            {
                                case "normal": skin.PlNormalColor = col; break;
                                case "current": skin.PlCurrentColor = col; break;
                                case "normalbg": skin.PlNormalBGColor = col; break;
                                case "selectedbg": skin.PlSelectedBGColor = col; break;
                                case "mbfg": skin.PlMbFGColor = col; break;
                                case "mbbg": skin.PlMbBGColor = col; break;
                            }
                        }
                    }
                }
            }
        }
    }
}
