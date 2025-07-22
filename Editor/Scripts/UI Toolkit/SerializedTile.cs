using System;

using UnityEditor;
using UnityEngine;

using UnityTile = UnityEngine.Tilemaps.Tile;

namespace Zlitz.Extra2D.BetterTile
{
    [Serializable]
    internal class SerializedTile
    {
        [SerializeField]
        private Tile m_tile;

        [SerializeField]
        private string m_tileName;

        [SerializeField]
        private Color m_tileColor;

        [SerializeField]
        private UnityTile.ColliderType m_colliderType;

        [SerializeField]
        private Sprite m_sprite;

        public Tile tile => m_tile;

        public string tileName
        {
            get => m_tileName;
            set => m_tileName = value;
        }

        public Color tileColor
        {
            get => m_tileColor;
            set => m_tileColor = value;
        }

        public UnityTile.ColliderType colliderType
        {
            get => m_colliderType;
            set => m_colliderType = value;
        }

        public Sprite sprite
        {
            get => m_sprite;
            set => m_sprite = value;
        }

        public bool Update(Tile tile)
        {
            if (m_tile != tile)
            {
                m_tile = tile;
                m_tileName = m_tile.name;

                SerializedObject serializedObject = new SerializedObject(m_tile);

                SerializedProperty colorProperty = serializedObject.FindProperty("m_color");
                m_tileColor = colorProperty.colorValue;

                SerializedProperty colliderTypeProperty = serializedObject.FindProperty("m_colliderType");
                m_colliderType = (UnityTile.ColliderType)colliderTypeProperty.enumValueIndex;

                SerializedProperty spriteProperty = serializedObject.FindProperty("m_sprite");
                m_sprite = spriteProperty.objectReferenceValue as Sprite;
            }

            return false;
        }

        public void SaveChanges()
        {
            SerializedObject serializedObject = new SerializedObject(m_tile);

            m_tile.name = m_tileName;

            SerializedProperty colorProperty = serializedObject.FindProperty("m_color");
            colorProperty.colorValue = m_tileColor.WithAlpha(1.0f);

            SerializedProperty colliderTypeProperty = serializedObject.FindProperty("m_colliderType");
            colliderTypeProperty.enumValueIndex = (int)m_colliderType;

            SerializedProperty spriteProperty = serializedObject.FindProperty("m_sprite");
            spriteProperty.objectReferenceValue = m_sprite;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

}
