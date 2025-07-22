using System.Linq;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zlitz.Extra2D.BetterTile
{
    public sealed class TileCategory : ScriptableObject
    {
        [SerializeField]
        private TileSet m_tileSet;

        #if UNITY_EDITOR

        [SerializeField]
        private Color m_color;

        #endif

        [SerializeField]
        private Tile[] m_tiles;

        public bool Contains(TileBase tile)
        {
            if (tile == null || m_tiles == null || tile is not Tile betterTile)
            {
                return false;
            }

            return m_tiles.Contains(betterTile);
        }
    }
}
