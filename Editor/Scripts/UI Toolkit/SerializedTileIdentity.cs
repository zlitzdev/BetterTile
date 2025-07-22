using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Zlitz.Extra2D.BetterTile
{
    [Serializable]
    internal class SerializedTileIdentity
    {
        [SerializeField]
        private TileIdentityType m_type;

        [SerializeField]
        private Tile m_tile;

        [SerializeField]
        private TileFilterType m_filterType;

        public TileIdentityType type
        {
            get => m_type;
            set => m_type = value;
        }

        public Tile tile
        {
            get => m_tile;
            set => m_tile = value;
        }

        public TileFilterType filterType
        {
            get => m_filterType;
            set => m_filterType = value;
        }

        public bool IsTile(out Tile tile)
        {
            tile = null;

            if (m_type == TileIdentityType.Tile)
            {
                tile = m_tile;
                return true;
            }

            return false;
        }

        public bool IsDecorator()
        {
            return m_type == TileIdentityType.Other && m_filterType == TileFilterType.Decorator;
        }

        public bool SetTile(Tile tile)
        {
            if (m_type != TileIdentityType.Tile || m_tile != tile)
            {
                m_type = TileIdentityType.Tile;
                m_tile = tile;

                return true;
            }

            return false;
        }

        public bool SetSpecialFilter(TileFilterType filterType)
        {
            TileIdentityType newType = TileIdentityType.Other;
            if (filterType == TileFilterType.Any)
            {
                newType = TileIdentityType.None;
            }

            m_tile = null;
            m_filterType = filterType;
            if (m_type != newType)
            {
                m_type = newType;
                return true;
            }

            return false;
        }

        public bool Update(SerializedProperty tileIdentityProperty)
        {
            SerializedProperty typeProperty = tileIdentityProperty.FindPropertyRelative("m_type");
            m_type = (TileIdentityType)typeProperty.enumValueIndex;

            SerializedProperty tileProperty = tileIdentityProperty.FindPropertyRelative("m_tile");
            m_tile = tileProperty.objectReferenceValue as Tile;

            SerializedProperty filterTypeProperty = tileIdentityProperty.FindPropertyRelative("m_filterType");
            m_filterType = (TileFilterType)filterTypeProperty.enumValueIndex;

            return false;
        }

        public bool OnTilesDeleted(IEnumerable<SerializedTile> tiles)
        {
            SerializedTile serializedTile = tiles.FirstOrDefault(t => t.tile == m_tile);
            if (serializedTile != null)
            {
                m_type = TileIdentityType.None;
                m_tile = null;
                return true;
            }

            return false;
        }

        public void SaveChanges(SerializedProperty tileIdentityProperty)
        {
            SerializedProperty typeProperty = tileIdentityProperty.FindPropertyRelative("m_type");
            typeProperty.enumValueIndex = (int)m_type;

            SerializedProperty tileProperty = tileIdentityProperty.FindPropertyRelative("m_tile");
            tileProperty.objectReferenceValue = m_tile;

            SerializedProperty filterTypeProperty = tileIdentityProperty.FindPropertyRelative("m_filterType");
            filterTypeProperty.enumValueIndex = (int)m_filterType;
        }
    }
}
