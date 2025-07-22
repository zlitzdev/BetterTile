using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Zlitz.Extra2D.BetterTile
{
    internal class AssetEvent : AssetPostprocessor
    {
        private static HashSet<string> m_trackedTextures = new HashSet<string>();

        public static event Action onAssetDeleted;

        public static event Action<string> onTextureImported;
        public static event Action<string> onTileSetImported;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string imported in importedAssets)
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imported);
                if (texture != null)
                {
                    onTextureImported?.Invoke(imported);
                }

                TileSet tileSet = AssetDatabase.LoadAssetAtPath<TileSet>(imported);
                if (tileSet != null)
                {
                    onTileSetImported?.Invoke(imported);
                }
            }

            if ((deletedAssets?.Length ?? 0) > 0)
            {
                onAssetDeleted?.Invoke();
            }
        }
    }
}
