using System.Linq;
using System.Collections.Generic;

using Unity.Collections;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zlitz.Extra2D.BetterTile
{
    [RequireComponent(typeof(Tilemap))]
    [HideInInspector]
    [ExecuteAlways]
    public class TilemapDecorator : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_tilemapObject;

        private Tilemap m_tilemap;
        private TilemapRenderer m_tilemapRenderer;

        private Tilemap m_decoratorTilemap;
        private TilemapRenderer m_decoratorTilemapRenderer;

        private GameObject tilemapObject
        {
            get
            {
                if (m_tilemapObject == null)
                {
                    m_tilemapObject = new GameObject("Tilemap Decorator");
                    m_tilemapObject.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;

                    m_tilemapObject.transform.parent = transform;

                    m_decoratorTilemap = m_tilemapObject.AddComponent<Tilemap>();
                    CopyTilemap(m_decoratorTilemap, m_tilemap);

                    m_decoratorTilemapRenderer = m_tilemapObject.AddComponent<TilemapRenderer>();
                    CopyTilemapRenderer(m_decoratorTilemapRenderer, m_tilemapRenderer);
                }
                return m_tilemapObject;
            }
        }

        internal void Resolve(Vector3Int position)
        {
            Init();

            TileBase currentTile = m_tilemap.GetTile(position);
            if (currentTile != null)
            {
                m_decoratorTilemap.SetTile(position, null);
                return;
            }

            TileSet tileSet = null;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    TileBase tile = m_tilemap.GetTile(position + new Vector3Int(dx, dy, 0));
                    if (tile is Tile betterTile)
                    {
                        tileSet = betterTile.tileSet;
                    }
                }
            }

            if (tileSet != null)
            {
                IEnumerable<TileSet.SpriteOutput> outputs = tileSet.MatchRulesForDecorator(position, new ITilemap(m_tilemap));
                if (TrySampleSpriteOutput(position, outputs, out TileSet.SpriteOutput output))
                {
                    m_decoratorTilemap.SetTile(position, tileSet.decorator.GetTile(output.sprite));
                }
                else
                {
                    m_decoratorTilemap.SetTile(position, null);
                }
            }
            else
            {
                m_decoratorTilemap.SetTile(position, null);
            }
        }

        private void Awake()
        {
            hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
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

            m_tilemapObject.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInHierarchy;
        }

        private void OnTileChanged(Tilemap tilemap, Tilemap.SyncTile[] tiles)
        {
            if (tilemap != m_tilemap)
            {
                return;
            }

            if (!tilemap.TryGetComponent(out TilemapDecorator decorator))
            {
                decorator = tilemap.gameObject.AddComponent<TilemapDecorator>();
            }

            foreach (Tilemap.SyncTile syncTile in tiles)
            {
                Resolve(syncTile.position);
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
                Resolve(position);
            }
        }

        private bool TrySampleSpriteOutput(Vector3Int position, IEnumerable<TileSet.SpriteOutput> outputs, out TileSet.SpriteOutput output)
        {
            int ss = m_tilemap?.GetInstanceID() ?? 0;

            int sx = (ss * 1103515245 + 12345) & int.MaxValue;
            int sy = (sx * 1103515245 + 12345) & int.MaxValue;
            int sz = (sy * 1103515245 + 12345) & int.MaxValue;


            Vector3 pos = position;
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

        private void Init()
        {
            if (m_tilemap == null)
            {
                m_tilemap = GetComponent<Tilemap>();
            }

            if (m_tilemapRenderer == null)
            {
                m_tilemapRenderer = GetComponent<TilemapRenderer>();
            }

            if (m_decoratorTilemap == null)
            {
                m_decoratorTilemap = tilemapObject.GetComponent<Tilemap>();
            }

            if (m_decoratorTilemapRenderer == null)
            {
                m_decoratorTilemapRenderer = tilemapObject.GetComponent<TilemapRenderer>();
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
