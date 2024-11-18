using UnityEditor;
using UnityEngine;

namespace Zlitz.Extra2D.BetterTile
{
    [CustomEditor(typeof(TileSet))]
    public class TileSetEditor : Editor
    {
        private static readonly GUIContent s_editButtonLabel = new GUIContent("Edit Tile Set");

        private TileSet m_tileSet;

        private SerializedProperty m_texturesProperty;
        private SerializedProperty m_categoriesProperty;
        private SerializedProperty m_tilesProperty;

        private void OnEnable()
        {
            m_tileSet = serializedObject.targetObject as TileSet;

            m_texturesProperty      = serializedObject.FindProperty("m_textures");
            m_categoriesProperty    = serializedObject.FindProperty("m_categories");
            m_tilesProperty         = serializedObject.FindProperty("m_tiles");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_texturesProperty);
                EditorGUILayout.PropertyField(m_categoriesProperty);
                EditorGUILayout.PropertyField(m_tilesProperty);
            }

            if (GUILayout.Button(s_editButtonLabel))
            {
                TileSetEditorWindow.Open(m_tileSet);
            }
        }
    }
}
