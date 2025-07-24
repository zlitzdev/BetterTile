using UnityEngine;
using UnityEngine.Tilemaps;

using UnityTile = UnityEngine.Tilemaps.Tile;

namespace Zlitz.Extra2D.BetterTile
{
    public abstract class BaseTile : TileBase
    {
        protected abstract UnityTile.ColliderType colliderType { get; }

        protected abstract TileOutput GetOutput(ITilemap tilemap, Vector3Int position);

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            TileOutput output = GetOutput(tilemap, position);

            tileData.colliderType = colliderType;
            tileData.sprite = null;

            output.GetTileData(position, ref tileData);
        }

        public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
        {
            TileOutput output = GetOutput(tilemap, position);
            return output.GetTileAnimationData(tilemap.GetRandomValue(position), ref tileAnimationData);
        }
    }
}
