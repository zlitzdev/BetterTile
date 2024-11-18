using System;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zlitz.Extra2D.BetterTile
{
    [Serializable]
    internal struct ConnectionRule
    {
        [SerializeField]
        private UnityEngine.Object m_topLeft;

        public ITileFilter topLeft => m_topLeft as ITileFilter;

        [SerializeField]
        private UnityEngine.Object m_top;

        public ITileFilter top => m_top as ITileFilter;

        [SerializeField]
        private UnityEngine.Object m_topRight;

        public ITileFilter topRight => m_topRight as ITileFilter;

        [SerializeField]
        private UnityEngine.Object m_left;

        public ITileFilter left => m_left as ITileFilter;

        [SerializeField]
        private UnityEngine.Object m_right;

        public ITileFilter right => m_right as ITileFilter;

        [SerializeField]
        private UnityEngine.Object m_bottomLeft;

        public ITileFilter bottomLeft => m_bottomLeft as ITileFilter;

        [SerializeField]
        private UnityEngine.Object m_bottom;

        public ITileFilter bottom => m_bottom as ITileFilter;

        [SerializeField]
        private UnityEngine.Object m_bottomRight;

        public ITileFilter bottomRight => m_bottomRight as ITileFilter;

        [SerializeField]
        private bool m_alwaysUsed;

        public bool alwaysUsed => m_alwaysUsed;

        public bool IsSame(ConnectionRule other)
        {
            return IsGeneralizedOf(other) && other.IsGeneralizedOf(this);
        }

        public bool IsGeneralizedOf(ConnectionRule other)
        {
            if (!IsGeneralizedOf(topLeft, other.topLeft))
            {
                return false;
            }
            if (!IsGeneralizedOf(top, other.top))
            {
                return false;
            }
            if (!IsGeneralizedOf(topRight, other.topRight))
            {
                return false;
            }
            if (!IsGeneralizedOf(left, other.left))
            {
                return false;
            }
            if (!IsGeneralizedOf(right, other.right))
            {
                return false;
            }
            if (!IsGeneralizedOf(bottomLeft, other.bottomLeft))
            {
                return false;
            }
            if (!IsGeneralizedOf(bottom, other.bottom))
            {
                return false;
            }
            if (!IsGeneralizedOf(bottomRight, other.bottomRight))
            {
                return false;
            }

            return true;
        }

        public bool Check(ITilemap tilemap, Vector3Int position, Tile sourceTile)
        {
            if (!MatchTile(topLeft, sourceTile, tilemap.GetTile(position + new Vector3Int(-1, 1, 0)))) 
            {
                return false;
            }
            if (!MatchTile(top, sourceTile, tilemap.GetTile(position + new Vector3Int(0, 1, 0))))
            {
                return false;
            }
            if (!MatchTile(topRight, sourceTile, tilemap.GetTile(position + new Vector3Int(1, 1, 0))))
            {
                return false;
            }
            if (!MatchTile(left, sourceTile, tilemap.GetTile(position + new Vector3Int(-1, 0, 0))))
            {
                return false;
            }
            if (!MatchTile(right, sourceTile, tilemap.GetTile(position + new Vector3Int(1, 0, 0))))
            {
                return false;
            }
            if (!MatchTile(bottomLeft, sourceTile, tilemap.GetTile(position + new Vector3Int(-1, -1, 0))))
            {
                return false;
            }
            if (!MatchTile(bottom, sourceTile, tilemap.GetTile(position + new Vector3Int(0, -1, 0))))
            {
                return false;
            }
            if (!MatchTile(bottomRight, sourceTile, tilemap.GetTile(position + new Vector3Int(1, -1, 0))))
            {
                return false;
            }

            return true;
        }

        private static bool IsGeneralizedOf(ITileFilter lhs, ITileFilter rhs)
        {
            // null      : includes everything
            // category  : includes some tiles
            // tile      : just itself
            // decorator : same as category, but only includes empty 
            // self      : same as category, but only includes source tile

            if (lhs == null)
            {
                return true;
            }

            return lhs.IsGeneralizedOf(rhs);
        }
    
        private static bool MatchTile(ITileFilter filter, Tile sourceTile, TileBase tile)
        {
            return filter?.Match(sourceTile, tile) ?? true;
        }
    }
}
