using System;

using UnityEngine;
using UnityEditor;

namespace Zlitz.Extra2D.BetterTile
{
    public class TileSetTexturePostprocessor : AssetPostprocessor
    {
        public static event Action onTextureReimported;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string imported in importedAssets)
            {
                Type type = AssetDatabase.GetMainAssetTypeAtPath(imported);
                if (typeof(Texture2D).IsAssignableFrom(type))
                {
                    onTextureReimported?.Invoke();
                    return;
                }
            }

        }
    }
}
