using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zlitz.Extra2D.BetterTile
{
    public class DecoratorTile : TileBase
    {
        [SerializeField]
        private Sprite m_sprite;

        public Sprite sprite => m_sprite;

        public static DecoratorTile Create(Sprite sprite)
        {
            DecoratorTile tile = CreateInstance<DecoratorTile>();

            tile.m_sprite = sprite;

            return tile;
        }

        #region TileBase

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite       = m_sprite;
            tileData.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.None;
        }

        #endregion
    }
}
