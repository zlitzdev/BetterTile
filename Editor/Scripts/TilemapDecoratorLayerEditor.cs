using UnityEditor;
using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    [CustomEditor(typeof(TilemapDecoratorLayer))]
    internal class TilemapDecoratorLayerEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            return new VisualElement();
        }
    }
}
