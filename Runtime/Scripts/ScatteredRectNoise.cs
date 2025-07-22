using UnityEngine;

using Rng = System.Random;

namespace Zlitz.Extra2D.BetterTile
{
    internal class ScatteredRectNoise
    {
        private int m_seed;

        private Vector2Int m_cellSize;
        private Vector2Int m_rectSize;
        private Vector2Int m_spacing;

        private float m_density;

        public Vector2Int cellSize
        {
            get => m_cellSize; 
            set => m_cellSize = value;
        }

        public Vector2Int rectSize
        {
            get => m_rectSize;
            set => m_rectSize = value;
        }

        public Vector2Int spacing
        {
            get => m_spacing;
            set => m_spacing = value;
        }

        public float density
        {
            get => m_density;
            set => m_density = value;
        }

        public SampleResult Sample(Vector3Int position)
        {
            (Vector3Int cell, Vector2Int offset) = ConvertToCellCoord(position);

            if (!HasRectInCell(cell, out Vector2Int rectPosition))
            {
                return SampleResult.NotInRect();
            }

            Vector2Int rectOffset = new Vector2Int(
                position.x - rectPosition.x,    
                position.y - rectPosition.y    
            );
                    
            if (rectOffset.x < 0 || rectOffset.y < 0 || rectOffset.x >= m_rectSize.x || rectOffset.y >= m_rectSize.y)
            {
                return SampleResult.NotInRect();
            }

            return SampleResult.InRect(rectPosition, rectOffset);
        }

        public ScatteredRectNoise(Vector2Int cellSize, Vector2Int rectSize, Vector2Int spacing, float density, int? seed = null)
        {
            m_seed = seed.HasValue ? seed.Value : new Rng().Next();

            m_rectSize = new Vector2Int(
                Mathf.Max(1, rectSize.x),    
                Mathf.Max(1, rectSize.y)    
            );

            m_cellSize = new Vector2Int(
                Mathf.Max(m_rectSize.x, cellSize.x),
                Mathf.Max(m_rectSize.y, cellSize.y)
            );

            m_spacing = new Vector2Int(
                Mathf.Max(0, spacing.x),
                Mathf.Max(0, spacing.y)
            );

            m_density = Mathf.Clamp01(density);
        }

        private (Vector3Int, Vector2Int) ConvertToCellCoord(Vector3Int position)
        {
            Vector3Int cell = new Vector3Int(
                Mathf.FloorToInt((float)position.x / (m_cellSize.x + m_spacing.x)),
                Mathf.FloorToInt((float)position.y / (m_cellSize.y + m_spacing.y)),
                position.z
            );

            Vector2Int offset = new Vector2Int(
                position.x - cell.x * (m_cellSize.x + m_spacing.x),    
                position.y - cell.y * (m_cellSize.y + m_spacing.y)    
            );

            return (cell, offset);
        }

        private bool HasRectInCell(Vector3Int cell, out Vector2Int rectPosition)
        {
            rectPosition = default;

            int hash = Hash(cell.x, cell.y, cell.z, m_seed);
            float u = (hash & 0xFFFF) / (float)0xFFFF;
            bool result = u < m_density;

            if (result)
            {
                float u1 = Mathf.Clamp01(((hash >> 16) & 0xFFFF) / (float)0xFFFF);
                float u2 = Mathf.Clamp01(((hash >> 32) & 0xFFFF) / (float)0xFFFF);

                float offsetX = Mathf.Lerp(0, m_cellSize.x + m_spacing.x - m_rectSize.x, u1);
                float offsetY = Mathf.Lerp(0, m_cellSize.y + m_spacing.y - m_rectSize.y, u2);

                rectPosition = new Vector2Int(
                    cell.x * (m_cellSize.x + m_spacing.x) + Mathf.RoundToInt(offsetX),
                    cell.y * (m_cellSize.y + m_spacing.y) + Mathf.RoundToInt(offsetY)
                );
            }

            return result;
        }

        private static int Hash(int x, int y, int z, int seed)
        {
            unchecked
            {
                int h = seed;
                h = h * 21 + x;
                h = h * 21 + y;
                h = h * 21 + z;
                h ^= (h >> 13);
                h *= 0x5bd1e995;
                h ^= (h >> 15);
                return h;
            }
        }

        public struct SampleResult
        {
            public bool inRect { get; private set; }

            public Vector2Int rectPosition { get; private set; }

            public Vector2Int offset { get; private set; }

            public static SampleResult NotInRect()
            {
                return new SampleResult()
                {
                    inRect = false
                };
            }
        
            public static SampleResult InRect(Vector2Int rectPosition, Vector2Int offset)
            {
                return new SampleResult()
                {
                    inRect = true,
                    rectPosition = rectPosition,
                    offset = offset
                };
            }
        }
    }
}
