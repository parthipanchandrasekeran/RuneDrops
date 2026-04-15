using UnityEngine;
using UnityEditor;
using System.IO;

namespace RuneDrop.Editor
{
    public static class IconGenerator
    {
        [MenuItem("RuneDrop/Generate App Icon")]
        public static void GenerateIcon()
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / size;
                    float ny = (float)y / size;

                    // Dark purple background with gradient
                    float bgR = Mathf.Lerp(0.05f, 0.15f, ny);
                    float bgG = Mathf.Lerp(0.02f, 0.05f, ny);
                    float bgB = Mathf.Lerp(0.1f, 0.3f, ny);
                    Color bg = new Color(bgR, bgG, bgB);

                    // Diamond shape in center (rune icon)
                    float dx = Mathf.Abs(nx - 0.5f) * 2f;
                    float dy = Mathf.Abs(ny - 0.5f) * 2f;
                    float diamond = dx + dy;

                    Color final_color = bg;

                    // Outer glow (0.5-0.8)
                    if (diamond < 0.8f && diamond > 0.5f)
                    {
                        float glow = 1f - (diamond - 0.5f) / 0.3f;
                        Color glowColor = new Color(0.5f, 0.2f, 0.8f, glow * 0.4f);
                        final_color = Color.Lerp(bg, glowColor, glow * 0.3f);
                    }

                    // Diamond body (< 0.5)
                    if (diamond < 0.5f)
                    {
                        float t = diamond / 0.5f;
                        // Purple to cyan gradient
                        Color inner = Color.Lerp(
                            new Color(0.7f, 0.4f, 1f),
                            new Color(0.3f, 0.8f, 1f),
                            ny
                        );
                        final_color = Color.Lerp(inner, new Color(0.4f, 0.15f, 0.6f), t);
                    }

                    // Inner highlight (< 0.15)
                    if (diamond < 0.15f)
                    {
                        Color bright = new Color(0.9f, 0.8f, 1f);
                        float hl = 1f - diamond / 0.15f;
                        final_color = Color.Lerp(final_color, bright, hl * 0.5f);
                    }

                    pixels[y * size + x] = final_color;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            // Save as PNG
            string dir = "Assets/_Project/Textures/UI";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string path = $"{dir}/AppIcon.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.Refresh();

            // Set as Android icon
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.maxTextureSize = 512;
                importer.SaveAndReimport();
            }

            // Set as default icon
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { tex });

            Debug.Log($"[IconGenerator] Created app icon at {path}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
