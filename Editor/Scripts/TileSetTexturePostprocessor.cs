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
            onTextureReimported?.Invoke();
        }
    }
}
