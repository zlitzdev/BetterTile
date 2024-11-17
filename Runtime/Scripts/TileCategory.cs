using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zlitz.Extra2D.BetterTile
{
    public sealed class TileCategory : ScriptableObject, ITileFilter
    {
        #region Editor-only properties
        #if UNITY_EDITOR

        [SerializeField, ColorUsage(showAlpha: false)]
        private Color m_color = Color.white;

        #endif
        #endregion

        [SerializeField]
        private Tile[] m_tiles;

        public IEnumerable<Tile> tiles => m_tiles;

        #region ITileFilter

        bool ITileFilter.Match(Tile sourceTile, TileBase tile)
        {
            return m_tiles?.FirstOrDefault(t => t != null && t == tile);
        }

        bool ITileFilter.IsGeneralizedOf(ITileFilter other)
        {
            return other switch
            {
                TileCategory category => category != null && category == this,
                Tile         tile     => tile != null && m_tiles?.First(t => t != null && t == tile),
                _                     => false
            };
        }

        #endregion
    }
}
