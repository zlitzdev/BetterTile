using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using UnityTile = UnityEngine.Tilemaps.Tile;

namespace Zlitz.Extra2D.BetterTile
{
    public sealed class Tile : BaseTile
    {
        [SerializeField]
        private TileSet m_tileSet;

        [SerializeField]
        private UnityTile.ColliderType m_colliderType;

        #if UNITY_EDITOR

        [SerializeField]
        private Sprite m_sprite;

        [SerializeField]
        private Color m_color = Color.white;

        #endif

        internal TileSet tileSet => m_tileSet;

        protected override UnityTile.ColliderType colliderType => m_colliderType;

        protected override TileOutput GetOutput(ITilemap tilemap, Vector3Int position)
        {
            RuleSet ruleSet = m_tileSet.GetTileRuleSet(this);
            if (ruleSet != null)
            {
                TileContext context = new TileContext(tilemap, position);
                if (ruleSet.Sample(context, tilemap.GetRandomValue(position), out TileOutput output))
                {
                    return output;
                }
            }

            return default;
        }

        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector3Int p = position + new Vector3Int(dx, dy);
                    tilemap.RefreshTile(p);
                }
            }

            Tilemap t = tilemap.GetTilemap();

            TilemapDecoratorLayer decoratorLayer = t.GetDecoratorLayer();
            TilemapOverlayLayer   overlayLayer   = t.GetOverlayLayer();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector3Int p = position + new Vector3Int(dx, dy);

                    m_tileSet.ResolveDecorator(decoratorLayer, p);
                    m_tileSet.ResolveOverlay(overlayLayer, p);
                }
            }
        }
    }
}
