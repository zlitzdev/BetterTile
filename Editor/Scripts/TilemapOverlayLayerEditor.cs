using UnityEditor;
using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    [CustomEditor(typeof(TilemapOverlayLayer))]
    internal class TilemapOverlayLayerEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            return new VisualElement();
        }
    }
}
