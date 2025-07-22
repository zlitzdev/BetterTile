using UnityEngine;
using UnityEngine.Tilemaps;

using UnityTile = UnityEngine.Tilemaps.Tile;

namespace Zlitz.Extra2D.BetterTile
{
    public sealed class SimpleTile : BaseTile
    {
        [SerializeField]
        private string m_id;
        
        [SerializeField]
        private TileOutput m_output;

        [SerializeField]
        private UnityTile.ColliderType m_colliderType;

        public string id => m_id;

        internal Sprite sprite => m_output.sprite;

        protected override UnityTile.ColliderType colliderType => m_colliderType;

        protected override TileOutput GetOutput(ITilemap tilemap, Vector3Int position)
        {
            return m_output;
        }
    }
}
