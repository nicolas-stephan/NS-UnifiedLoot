using System.Collections.Generic;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Tables;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NS.UnifiedLoot.Editor {
    /// <summary>
    /// Prevents building or entering Play Mode if any LootTableAsset has circular dependencies.
    /// </summary>
    [InitializeOnLoad]
    public class CircularDependencyGuard : IPreprocessBuildWithReport {
        public int callbackOrder => 0;

        static CircularDependencyGuard() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        private static void OnPlayModeStateChanged(PlayModeStateChange state) {
            if (
                state != PlayModeStateChange.ExitingEditMode ||
                !CheckForAllCircularDependencies(out var errorMessage, out var problematicAsset)
            )
                return;

            EditorApplication.isPlaying = false;
            EditorUtility.DisplayDialog(
                "Circular Dependency Detected",
                $"Cannot enter Play Mode because a circular dependency was detected in a LootTableAsset.\n\n{errorMessage}\n\nPlease fix it before playing.",
                "OK"
            );

            if (problematicAsset != null)
                Selection.activeObject = problematicAsset;
        }

        public void OnPreprocessBuild(BuildReport report) {
            if (CheckForAllCircularDependencies(out var errorMessage, out _))
                throw new BuildPlayerWindow.BuildMethodException($"[UnifiedLoot] Build aborted: Circular dependency detected.\n{errorMessage}");
        }

        /// <summary>
        /// Scans all LootTableAssetBase assets in the project for circular dependencies.
        /// </summary>
        public static bool CheckForAllCircularDependencies(out string errorMessage, out LootTableAssetBase? problematicAsset) {
            errorMessage = string.Empty;
            problematicAsset = null;

            string[] guids = AssetDatabase.FindAssets("t:LootTableAssetBase");
            var stack = new List<LootTableAssetBase>();
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<LootTableAssetBase>(path);
                if (asset == null)
                    continue;

                stack.Clear();
                if (!asset.HasCircularDependency(stack))
                    continue;

                problematicAsset = asset;
                var chain = string.Join(" -> ", stack.ConvertAll(s => s != null ? s.name : "(null)"));
                errorMessage = $"Cycle found in '{asset.name}': {chain}";
                Debug.LogError($"[UnifiedLoot] {errorMessage}", asset);
                return true;
            }

            return false;
        }
    }
}