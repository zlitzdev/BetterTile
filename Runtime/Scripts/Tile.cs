using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

using ColliderType = UnityEngine.Tilemaps.Tile.ColliderType;
using System.Data.SqlTypes;

namespace Zlitz.Extra2D.BetterTile
{
    public sealed class Tile : TileBase, ITileFilter, ITileIdentifier
    {
        #region Editor-only properties
        #if UNITY_EDITOR

        [SerializeField, ColorUsage(showAlpha: false)]
        private Color m_color = Color.white;

#endif
        #endregion

        [SerializeField]
        private TileSet m_tileSet;

        [SerializeField]
        private ColliderType m_colliderType;

        [SerializeField]
        private Tile m_baseTile;

        [SerializeField]
        private bool m_overwriteRules;

        internal TileSet tileSet => m_tileSet;

        internal Tile baseTile => m_baseTile;

        internal bool overwriteRules => m_overwriteRules;

        public bool IsDescendantOf(Tile other)
        {
            Tile parentTile = this;
            while (parentTile != null)
            {
                if (parentTile == other)
                {
                    return true;
                }
                parentTile = parentTile.baseTile;
            }

            return false;
        }

        #region ITileFilter

        bool ITileFilter.Match(Tile sourceTile, TileBase tile)
        {
            return tile == this;
        }

        bool ITileFilter.IsGeneralizedOf(ITileFilter other)
        {
            return other switch
            {
                Tile tile => tile != null && tile == this,
                _         => false
            };
        }

        #endregion

        #region TileBase

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
            Tilemap unityTileMap = GetTileMap(tilemap);
            if (unityTileMap != null)
            {
                if (!unityTileMap.TryGetComponent(out TilemapDecorator decorator))
                {
                    decorator = unityTileMap.gameObject.AddComponent<TilemapDecorator>();
                }

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        decorator.Resolve(position + new Vector3Int(dx, dy, 0));
                    }
                }
            }

            return base.StartUp(position, tilemap, go);
        }

        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    tilemap.RefreshTile(position + new Vector3Int(dx, dy, 0));
                }
            }

            Tilemap unityTileMap = GetTileMap(tilemap);
            TilemapDecorator decorator = null;
            if (unityTileMap != null)
            {
                if (!unityTileMap.TryGetComponent(out decorator))
                {
                    decorator = unityTileMap.gameObject.AddComponent<TilemapDecorator>();
                }
            }

            if (decorator != null)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        decorator.Resolve(position + new Vector3Int(dx, dy, 0));
                    }
                }
            }

            base.RefreshTile(position, tilemap);
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.colliderType = m_colliderType;

            IEnumerable<TileSet.SpriteOutput> outputs = m_tileSet.MatchRules(this, position, tilemap);
            if (TrySampleSpriteOutput(position, GetTileMap(tilemap), outputs, out TileSet.SpriteOutput output))
            {
                tileData.sprite = output.sprite;
            }
            else
            {
                tileData.sprite = null;
            }
        }

        private bool TrySampleSpriteOutput(Vector3Int position, Tilemap tilemap, IEnumerable<TileSet.SpriteOutput> outputs, out TileSet.SpriteOutput output)
        {
            int ss = tilemap?.GetInstanceID() ?? 0;

            int sx = (ss * 1103515245 + 12345) & int.MaxValue;
            int sy = (sx * 1103515245 + 12345) & int.MaxValue;
            int sz = (sy * 1103515245 + 12345) & int.MaxValue;

            Vector3 pos = position + new Vector3Int(sx, sy, sz);
            Vector3 vec = new Vector3(12.9898f, 78.233f, -35.8033f);

            float random = Mathf.Sin(Vector3.Dot(pos, vec)) * 43758.5453f;
            random -= Mathf.Floor(random);

            float totalWeight = outputs.Sum(o => o.weight);
            random = Mathf.Clamp(random * totalWeight, 0.0f, totalWeight - 0.0001f);

            float cumulativeWeight = 0.0f;
            foreach (TileSet.SpriteOutput o in outputs)
            {
                cumulativeWeight += o.weight;
                if (random < cumulativeWeight)
                {
                    output = o;
                    return true;
                }
            }

            output = default;
            return false;
        }

        private static FieldInfo s_tilemapFieldInfo;

        internal static Tilemap GetTileMap(ITilemap tilemap)
        {
            if (s_tilemapFieldInfo == null)
            {
                s_tilemapFieldInfo = typeof(ITilemap).GetField("m_Tilemap", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            return s_tilemapFieldInfo?.GetValue(tilemap) as Tilemap ?? null;
        }

        #endregion
    }
}
