using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zlitz.Extra2D.BetterTile
{
    [CreateAssetMenu(menuName = "Zlitz/Extra2D/Better Tile/Tile Set", fileName = "Tile Set")]
    public sealed class TileSet : ScriptableObject
    {
        #region Editor-only properties
        #if UNITY_EDITOR

        [SerializeField]
        private Texture2D[] m_textures;

        #endif
        #endregion

        [SerializeField]
        private SpriteEntry[] m_spriteEntries;

        [SerializeField]
        private TileCategory[] m_categories;

        [SerializeField]
        private Tile[] m_tiles;

        [SerializeField]
        private SelfTileFilter m_selfFilter;

        [SerializeField]
        private TileDecorator m_decorator;

        public IEnumerable<TileCategory> categories => m_categories ?? Enumerable.Empty<TileCategory>();

        public IEnumerable<Tile> tiles => m_tiles ?? Enumerable.Empty<Tile>();

        internal TileDecorator decorator => m_decorator;

        internal IEnumerable<SpriteOutput> MatchRules(Tile tile, Vector3Int position, ITilemap tilemap)
        {
            List<SpriteOutput> results = new List<SpriteOutput>();

            ConnectionRule currentRule = default;
            foreach (SpriteEntry spriteEntry in m_spriteEntries)
            {
                if (spriteEntry.tile == null)
                {
                    continue;
                }

                Tile parentTile = tile;
                bool isCorrectTileType = false;
                while (parentTile != null)
                {
                    if (parentTile == spriteEntry.tile)
                    {
                        isCorrectTileType = true;
                        break;
                    }
                    parentTile = parentTile.baseTile;
                }

                if (!isCorrectTileType)
                {
                    continue;
                }

                bool matchedRules = spriteEntry.connectionRule.Check(tilemap, position, tile);
                if (!matchedRules)
                {
                    continue;
                }

                bool currentRuleIsGeneralized = currentRule.IsGeneralizedOf(spriteEntry.connectionRule);
                if (currentRuleIsGeneralized || spriteEntry.connectionRule.alwaysUsed)
                {
                    if (currentRuleIsGeneralized && !spriteEntry.connectionRule.IsGeneralizedOf(currentRule))
                    {
                        results = results.Where(r => r.connectionRule.alwaysUsed).ToList();
                    }

                    if (currentRuleIsGeneralized)
                    {
                        currentRule = spriteEntry.connectionRule;
                    }

                    results.Add(new SpriteOutput(spriteEntry.tile, spriteEntry.connectionRule, spriteEntry.sprite, spriteEntry.weight));
                }
            }

            Tile baseTile = tile;
            while (baseTile != null)
            {
                if (baseTile.overwriteRules && results.Any(o => o.ruleProvider == baseTile))
                {
                    results = results.Where(o => o.ruleProvider.IsDescendantOf(baseTile)).ToList();
                    break;
                }
                baseTile = baseTile.baseTile;
            }

            return results;
        }

        internal IEnumerable<SpriteOutput> MatchRulesForDecorator(Vector3Int position, ITilemap tilemap)
        {
            List<SpriteOutput> results = new List<SpriteOutput>();

            ConnectionRule currentRule = default;
            foreach (SpriteEntry spriteEntry in m_spriteEntries)
            {
                if (spriteEntry.decorator == null)
                {
                    continue;
                }

                bool matchedRules = spriteEntry.connectionRule.Check(tilemap, position, null);
                if (!matchedRules)
                {
                    continue;
                }

                bool currentRuleIsGeneralized = currentRule.IsGeneralizedOf(spriteEntry.connectionRule);
                if (currentRuleIsGeneralized || spriteEntry.connectionRule.alwaysUsed)
                {
                    if (currentRuleIsGeneralized && !spriteEntry.connectionRule.IsGeneralizedOf(currentRule))
                    {
                        results = results.Where(r => r.connectionRule.alwaysUsed).ToList();
                    }

                    if (currentRuleIsGeneralized)
                    {
                        currentRule = spriteEntry.connectionRule;
                    }

                    results.Add(new SpriteOutput(spriteEntry.tile, spriteEntry.connectionRule, spriteEntry.sprite, spriteEntry.weight));
                }
            }

            return results;
        }


        [Serializable]
        private class SpriteEntry
        {
            [SerializeField]
            private Sprite m_sprite;

            [SerializeField]
            private float m_weight;

            [SerializeField]
            private UnityEngine.Object m_tile;

            [SerializeField]
            private ConnectionRule m_rule;

            public Sprite sprite => m_sprite;

            public float weight => m_weight;

            public Tile tile => m_tile as Tile;

            public TileDecorator decorator => m_tile as TileDecorator;

            public ConnectionRule connectionRule => m_rule;
        }

        internal struct SpriteOutput
        {
            public Tile ruleProvider { get; private set; }

            public ConnectionRule connectionRule { get; private set; }

            public float weight { get; private set; }

            public Sprite sprite { get; private set; }

            public SpriteOutput(Tile ruleProvider, ConnectionRule connectionRule, Sprite sprite, float weight)
            {
                this.ruleProvider = ruleProvider;

                this.connectionRule = connectionRule;

                this.weight = weight;
                this.sprite = sprite;
            }
        }
    }
}
