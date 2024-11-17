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
            if (topLeft != null && !topLeft.Match(sourceTile, tilemap.GetTile(position + new Vector3Int(-1, 1, 0)))) 
            {
                return false;
            }
            if (top != null && !top.Match(sourceTile, tilemap.GetTile(position + new Vector3Int(0, 1, 0))))
            {
                return false;
            }
            if (topRight != null && !topRight.Match(sourceTile, tilemap.GetTile(position + new Vector3Int(1, 1, 0))))
            {
                return false;
            }
            if (left != null && !left.Match(sourceTile, tilemap.GetTile(position + new Vector3Int(-1, 0, 0))))
            {
                return false;
            }
            if (right != null && !right.Match(sourceTile, tilemap.GetTile(position + new Vector3Int(1, 0, 0))))
            {
                return false;
            }
            if (bottomLeft != null && !bottomLeft.Match(sourceTile, tilemap.GetTile(position + new Vector3Int(-1, -1, 0))))
            {
                return false;
            }
            if (bottom != null && !bottom.Match(sourceTile, tilemap.GetTile(position + new Vector3Int(0, -1, 0))))
            {
                return false;
            }
            if (bottomRight != null && !bottomRight.Match(sourceTile, tilemap.GetTile(position + new Vector3Int(1, -1, 0))))
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
    }
}
