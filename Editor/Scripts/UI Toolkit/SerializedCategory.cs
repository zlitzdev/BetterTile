using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace Zlitz.Extra2D.BetterTile
{
    [Serializable]
    internal class SerializedCategory
    {
        [SerializeField]
        private TileCategory m_category;

        [SerializeField]
        private string m_categoryName;

        [SerializeField]
        private Color m_categoryColor;

        [SerializeField]
        private List<Tile> m_tiles = new List<Tile>();

        public TileCategory category => m_category;

        public string categoryName
        {
            get => m_categoryName;
            set => m_categoryName = value;
        }

        public Color categoryColor
        {
            get => m_categoryColor;
            set => m_categoryColor = value;
        }

        public List<Tile> tiles => m_tiles;

        public bool Update(TileCategory category)
        {
            if (m_category != category)
            {
                m_category = category;
                m_categoryName = m_category.name;

                SerializedObject serializedObject = new SerializedObject(m_category);

                SerializedProperty colorProperty = serializedObject.FindProperty("m_color");
                m_categoryColor = colorProperty.colorValue;

                SerializedProperty tilesProperty = serializedObject.FindProperty("m_tiles");
                m_tiles.Clear();
                for (int i = 0; i < tilesProperty.arraySize; i++)
                {
                    Tile tile = tilesProperty.GetArrayElementAtIndex(i).objectReferenceValue as Tile;
                    if (tile != null)
                    {
                        tiles.Add(tile);
                    }
                }
            }

            return false;
        }

        public void SaveChanges()
        {
            SerializedObject serializedObject = new SerializedObject(m_category);

            m_category.name = m_categoryName;

            SerializedProperty colorProperty = serializedObject.FindProperty("m_color");
            colorProperty.colorValue = m_categoryColor.WithAlpha(1.0f);

            SerializedProperty tilesProperty = serializedObject.FindProperty("m_tiles");
            tilesProperty.arraySize = m_tiles.Count;
            for (int i = 0; i < m_tiles.Count; i++)
            {
                SerializedProperty tileProperty = tilesProperty.GetArrayElementAtIndex(i);
                tileProperty.objectReferenceValue = m_tiles[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

}
