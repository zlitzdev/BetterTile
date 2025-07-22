using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Zlitz.Extra2D.BetterTile
{
    [Serializable]
    internal class SerializedTileFilter
    {
        [SerializeField]
        private TileFilterType m_type;

        [SerializeField]
        private bool m_inverted;

        [SerializeField]
        private Tile m_tile;

        [SerializeField]
        private TileCategory m_category;

        public TileFilterType type
        {
            get => m_type;
            set => m_type = value;
        }

        public bool inverted
        {
            get => m_inverted;
            set => m_inverted = value;
        }

        public Tile tile
        {
            get => m_tile;
            set => m_tile = value;
        }

        public TileCategory category
        {
            get => m_category;
            set => m_category = value;
        }

        public bool SetTile(Tile tile, bool inverted)
        {
            if (m_type != TileFilterType.Tile || m_tile != tile || m_inverted != inverted)
            {
                m_type = TileFilterType.Tile;
                m_inverted = inverted;
                m_tile = tile;
                m_category = null;

                return true;
            }

            return false;
        }

        public bool SetCategory(TileCategory category, bool inverted)
        {
            if (m_type != TileFilterType.TileCategory || m_category != category || m_inverted != inverted)
            {
                m_type = TileFilterType.TileCategory;
                m_inverted = inverted;
                m_category = category;
                m_tile = null;

                return true;
            }

            return false;
        }

        public bool SetSpecialFilter(TileFilterType filterType, bool inverted)
        {
            if (filterType == TileFilterType.Any)
            {
                inverted = false;
            }

            m_tile = null;
            m_category = null;
            if (m_type != filterType || m_inverted != inverted)
            {
                m_type = filterType;
                m_inverted = inverted;
                return true;
            }

            return false;
        }

        public bool Update(SerializedProperty tileFilterProperty)
        {
            SerializedProperty typeProperty = tileFilterProperty.FindPropertyRelative("m_type");
            m_type = (TileFilterType)typeProperty.enumValueIndex;

            SerializedProperty invertedProperty = tileFilterProperty.FindPropertyRelative("m_inverted");
            m_inverted = invertedProperty.boolValue;

            SerializedProperty tileProperty = tileFilterProperty.FindPropertyRelative("m_tile");
            m_tile = tileProperty.objectReferenceValue as Tile;

            SerializedProperty categoryProperty = tileFilterProperty.FindPropertyRelative("m_category");
            m_category = categoryProperty.objectReferenceValue as TileCategory;

            return false;
        }

        public bool OnTilesDeleted(IEnumerable<SerializedTile> tiles)
        {
            if (m_type == TileFilterType.Tile)
            {
                SerializedTile serializedTile = tiles.FirstOrDefault(t => t.tile == m_tile);
                if (serializedTile != null)
                {
                    m_type = TileFilterType.Any;
                    m_tile = null;
                    m_inverted = false;
                    return true;
                }
            }
            else
            {
                m_tile = null;
            }

            return false;
        }

        public bool OnCategoriesDeleted(IEnumerable<SerializedCategory> categories)
        {
            if (m_type == TileFilterType.TileCategory)
            {
                SerializedCategory serializedCategory = categories.FirstOrDefault(c => c.category == m_category);
                if (serializedCategory != null)
                {
                    m_type = TileFilterType.Any;
                    m_category = null;
                    m_inverted = false;
                    return true;
                }
            }
            else
            {
                m_category = null;
            }

            return false;
        }

        public void SaveChanges(SerializedProperty tileFilterProperty)
        {
            SerializedProperty typeProperty = tileFilterProperty.FindPropertyRelative("m_type");
            typeProperty.enumValueIndex = (int)m_type;

            SerializedProperty invertedProperty = tileFilterProperty.FindPropertyRelative("m_inverted");
            invertedProperty.boolValue = m_inverted;

            SerializedProperty tileProperty = tileFilterProperty.FindPropertyRelative("m_tile");
            tileProperty.objectReferenceValue = m_tile;

            SerializedProperty categoryProperty = tileFilterProperty.FindPropertyRelative("m_category");
            categoryProperty.objectReferenceValue = m_category;
        }
    }
}
