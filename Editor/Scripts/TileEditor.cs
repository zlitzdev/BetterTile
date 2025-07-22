using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    [CustomEditor(typeof(Tile))]
    public class TileEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            SerializedProperty tileSetProperty = serializedObject.FindProperty("m_tileSet");
            PropertyField tileSetField = new PropertyField(tileSetProperty);
            tileSetField.SetEnabled(false);
            root.Add(tileSetField);

            SerializedProperty colliderTypeProperty = serializedObject.FindProperty("m_colliderType");
            PropertyField colliderTypeField = new PropertyField(colliderTypeProperty);
            colliderTypeField.SetEnabled(false);
            root.Add(colliderTypeField);

            Button openEditorButton = new Button(() =>
            {
                TileSet tileSet = serializedObject.FindProperty("m_tileSet").objectReferenceValue as TileSet;
                if (tileSet != null)
                {
                    TileSetEditorWindow.Open(tileSet);
                }
            });
            openEditorButton.text = "Open Editor";
            root.Add(openEditorButton);

            return root;
        }
    }
}
