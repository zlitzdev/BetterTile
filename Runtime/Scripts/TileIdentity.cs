using System;

using UnityEngine;

namespace Zlitz.Extra2D.BetterTile
{
    [Serializable]
    internal struct TileIdentity
    {
        [SerializeField]
        private TileIdentityType m_type;

        [SerializeField]
        private Tile m_tile;

        [SerializeField]
        private TileFilterType m_filterType;

        public bool IsTile(out Tile tile)
        {
            tile = null;
            if (m_type == TileIdentityType.Tile)
            {
                tile = m_tile;
                return true;
            }

            return false;
        }

        public bool IsDecorator()
        {
            return m_type == TileIdentityType.Other && m_filterType == TileFilterType.Decorator;
        }
    }
}
