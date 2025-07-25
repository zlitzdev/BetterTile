using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal class AspectRatioPanel : VisualElement
    {
        private float m_aspectRatio = 1.0f; // width : height

        private bool m_lockHeight = false;

        public float aspectRatio
        {
            get => m_aspectRatio;
            set
            {
                if (m_aspectRatio != value)
                {
                    m_aspectRatio = value;
                    UpdateGeometry(contentRect.size);
                }
            }
        }

        public bool lockHeight
        {
            get => m_lockHeight;
            set
            {
                if (m_lockHeight != value)
                {
                    m_lockHeight = value;
                    UpdateGeometry(contentRect.size);
                }
            }
        }

        public AspectRatioPanel()
        {
            RegisterCallback<GeometryChangedEvent>(e =>
            {
                UpdateGeometry(e.newRect.size);
            });
        }

        private void UpdateGeometry(Vector2 size)
        {
            if (!m_lockHeight)
            {
                float forcedHeight = size.x / m_aspectRatio;
                if (size.y != forcedHeight)
                {
                    EditorApplication.delayCall += () => style.height = forcedHeight;
                }
            }
            else
            {
                float forcedWidth = size.y * m_aspectRatio;
                if (size.x != forcedWidth)
                {
                    EditorApplication.delayCall += () => style.width = forcedWidth;
                }
            }
        }
    }
}
