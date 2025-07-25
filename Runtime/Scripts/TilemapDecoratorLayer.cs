using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Collections;

namespace Zlitz.Extra2D.BetterTile
{
    [ExecuteAlways]
    [AddComponentMenu("Zlitz/Extra2D/Better Tile/Tilemap Decorator Layer")]
    public class TilemapDecoratorLayer : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_obj;

        [SerializeField]
        private Tilemap m_tilemap;

        [SerializeField]
        private Tilemap m_sourceTilemap;

        [SerializeField]
        private TilemapRenderer m_renderer;
        
        [SerializeField]
        private TilemapRenderer m_sourceRenderer;

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

            m_obj = new GameObject("Decorator Layer");
            m_obj.hideFlags = HideFlags.HideInHierarchy;
            m_obj.transform.parent = transform;

            m_tilemap = m_obj.AddComponent<Tilemap>();
            CopyTilemapProperties(m_sourceTilemap, m_tilemap);

            if (tilemap.TryGetComponent(out TilemapRenderer renderer))
            {
                m_sourceRenderer = renderer;

                m_renderer = m_obj.AddComponent<TilemapRenderer>();
                CopyTilemapRendererProperties(m_sourceRenderer, m_renderer);
            }
        }

        public void Resolve(Vector3Int position)
        {
            TileBase currentTile = m_sourceTilemap.GetTile(position);
            if (currentTile != null)
            {
                m_tilemap.SetTile(position, null);
                return;
            }

            TileSet tileSet = null;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    TileBase tile = m_sourceTilemap.GetTile(position + new Vector3Int(dx, dy));
                    if (tile is Tile betterTile)
                    {
                        tileSet = betterTile.tileSet;
                    }
                }
            }

            if (tileSet != null)
            {
                RuleSet decoratorRuleSet = tileSet.GetDecoratorRuleSet();
                if (decoratorRuleSet != null)
                {
                    int alternatingIndex = position.x + position.y + position.z;
                    TileContext context = new TileContext(m_sourceTilemap, position);
                    if (decoratorRuleSet.Sample(context, m_sourceTilemap.GetRandomValue(position), alternatingIndex, out TileOutput output))
                    {
                        m_tilemap.SetTile(position, output.assignedTile);
                    }
                    else
                    {
                        m_tilemap.SetTile(position, null);
                    }
                }
                else
                {
                    m_tilemap.SetTile(position, null);
                }
            }
            else
            {
                m_tilemap.SetTile(position, null);
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

        private void Update()
        {
            if (m_obj != null)
            {
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
    }
}
