using UnityEditor;

namespace Zlitz.Extra2D.BetterTile
{
    //[CustomEditor(typeof(Tile))]
    public class TileEditor : Editor
    {
        private SerializedProperty m_colliderTypeProperty;
        private SerializedProperty m_baseTileProperty;
        private SerializedProperty m_overwriteRulesProperty;

        private void OnEnable()
        {
            m_colliderTypeProperty   = serializedObject.FindProperty("m_colliderType");
            m_baseTileProperty       = serializedObject.FindProperty("m_baseTile");
            m_overwriteRulesProperty = serializedObject.FindProperty("m_overwriteRules");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_colliderTypeProperty);
            EditorGUILayout.PropertyField(m_baseTileProperty);
            EditorGUILayout.PropertyField(m_overwriteRulesProperty);
        }
    }
}
