using UnityEngine;
using UnityEditor;

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
        private SerializedProperty m_selfProperty;
        private SerializedProperty m_decoratorProperty;
        private SerializedProperty m_spriteEntriesProperty;

        private void OnEnable()
        {
            m_tileSet = serializedObject.targetObject as TileSet;

            m_texturesProperty      = serializedObject.FindProperty("m_textures");
            m_categoriesProperty    = serializedObject.FindProperty("m_categories");
            m_tilesProperty         = serializedObject.FindProperty("m_tiles");
            m_selfProperty          = serializedObject.FindProperty("m_selfFilter");
            m_decoratorProperty     = serializedObject.FindProperty("m_decorator");
            m_spriteEntriesProperty = serializedObject.FindProperty("m_spriteEntries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Read only list of textures
            DrawTextures();

            // Read only list of category, each one showing its content
            DrawCategories();

            // Read only list of tiles
            DrawTiles();

            // Read only list of other elements
            DrawOthers();

            // Read only list of detected sprites
            DrawSprites();

            // Edit button to open the Editor Window
            DrawEditButton();
        }

        private void DrawTextures()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_texturesProperty);
            }
        }

        private void DrawCategories()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_categoriesProperty);
            }
        }

        private void DrawTiles()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_tilesProperty);
            }
        }

        private void DrawOthers()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_selfProperty);
                EditorGUILayout.PropertyField(m_decoratorProperty);
            }
        }

        private void DrawSprites()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_spriteEntriesProperty);
            }
        }

        private void DrawEditButton()
        {
            if (GUILayout.Button(s_editButtonLabel))
            {
                TileSetEditorWindow.Open(m_tileSet);
            }
        }
    }
}
