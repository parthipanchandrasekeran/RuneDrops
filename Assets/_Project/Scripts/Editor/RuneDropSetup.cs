using UnityEngine;
using UnityEditor;
using RuneDrop.Data;

namespace RuneDrop.Editor
{
    public static class RuneDropSetup
    {
        [MenuItem("RuneDrop/Create Game Config")]
        public static void CreateGameConfig()
        {
            var config = ScriptableObject.CreateInstance<GameConfigSO>();

            // Put in Resources so scripts can auto-load via Resources.Load
            string path = "Assets/_Project/Resources/Configs/GameConfig.asset";

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources"))
                AssetDatabase.CreateFolder("Assets/_Project", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources/Configs"))
                AssetDatabase.CreateFolder("Assets/_Project/Resources", "Configs");

            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;

            Debug.Log($"[RuneDropSetup] Created GameConfig at {path}");
        }
    }
}
