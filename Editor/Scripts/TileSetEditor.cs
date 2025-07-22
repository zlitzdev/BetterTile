using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    //[CustomEditor(typeof(TileSet))]
    public class TileSetEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            SerializedProperty tilesProperty = serializedObject.FindProperty("m_tiles");
            PropertyField tilesField = new PropertyField(tilesProperty);
            tilesField.SetEnabled(false);
            root.Add(tilesField);

            Button openEditorButton = new Button(() =>
            {
                TileSetEditorWindow.Open(target as TileSet);
            });
            openEditorButton.text = "Open Editor";
            root.Add(openEditorButton);

            return root;
        }
    }
}
