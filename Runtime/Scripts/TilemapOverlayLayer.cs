using System;
using System.Linq;
using Unity.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zlitz.Extra2D.BetterTile
{
    [ExecuteAlways]
    internal class TilemapOverlayLayer : MonoBehaviour
    {
        [SerializeField]
        private Tilemap m_sourceTilemap;

        [SerializeField]
        private TilemapRenderer m_sourceRenderer;

        [SerializeField]
        private List<Layer> m_layers;

        private List<Layer> m_filteredLayers;

        internal static readonly HashSet<Tilemap> tilemaps = new HashSet<Tilemap>();

        public void Initialize(Tilemap tilemap)
        {
            if (m_sourceTilemap != null)
            {
                return;
            }

            Tilemap.tilemapTileChanged      += OnTileChanged;
            Tilemap.tilemapPositionsChanged += OnTileChanged;

            m_sourceTilemap = tilemap;
            tilemaps.Add(m_sourceTilemap);

            m_sourceRenderer = tilemap.GetComponent<TilemapRenderer>();
        }

        public void Resolve(Vector3Int position, TileSet tileSet = null)
        {
            if (tileSet != null)
            {
                foreach (TileSet.OverlayGroup overlayGroup in tileSet.overlayGroups)
                {
                    GetLayer(tileSet, overlayGroup.id);
                }
            }

            if (m_layers == null)
            {
                return;
            }

            foreach (Layer layer in m_layers)
            {
                layer.Resolve(position);
            }
        }

        private void OnDestroy()
        {
            if (m_sourceTilemap != null)
            {
                Tilemap.tilemapTileChanged      -= OnTileChanged;
                Tilemap.tilemapPositionsChanged -= OnTileChanged;

                tilemaps.Remove(m_sourceTilemap);
                m_sourceTilemap = null;
            }
        }

        private void OnEnable()
        {
            #if UNITY_EDITOR

            TileSet.onTileSetUpdated += OnTileSetUpdated;

            #endif
        }

        private void OnDisable()
        {
            #if UNITY_EDITOR

            TileSet.onTileSetUpdated -= OnTileSetUpdated;

            #endif
        }

        private void Update()
        {
            hideFlags = HideFlags.HideInInspector;

            if (m_layers != null)
            {
                foreach (Layer layer in m_layers)
                {
                    layer.Update();
                }
            }
        }

        private Layer GetLayer(TileSet tileSet, string overlayGroupId)
        {
            if (m_layers == null)
            {
                m_layers = new List<Layer>();
            }

            Layer layer = m_layers.FirstOrDefault(l => l.tileSet == tileSet && l.overlayGroupId == overlayGroupId);
            if (layer == null)
            {
                layer = new Layer(m_sourceTilemap, m_sourceRenderer, tileSet, overlayGroupId);
                m_layers.Add(layer);
            }

            return layer;
        }

        #if UNITY_EDITOR

        internal void OnTileSetUpdated(TileSet tileSet)
        {
            if (m_layers != null)
            {
                if (m_filteredLayers == null)
                {
                    m_filteredLayers = new List<Layer>();
                }
                m_filteredLayers.Clear();

                List<Layer> temp = m_filteredLayers;
                m_filteredLayers = m_layers;

                foreach (Layer layer in m_layers)
                {
                    layer.OnTileSetUpdated(tileSet, out bool shouldRemove);
                    if (!shouldRemove)
                    {
                        temp.Add(layer);
                    }
                    else
                    {
                        layer.Remove();
                    }
                }

                m_layers = temp;
            }
        }

        #endif

        private void OnTileChanged(Tilemap tilemap, NativeArray<Vector3Int> positions)
        {
            if (tilemap != m_sourceTilemap)
            {
                return;
            }

            foreach (Vector3Int position in positions)
            {
                Resolve(position);
            }
        }

        private void OnTileChanged(Tilemap tilemap, Tilemap.SyncTile[] tiles)
        {
            if (tilemap != m_sourceTilemap)
            {
                return;
            }

            foreach (Tilemap.SyncTile syncTile in tiles)
            {
                Resolve(syncTile.position);
            }
        }

        private static void CopyTilemapProperties(Tilemap source, Tilemap destination)
        {
            destination.tileAnchor         = source.tileAnchor;
            destination.orientation        = source.orientation;
            destination.color              = source.color;
            destination.animationFrameRate = source.animationFrameRate;
        }

        private static void CopyTilemapRendererProperties(TilemapRenderer source, TilemapRenderer destination)
        {
            destination.maskInteraction          = source.maskInteraction;
            destination.sortingLayerID           = source.sortingLayerID;
            destination.sortingOrder             = source.sortingOrder;
            destination.sortOrder                = source.sortOrder;
            destination.mode                     = source.mode;
            destination.detectChunkCullingBounds = source.detectChunkCullingBounds;
            destination.chunkCullingBounds       = source.chunkCullingBounds;
            destination.sharedMaterial           = source.sharedMaterial;
        }

        [Serializable]
        private class Layer
        {
            [SerializeField]
            private Tilemap m_sourceTilemap;

            [SerializeField]
            private TilemapRenderer m_sourceRenderer;

            [SerializeField]
            private GameObject m_obj;

            [SerializeField]
            private Tilemap m_tilemap;

            [SerializeField]
            private TilemapRenderer m_renderer;

            [SerializeField]
            private TileSet m_tileSet;

            [SerializeField]
            private string m_overlayGroupId;

            [SerializeField]
            private List<Pattern> m_patterns = new List<Pattern>();

            private TileSet.OverlayGroup m_overlayGroup;

            private Entry[] m_entries;

            public TileSet tileSet => m_tileSet;

            public string overlayGroupId => m_overlayGroupId;

            public TileSet.OverlayGroup overlayGroup
            {
                get
                {
                    if (m_overlayGroup == null)
                    {
                        m_overlayGroup = m_tileSet.GetOverlayGroup(m_overlayGroupId);

                        m_entries = null;
                        if (m_overlayGroup != null)
                        {
                            m_entries = new Entry[m_overlayGroup.overlayPatterns.Count];
                            for (int i = 0; i < m_overlayGroup.overlayPatterns.Count; i++)
                            {
                                m_entries[i] = new Entry(m_overlayGroup.overlayPatterns[i], $"{m_sourceTilemap.GetUniqueId()}_{m_tileSet.id}_{m_overlayGroupId}_{i}".GetHashCode());
                            }
                        }
                    }
                    return m_overlayGroup;
                }
            }

            public void Remove()
            {
                if (m_obj != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(m_obj);
                    }
                    else
                    {
                        DestroyImmediate(m_obj);
                    }
                }
            }

            public void Update()
            {
                if (m_obj != null)
                {
                    m_obj.hideFlags = HideFlags.HideInHierarchy;
                    m_obj.transform.localPosition = new Vector3(0.0f, 0.0f, -0.0001f);
                }

                if (m_renderer != null && m_sourceRenderer != null)
                {
                    m_renderer.enabled = m_sourceRenderer.enabled;
                    CopyTilemapRendererProperties(m_sourceRenderer, m_renderer);
                }

                if (m_tilemap != null && m_sourceTilemap != null)
                {
                    m_tilemap.enabled = m_sourceTilemap.enabled;
                    CopyTilemapProperties(m_sourceTilemap, m_tilemap);
                }
            }

            public void Resolve(Vector3Int position)
            {
                if (m_entries != null)
                {
                    foreach (Entry e in m_entries)
                    {
                        e.Update();
                    }
                }

                (int i, ScatteredRectNoise.SampleResult r) = Sample(position);
                if (i < 0)
                {
                    RemovePattern(position);
                    return;
                }

                Entry entry = m_entries[i];

                for (int x = 0; x < m_entries[i].pattern.size.x; x++)
                {
                    for (int y = 0; y < m_entries[i].pattern.size.y; y++)
                    {
                        Vector3Int p = position;
                        p.x = r.rectPosition.x + x;
                        p.y = r.rectPosition.y + y;

                        if (!entry.pattern.GetRule(new Vector2Int(x, y)).Match(m_sourceTilemap, p))
                        {
                            goto notMatched;
                        }

                        (int i2, ScatteredRectNoise.SampleResult r2) = Sample(p);
                        if (i2 != i)
                        {
                            goto notMatched;
                        }
                    }
                }

                Vector3Int pos = position;
                pos.x = r.rectPosition.x;
                pos.y = r.rectPosition.y;
                SetPattern(pos, m_entries[i].pattern, i);
                return;

            notMatched:

                RemovePattern(position);
            }

            public void OnTileSetUpdated(TileSet tileSet, out bool shouldRemove)
            {
                shouldRemove = false;

                if (tileSet != null)
                {
                    if (m_tileSet != tileSet)
                    {
                        shouldRemove = m_tileSet == null;
                        return;
                    }
                }
                else
                {
                    if (m_tileSet == null)
                    {
                        shouldRemove = true;
                        return;
                    }
                }

                m_overlayGroup = null;
                if (overlayGroup == null)
                {
                    shouldRemove = true;
                    return;
                }

                EvaluateAllTiles();
            }

            public Layer(Tilemap sourceTilemap, TilemapRenderer sourceRenderer, TileSet tileSet, string overlayGroupId)
            {
                m_tileSet = tileSet;
                m_overlayGroupId = overlayGroupId;

                m_sourceTilemap  = sourceTilemap;
                m_sourceRenderer = sourceRenderer;

                m_obj = new GameObject("Overlay Layer");
                m_obj.transform.parent = sourceTilemap.transform;
                m_obj.hideFlags = HideFlags.HideInHierarchy;

                m_tilemap = m_obj.AddComponent<Tilemap>();
                CopyTilemapProperties(m_sourceTilemap, m_tilemap);

                if (m_sourceRenderer != null)
                {
                    m_renderer = m_obj.AddComponent<TilemapRenderer>();
                    CopyTilemapRendererProperties(m_sourceRenderer, m_renderer);
                }
            }

            internal void EvaluateAllTiles()
            {
                m_patterns?.Clear();
                m_tilemap.ClearAllTiles();

                BoundsInt bounds = m_sourceTilemap.cellBounds;
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    for (int y = bounds.yMin; y < bounds.yMax; y++)
                    {
                        for (int z = bounds.zMin; z < bounds.zMax; z++)
                        {
                            Vector3Int pos = new Vector3Int(x, y, z);
                            Resolve(pos);
                        }
                    }
                }
            }

            private (int, ScatteredRectNoise.SampleResult) Sample(Vector3Int position)
            {
                if (overlayGroup == null || m_entries == null)
                {
                    return (-1, ScatteredRectNoise.SampleResult.NotInRect());
                }

                for (int i = 0; i < m_entries.Length; i++)
                {
                    ScatteredRectNoise.SampleResult result = m_entries[i].noise.Sample(position);
                    if (result.inRect)
                    {
                        return (i, result);
                    }
                }

                return (-1, ScatteredRectNoise.SampleResult.NotInRect());
            }

            private void SetPattern(Vector3Int position, TileSet.Overlay pattern, int index)
            {
                if (m_patterns != null && m_patterns.Any(pat => pat.Overlaps(position, pattern.size)))
                {
                    return;
                }

                if (m_patterns == null)
                {
                    m_patterns = new List<Pattern>();
                }

                m_patterns.Add(new Pattern(position, pattern.size, index));

                for (int x = 0; x < pattern.size.x; x++)
                {
                    for (int y = 0; y < pattern.size.y; y++)
                    {
                        Vector3Int p = position;
                        p.x += x;
                        p.y += y;

                        TileSet.OverlayRule rule = pattern.GetRule(new Vector2Int(x, y));
                        m_tilemap.SetTile(p + Vector3Int.forward, rule.output.assignedTile);
                    }
                }
            }

            private void RemovePattern(Vector3Int position)
            {
                if (m_patterns != null)
                {
                    int found = -1;
                    int index = 0;
                    foreach (Pattern pattern in m_patterns)
                    {
                        if (pattern.Contains(position))
                        {
                            found = index;

                            for (int x = 0; x < pattern.size.x; x++)
                            {
                                for (int y = 0; y < pattern.size.y; y++)
                                {
                                    Vector3Int pos = pattern.position + new Vector3Int(x, y);

                                    (int i, ScatteredRectNoise.SampleResult r) = Sample(pos);
                                    if (i == pattern.index)
                                    {
                                        m_tilemap.SetTile(pos + Vector3Int.forward, null);
                                    }
                                }
                            }

                            break;
                        }
                        index++;
                    }

                    if (found >= 0)
                    {
                        m_patterns.RemoveAt(found);
                    }
                }
            }

            private class Entry
            {
                public TileSet.Overlay pattern { get; private set; }

                public ScatteredRectNoise noise { get; private set; }

                public void Update()
                {
                    noise.cellSize = pattern.cellSize;
                    noise.rectSize = pattern.size;
                    noise.density  = pattern.density;
                }

                public Entry(TileSet.Overlay overlayPattern, int seed)
                {
                    pattern = overlayPattern;
                    noise = new ScatteredRectNoise(pattern.cellSize, pattern.size, pattern.spacing, pattern.density, seed);
                }
            }

            [Serializable]
            private struct Pattern
            {
                [SerializeField]
                private Vector3Int m_position;

                [SerializeField]
                private Vector2Int m_size;

                [SerializeField]
                private int m_index;

                public Vector3Int position => m_position;

                public Vector2Int size => m_size;

                public int index => m_index;

                public bool Contains(Vector3Int position)
                {
                    Vector3Int offset = position - m_position;
                    if (offset.z != 0)
                    {
                        return false;
                    }

                    if (offset.x < 0 || offset.x >= m_size.x)
                    {
                        return false;
                    }

                    if (offset.y < 0 || offset.y >= m_size.y)
                    {
                        return false;
                    }

                    return true;
                }

                public bool Overlaps(Vector3Int position, Vector2Int size)
                {
                    if (position.z != m_position.z)
                    {
                        return false;
                    }

                    Vector2Int max1 = new Vector2Int(
                        m_position.x + m_size.x,    
                        m_position.y + m_size.y    
                    );

                    Vector2Int max2 = new Vector2Int(
                        position.x + size.x,
                        position.y + size.y
                    );

                    if (max2.x <= m_position.x || max1.x <= position.x) return false;
                    if (max2.y <= m_position.y || max1.y <= position.y) return false;

                    return true;
                }

                public Pattern(Vector3Int position, Vector2Int size, int index)
                {
                    m_position = position;
                    m_size     = size;
                    m_index    = index;
                }
            }
        }
    }
}
