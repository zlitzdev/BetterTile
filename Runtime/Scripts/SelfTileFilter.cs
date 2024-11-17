using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zlitz.Extra2D.BetterTile
{
    public class SelfTileFilter : ScriptableObject, ITileFilter
    {
        #region Editor-only properties
        #if UNITY_EDITOR

        [SerializeField]
        private Color m_color = Color.white;

#endif
        #endregion

        #region ITileFilter

        bool ITileFilter.Match(Tile sourceTile, TileBase tile)
        {
            return tile == sourceTile;
        }

        bool ITileFilter.IsGeneralizedOf(ITileFilter other)
        {
            return other is SelfTileFilter;
        }

        #endregion
    }
}
