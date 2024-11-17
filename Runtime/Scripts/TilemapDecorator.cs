using System;
using System.Collections.Generic;

using Unity.Collections;

using UnityEngine;
using UnityEngine.Tilemaps;

using static UnityEngine.Tilemaps.Tilemap;

namespace Zlitz.Extra2D.BetterTile
{
    [RequireComponent(typeof(Tilemap))]
    [HideInInspector]
    [ExecuteAlways]
    public class TilemapDecorator : MonoBehaviour
    {
        private Tilemap         m_tilemap;
        private TilemapRenderer m_tilemapRenderer;

        [SerializeField]
        private Tilemap m_decoratorTilemap;

        [SerializeField]
        private TilemapRenderer m_decoratorTilemapRenderer;

        internal void Set(Vector3Int position, TileBase tile)
        {
            Init();
            m_decoratorTilemap.SetTile(position, tile);
        }

        internal void Remove(Vector3Int position)
        {
            Init();
            m_decoratorTilemap.SetTile(position, null);
        }

        private void Awake()
        {
            hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

            m_tilemap         = GetComponent<Tilemap>();
            m_tilemapRenderer = GetComponent<TilemapRenderer>();

            Init();
        }

        private void OnEnable()
        {
            Tilemap.tilemapPositionsChanged += OnTileChanged;
            Tilemap.tilemapTileChanged      += OnTileChanged;

            m_decoratorTilemap?.gameObject.SetActive(true);
            m_decoratorTilemapRenderer?.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            Tilemap.tilemapPositionsChanged -= OnTileChanged;
            Tilemap.tilemapTileChanged      -= OnTileChanged;

            m_decoratorTilemap?.gameObject.SetActive(false);
            m_decoratorTilemapRenderer?.gameObject.SetActive(false);
        }

        private void Update()
        {
            CopyTilemap(m_decoratorTilemap, m_tilemap);
            CopyTilemapRenderer(m_decoratorTilemapRenderer, m_tilemapRenderer);
        }

        private void OnTileChanged(Tilemap tilemap, SyncTile[] tiles)
        {
            if (tilemap != m_tilemap)
            {
                return;
            }

            if (!tilemap.TryGetComponent(out TilemapDecorator decorator))
            {
                decorator = tilemap.gameObject.AddComponent<TilemapDecorator>();
            }

            foreach (SyncTile syncTile in tiles)
            {
                Vector3Int position = syncTile.position;

                bool handled = false;
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        TileBase tile = tilemap.GetTile(position + new Vector3Int(dx, dy, 0));
                        if (tile is Tile betterTile)
                        {
                            betterTile.UpdateDecorator(tilemap, decorator, position);
                            handled = true;
                            break;
                        }
                    }
                    if (handled)
                    {
                        break;
                    }
                }

                if (!handled)
                {
                    decorator.Remove(position);
                }
            }
        }

        private void OnTileChanged(Tilemap tilemap, NativeArray<Vector3Int> positions)
        {
            if (tilemap != m_tilemap)
            {
                return;
            }

            if (!tilemap.TryGetComponent(out TilemapDecorator decorator))
            {
                decorator = tilemap.gameObject.AddComponent<TilemapDecorator>();
            }

            foreach (Vector3Int position in positions)
            {
                bool handled = false;
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        TileBase tile = tilemap.GetTile(position + new Vector3Int(dx, dy, 0));
                        if (tile is Tile betterTile)
                        {
                            betterTile.UpdateDecorator(tilemap, decorator, position);
                            handled = true;
                            break;
                        }
                    }
                    if (handled)
                    {
                        break;
                    }
                }

                if (!handled)
                {
                    decorator.Remove(position);
                }
            }
        }

        private void Init()
        {
            if (m_decoratorTilemap == null || m_decoratorTilemapRenderer == null)
            {
                GameObject decorator = new GameObject("Tilemap Decorator");
                decorator.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

                decorator.transform.parent = transform;

                m_decoratorTilemap = decorator.AddComponent<Tilemap>();
                CopyTilemap(m_decoratorTilemap, m_tilemap);

                m_decoratorTilemapRenderer = decorator.AddComponent<TilemapRenderer>();
                CopyTilemapRenderer(m_decoratorTilemapRenderer, m_tilemapRenderer);
            }
        }

        private static void CopyTilemap(Tilemap dest, Tilemap source)
        {
            dest.animationFrameRate = source.animationFrameRate;
            dest.color              = source.color;
            dest.tileAnchor         = source.tileAnchor;
            dest.orientation        = source.orientation;
        }

        private static void CopyTilemapRenderer(TilemapRenderer dest, TilemapRenderer source)
        {
            dest.enabled = source?.enabled ?? false;

            if (source != null)
            {
                dest.maskInteraction          = source.maskInteraction;
                dest.sharedMaterial           = source.sharedMaterial;
                dest.sortingLayerID           = source.sortingLayerID;
                dest.sortingOrder             = source.sortingOrder;
                dest.sortOrder                = source.sortOrder;
                dest.mode                     = source.mode;
                dest.detectChunkCullingBounds = source.detectChunkCullingBounds;
                dest.chunkCullingBounds       = source.chunkCullingBounds;
            }
        }
    }
}
