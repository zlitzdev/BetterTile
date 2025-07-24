using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Reflection;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Zlitz.Extra2D.BetterTile
{
    [CreateAssetMenu(menuName = "Zlitz/Extra 2D/Better Tile/Tile Set")]
    public sealed class TileSet : ScriptableObject
    {
        [SerializeField]
        private string m_id;

        public string id => m_id;

        #if UNITY_EDITOR

        [SerializeField]
        private string m_newBuildId;

        [SerializeField]
        private string m_oldBuildId;

        #endif

        [SerializeField]
        private Tile[] m_tiles;

        [SerializeField]
        private TileCategory[] m_categories;

        [SerializeField]
        private SimpleTile[] m_decorators;

        #if UNITY_EDITOR

        [SerializeField]
        private SpecialTileFilter[] m_specialFilters;

        [Serializable]
        private struct SpecialTileFilter
        {
            [SerializeField]
            private TileFilterType m_type;

            [SerializeField]
            private Color m_color;
        }

        #endif

        [SerializeField]
        private RuleGroup[] m_ruleGroups;

        [SerializeField]
        private OverlayGroup[] m_overlayGroups;

        [SerializeField]
        private TileRule[] m_emptySpriteRules;

        internal IReadOnlyList<OverlayGroup> overlayGroups => m_overlayGroups;

        internal OverlayGroup GetOverlayGroup(string id)
        {
            return m_overlayGroups?.FirstOrDefault(o => o.id == id);
        }

        [Serializable]
        private struct RuleGroup
        {
            [SerializeField]
            private string m_id;

            #if UNITY_EDITOR

            [SerializeField]
            private Texture2D m_texture;

            #endif

            [SerializeField]
            private TileRule[] m_rules;

            public string id => m_id;

            public TileRule[] rules => m_rules;
        }

        [Serializable]
        internal class OverlayGroup
        {
            [SerializeField]
            private string m_id;

            #if UNITY_EDITOR

            [SerializeField]
            private string m_debugName;

            #endif

            [SerializeField]
            private Overlay[] m_overlayPatterns;

            public string id => m_id;

            public IReadOnlyList<Overlay> overlayPatterns => m_overlayPatterns;
        }

        [Serializable]
        internal class Overlay
        {
            [SerializeField]
            private string m_id;

            [SerializeField]
            private Vector2Int m_size;

            [SerializeField]
            private Vector2Int m_cellSize;

            [SerializeField]
            private Vector2Int m_spacing;

            [SerializeField]
            private float m_density;

            [SerializeField]
            private OverlayRule[] m_rules;

            [SerializeField]
            private SimpleTile[] m_overlayTiles;

            public string id => m_id;

            public Vector2Int size => m_size;

            public Vector2Int cellSize => m_cellSize;

            public Vector2Int spacing => m_spacing;

            public float density => m_density;

            public OverlayRule GetRule(Vector2Int offset)
            {
                int index = offset.y * m_size.x + offset.x;
                return m_rules[index];
            }

            public void Initialize()
            {
                for (int i = 0; i < m_rules.Length; i++)
                {
                    OverlayRule rule = m_rules[i];
                    TileOutput output = rule.output;

                    output.assignedTile = m_overlayTiles[i];

                    rule.output = output;
                    m_rules[i] = rule;
                }
            }
        }

        [Serializable]
        internal struct OverlayRule
        {
            [SerializeField]
            private TileOutput m_output;

            [SerializeField]
            private TileFilter m_c;

            [SerializeField]
            private TileFilter m_nx;

            [SerializeField]
            private TileFilter m_px;

            [SerializeField]
            private TileFilter m_ny;

            [SerializeField]
            private TileFilter m_py;

            [SerializeField]
            private TileFilter m_nxny;

            [SerializeField]
            private TileFilter m_pxny;

            [SerializeField]
            private TileFilter m_nxpy;

            [SerializeField]
            private TileFilter m_pxpy;

            public TileOutput output
            {
                get => m_output;
                set => m_output = value;
            }

            public bool Match(Tilemap tilemap, Vector3Int position)
            {
                TileBase    center  = tilemap.GetTile(position);
                TileContext context = new TileContext(tilemap, position);

                return
                    m_c.MatchTile(center) &&
                    m_nx.MatchTile(context.nx) &&
                    m_px.MatchTile(context.px) &&
                    m_ny.MatchTile(context.ny) &&
                    m_py.MatchTile(context.py) &&
                    m_nxny.MatchTile(context.nxny) &&
                    m_pxny.MatchTile(context.pxny) &&
                    m_nxpy.MatchTile(context.nxpy) &&
                    m_pxpy.MatchTile(context.pxpy);
            }
        }

        #region Rule Sets

        internal static HashSet<TileSet> initializedTileSets = new HashSet<TileSet>();

        private readonly Dictionary<Tile, List<RuleSet>> m_tileRuleSets = new Dictionary<Tile, List<RuleSet>>();

        private readonly List<RuleSet> m_decoratorRuleSets = new List<RuleSet>();

        internal void ResetRuleSets()
        {
            m_tileRuleSets.Clear();
            m_decoratorRuleSets.Clear();

            initializedTileSets.Remove(this);
        }

        internal RuleSet[] GetTileRuleSets(Tile tile)
        {
            Initialize();
            return m_tileRuleSets[tile].ToArray();
        }

        internal RuleSet[] GetDecoratorRuleSets()
        {
            Initialize();
            return m_decoratorRuleSets.ToArray();
        }

        internal void Initialize()
        {
            #if UNITY_EDITOR

            if (m_newBuildId != m_oldBuildId)
            {
                m_oldBuildId = m_newBuildId;
                initializedTileSets.Remove(this);
            }

            #endif

            if (!initializedTileSets.Add(this))
            {
                return;
            }

            m_tileRuleSets.Clear();
            m_decoratorRuleSets.Clear();

            if (m_tiles != null)
            {
                foreach (Tile tile in m_tiles)
                {
                    List<RuleSet> ruleSets = new List<RuleSet>();
                    m_tileRuleSets.Add(tile, ruleSets);
                }
            }

            if (m_emptySpriteRules != null)
            {
                int index = 0;
                foreach (TileRule rule in m_emptySpriteRules)
                {
                    int alternatingIndex = rule.alternatingIndex;
                    if (rule.identity.IsTile(out Tile tile))
                    {
                        RuleSet ruleSet = GetRuleSet(tile, alternatingIndex);
                        ruleSet.Insert(rule, assignedTile: tile);
                    }
                    else if (rule.identity.IsDecorator())
                    {
                        SimpleTile decorator = GetDecorator($"empty.{index}", rule.output.sprite);
                        RuleSet decoratorRuleSet = GetDecoratorRuleSet(alternatingIndex);
                        decoratorRuleSet.Insert(rule, assignedTile: decorator);
                    }
                    index++;
                }
            }

            if (m_ruleGroups != null)
            {
                foreach (RuleGroup ruleGroup in m_ruleGroups)
                {
                    if (ruleGroup.rules == null)
                    {
                        continue;
                    }

                    string id = ruleGroup.id;
                    foreach (TileRule rule in ruleGroup.rules)
                    {
                        int alternatingIndex = rule.alternatingIndex;
                        if (rule.identity.IsTile(out Tile tile))
                        {
                            RuleSet ruleSet = GetRuleSet(tile, alternatingIndex);
                            ruleSet.Insert(rule, assignedTile: tile);
                        }
                        else if (rule.identity.IsDecorator())
                        {
                            SimpleTile decorator = GetDecorator(id, rule.output.sprite);
                            RuleSet decoratorRuleSet = GetDecoratorRuleSet(alternatingIndex);
                            decoratorRuleSet.Insert(rule, assignedTile: decorator);
                        }
                    }
                }
            }

            if (m_overlayGroups != null)
            {
                foreach (OverlayGroup overlayGroup in m_overlayGroups)
                {
                    if (overlayGroup.overlayPatterns == null)
                    {
                        continue;
                    }

                    string id = overlayGroup.id;

                    foreach (Overlay overlayPattern in overlayGroup.overlayPatterns)
                    {
                        overlayPattern.Initialize();
                    }
                }
            }

            #if UNITY_EDITOR
            
            onTileSetUpdated?.Invoke(this);

            #endif
        }

        private SimpleTile GetDecorator(string id, Sprite sprite)
        {
            return m_decorators.FirstOrDefault(d => d.id == id && d.sprite == sprite);
        }

        private RuleSet GetRuleSet(Tile tile, int alternatingIndex)
        {
            List<RuleSet> ruleSets = m_tileRuleSets[tile];
            foreach (RuleSet ruleSet in ruleSets)
            {
                if (ruleSet.alternatingIndex == alternatingIndex)
                {
                    return ruleSet;
                }
            }

            RuleSet newRuleSet = new RuleSet(alternatingIndex);

            int insertIndex = 0;
            while (insertIndex < ruleSets.Count && ruleSets[insertIndex].alternatingIndex < alternatingIndex)
            {
                insertIndex++;
            }

            ruleSets.Insert(insertIndex, newRuleSet);
            return newRuleSet;
        }

        private RuleSet GetDecoratorRuleSet(int alternatingIndex)
        {
            foreach (RuleSet ruleSet in m_decoratorRuleSets)
            {
                if (ruleSet.alternatingIndex == alternatingIndex)
                {
                    return ruleSet;
                }
            }

            RuleSet newRuleSet = new RuleSet(alternatingIndex);

            int insertIndex = 0;
            while (insertIndex < m_decoratorRuleSets.Count && m_decoratorRuleSets[insertIndex].alternatingIndex < alternatingIndex)
            {
                insertIndex++;
            }

            m_decoratorRuleSets.Insert(insertIndex, newRuleSet);
            return newRuleSet;
        }

        #endregion

        #region Decorators & Overlays

        internal void ResolveDecorator(TilemapDecoratorLayer decoratorLayer, Vector3Int position)
        {
            if (decoratorLayer == null)
            {
                return;
            }

            decoratorLayer.Resolve(position);
        }

        internal void ResolveOverlay(TilemapOverlayLayer overlayLayer, Vector3Int position)
        {
            if (overlayLayer == null)
            {
                return;
            }

            overlayLayer.Resolve(position, this);
        }

        #endregion

        #region Events


        #if UNITY_EDITOR

        public static event Action<TileSet> onTileSetUpdated;

        #endif

        #endregion
    }

    internal static class TileSetInitialization
    {
        [RuntimeInitializeOnLoadMethod]
        internal static void ResetTileSets()
        {
            TileSet[] tileSets = TileSet.initializedTileSets.ToArray();
            foreach (TileSet tileSet in tileSets)
            {
                tileSet.ResetRuleSets();
                tileSet.Initialize();
            }
        }

        #if UNITY_EDITOR
        
        [InitializeOnLoadMethod]
        internal static void ResetTileSetsEditor()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(TileSet)}");
            if (guids == null)
            {
                return;
            }

            foreach (string guid in guids)
            {
                if (string.IsNullOrEmpty(guid))
                {
                    continue;
                }

                TileSet tileSet = AssetDatabase.LoadAssetAtPath<TileSet>(AssetDatabase.GUIDToAssetPath(guid));
                if (tileSet == null)
                {
                    continue;
                }

                tileSet.ResetRuleSets();
                tileSet.Initialize();
            }
        }

        #endif
    }
}
