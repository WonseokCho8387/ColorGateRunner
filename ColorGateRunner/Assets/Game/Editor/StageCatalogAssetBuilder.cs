using ColorGateRunner.Presentation;
using UnityEditor;
using UnityEngine;

namespace ColorGateRunner.Editor
{
    public static class StageCatalogAssetBuilder
    {
        public const string AssetFolder = "Assets/Game/Resources";
        public const string AssetPath = AssetFolder + "/StageCatalog.asset";

        [MenuItem("Tools/Color Gate Runner/Create Stage Catalog Asset")]
        public static StageCatalogAsset EnsureAndConfigure()
        {
            EnsureFolder("Assets/Game", "Resources");

            StageCatalogAsset asset =
                AssetDatabase.LoadAssetAtPath<StageCatalogAsset>(AssetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<StageCatalogAsset>();
                asset.InitializeLegacyStages();
                AssetDatabase.CreateAsset(asset, AssetPath);
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
            else if (asset.Count != 23 || asset.RequiresUpgrade)
            {
                asset.InitializeLegacyStages();
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }

            StageCatalogProvider.Configure(asset);
            return asset;
        }

        public static void CreateStageCatalogAssetFromCommandLine()
        {
            EnsureAndConfigure();
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
