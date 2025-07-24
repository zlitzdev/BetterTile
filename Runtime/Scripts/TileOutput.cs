using System;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zlitz.Extra2D.BetterTile
{
    [Serializable]
    public struct TileOutput
    {
        [SerializeField]
        private Sprite m_sprite;

        [SerializeField]
        private Vector2 m_speed;

        [SerializeField]
        private Vector2 m_startTime;

        [SerializeField]
        private Sprite[] m_frames;

        private TileBase m_assignedTile;

        public TileBase assignedTile
        {
            get => m_assignedTile;
            set => m_assignedTile = value;
        }

        public Sprite sprite => m_sprite;

        public void GetTileData(Vector3Int position, ref TileData tileData)
        {
            tileData.sprite = m_sprite;
        }

        public bool GetTileAnimationData(float randomValue, ref TileAnimationData animationData)
        {
            if (m_frames != null && m_frames.Length > 0)
            {
                animationData.animatedSprites = m_frames;

                animationData.animationSpeed     = Mathf.Lerp(HashFloat(ref randomValue), m_speed.x, m_speed.y);
                animationData.animationStartTime = Mathf.Lerp(HashFloat(ref randomValue), m_startTime.x, m_startTime.y);

                return true;
            }

            return false;
        }

        private float HashFloat(ref float current)
        {
            unchecked
            {
                int hash = (int)(current * 1000000);
                hash ^= (hash << 13);
                hash ^= (hash >> 17);
                hash ^= (hash << 5);

                uint result = (uint)hash;
                current = (result & 0xFFFFFF) / (float)0x1000000;
            }

            return current;
        }
    }
}
