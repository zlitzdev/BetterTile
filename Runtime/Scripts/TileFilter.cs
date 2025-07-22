using System;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zlitz.Extra2D.BetterTile
{
    [Serializable]
    internal struct TileFilter
    {
        [SerializeField]
        private TileFilterType m_type;

        [SerializeField]
        private bool m_inverted;

        [SerializeField]
        private Tile m_tile;

        [SerializeField]
        private TileCategory m_category;

        public bool MatchTile(TileBase otherTile)
        {
            switch (m_type)
            {
                case TileFilterType.Any:
                    {
                        return MatchedToResult(true);
                    }
                case TileFilterType.Tile:
                    {
                        return MatchedToResult(otherTile == m_tile);
                    }
                case TileFilterType.TileCategory:
                    {
                        return MatchedToResult(m_category?.Contains(otherTile) ?? false);
                    }
                case TileFilterType.Decorator:
                    {
                        return MatchedToResult(otherTile == null);
                    }
            }

            throw new NotImplementedException($"Unknown Tile Filter Type {m_type}");
        }

        private bool MatchedToResult(bool matched)
        {
            return m_inverted != matched;
        }

        #region Compare

        private static readonly int s_generalizationLevel_Global = 0;
        private static readonly int s_generalizationLevel_Group  = 1;
        private static readonly int s_generalizationLevel_Single = 2;

        internal int generalizationLevel
        {
            get
            {
                switch (m_type)
                {
                    case TileFilterType.Any:
                        {
                            return s_generalizationLevel_Global;
                        }
                    case TileFilterType.TileCategory:
                        {
                            return s_generalizationLevel_Group;
                        }
                    case TileFilterType.Tile:
                    case TileFilterType.Decorator:
                        {
                            return m_inverted ? s_generalizationLevel_Group : s_generalizationLevel_Single;
                        }
                }

                throw new NotImplementedException($"Unknown Tile Filter Type {m_type}");
            }
        }

        internal bool IsSame(TileFilter other)
        {
            switch (m_type)
            {
                case TileFilterType.Any:
                    {
                        return other.m_type == TileFilterType.Any;
                    }
                case TileFilterType.Tile:
                    {
                        return other.m_type == TileFilterType.Tile && other.m_tile == m_tile && other.m_inverted == m_inverted;
                    }
                case TileFilterType.TileCategory:
                    {
                        return other.m_type == TileFilterType.TileCategory && other.m_category == m_category && other.m_inverted == m_inverted;
                    }
                default:
                    {
                        return other.m_type == m_type && other.m_inverted == m_inverted;
                    }
            }

            throw new NotImplementedException($"Unknown Tile Filter Type {m_type}");
        }

        internal bool Contains(TileFilter other)
        {
            switch (m_type)
            {
                case TileFilterType.Any:
                    {
                        return true;
                    }
                case TileFilterType.Tile:
                    {
                        if (m_inverted)
                        {
                            return other.m_type == TileFilterType.Tile && ((other.m_tile != m_tile) != other.m_inverted);
                        }
                        else
                        {
                            return other.m_type == TileFilterType.Tile && other.m_tile == m_tile && !other.m_inverted;
                        }
                    }
                case TileFilterType.TileCategory:
                    {
                        return other.m_type == TileFilterType.Tile && (m_inverted != m_category.Contains(other.m_tile)) && !other.m_inverted;
                    }
                case TileFilterType.Decorator:
                    {
                        return other.m_type == TileFilterType.Decorator && m_inverted == other.m_inverted;
                    }
            }

            throw new NotImplementedException($"Unknown Tile Filter Type {m_type}");
        }

        #endregion
    }
}
