using System.Linq;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zlitz.Extra2D.BetterTile
{
    public class TileDecorator : ScriptableObject, ITileFilter, ITileIdentifier
    {
        #region Editor-only properties
        #if UNITY_EDITOR

        [SerializeField]
        private Color m_color = Color.white;

#endif
        #endregion

        [SerializeField]
        private DecoratorTile[] m_tiles;

        internal TileBase GetTile(Sprite sprite)
        {
            return m_tiles?.FirstOrDefault(t => t.sprite == sprite) ?? null;
        }

        #region ITileFilter

        bool ITileFilter.Match(Tile sourceTile, TileBase tile)
        {
            return tile is null;
        }

        bool ITileFilter.IsGeneralizedOf(ITileFilter other)
        {
            return other is TileDecorator;
        }

        #endregion
    }
}
