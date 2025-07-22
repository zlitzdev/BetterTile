using System;
using System.Reflection;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zlitz.Extra2D.BetterTile
{
    internal static class TilemapExtensions
    {
        private static readonly Type s_iTilemapType = typeof(ITilemap);

        private static readonly FieldInfo s_tilemapField = s_iTilemapType.GetField("m_Tilemap", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public static Tilemap GetTilemap(this ITilemap tilemap)
        {
            return s_tilemapField.GetValue(tilemap) as Tilemap;
        }

        public static float GetRandomValue(this ITilemap tilemap, Vector3Int position)
        {
            return GetRandomValue(tilemap.GetTilemap(), position);
        }

        public static float GetRandomValue(this Tilemap tilemap, Vector3Int position)
        {
            long seed = tilemap?.GetUniqueId().GetHashCode() ?? 0;

            seed ^= position.x + 0x9e3779b9 + (seed << 6) + (seed >> 2);
            seed ^= position.y + 0x9e3779b9 + (seed << 6) + (seed >> 2);
            seed ^= position.z + 0x9e3779b9 + (seed << 6) + (seed >> 2);

            ulong v = (ulong)seed;

            v ^= v >> 33;
            v *= 0xff51afd7ed558ccdUL;
            v ^= v >> 33;
            v *= 0xc4ceb9fe1a85ec53UL;
            v ^= v >> 33;

            ulong bits = (uint)(v >> 41) | 0x3f800000;
            float result = BitConverter.Int32BitsToSingle((int)bits);

            return result - 1.0f;
        }

        public static string GetUniqueId(this Tilemap tilemap)
        {
            if (tilemap == null)
            {
                return null;
            }

            if (!tilemap.TryGetComponent(out TilemapUniqueId uniqueId))
            {
                uniqueId = tilemap.gameObject.AddComponent<TilemapUniqueId>();
            }

            return uniqueId.id;
        }

        public static TilemapDecoratorLayer GetDecoratorLayer(this Tilemap tilemap)
        {
            if (!tilemap.TryGetComponent(out TilemapDecoratorLayer decoratorLayer))
            {
                decoratorLayer = tilemap.gameObject.AddComponent<TilemapDecoratorLayer>();
            }

            decoratorLayer.Initialize(tilemap);

            return decoratorLayer;
        }
    
        public static TilemapOverlayLayer GetOverlayLayer(this Tilemap tilemap)
        {
            if (!tilemap.TryGetComponent(out TilemapOverlayLayer overlayLayer))
            {
                overlayLayer = tilemap.gameObject.AddComponent<TilemapOverlayLayer>();
            }

            overlayLayer.Initialize(tilemap);

            return overlayLayer;
        }
    }
}
