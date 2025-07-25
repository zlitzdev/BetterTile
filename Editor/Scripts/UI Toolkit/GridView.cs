using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal class GridView : VisualElement
    {
        private float m_aspectRatio; // width / height

        private Vector2Int m_currentSize;
        private float m_itemSize;

        private List<KeyValuePair<VisualElement, VisualElement>> m_children = new List<KeyValuePair<VisualElement, VisualElement>>();

        public float itemSize => m_itemSize;

        public Vector2 size => m_currentSize * size;

        public void InsertChild(VisualElement childElement, int index = -1)
        {
            if (index < 0)
            {
                index = m_children.Count;
            }

            VisualElement container = new VisualElement();
            container.style.position = Position.Absolute;
            container.style.width  = m_itemSize;
            container.style.height = m_itemSize;
            Add(container);
            container.Add(childElement);
            m_children.Insert(index, new KeyValuePair<VisualElement, VisualElement>(container, childElement));

            UpdateSize();
            UpdateChildrenPosition();
        }

        public void RemoveChild(VisualElement childElement)
        {
            for (int i = 0; i < m_children.Count; i++)
            {
                if (m_children[i].Value == childElement)
                {
                    VisualElement item = m_children[i].Key;

                    m_children.RemoveAt(i);
                    item.RemoveFromHierarchy();

                    UpdateSize();
                    UpdateChildrenPosition();

                    return;
                }
            }
        }

        public void RemoveChild(int index)
        {
            VisualElement item = m_children[index].Key;

            m_children.RemoveAt(index);
            item.RemoveFromHierarchy();

            UpdateSize();
            UpdateChildrenPosition();
        }

        public void ClearChildren()
        {
            Clear();
            m_children.Clear();
            UpdateSize();
        }

        public VisualElement GetChild(int index)
        {
            return m_children[index].Value;
        }

        public (int, int) ItemIndexToGridPosition(int index)
        {
            return (
                index % m_currentSize.x,
                index / m_currentSize.x
            );
        }

        public GridView(float aspectRatio)
        {
            m_aspectRatio = aspectRatio;

            RegisterCallback<GeometryChangedEvent>(e =>
            {
                m_itemSize = m_currentSize.x == 0 ? 0.0f : e.newRect.width / m_currentSize.x;
                style.height = m_itemSize * m_currentSize.y;

                UpdateChildrenPosition();
            });
        }

        private void UpdateSize()
        {
            int childrenCount = m_children.Count;

            int height = 0;
            while (height * (int)(height * m_aspectRatio) < childrenCount)
            {
                height++;
            }

            int width = (int)(height * m_aspectRatio);
            height = width == 0 ? 0 : childrenCount / width;
            if (height * width < childrenCount)
            {
                height++;
            }

            m_currentSize = new Vector2Int(width, height);

            m_itemSize = m_currentSize.x == 0 ? 0.0f : contentRect.width / m_currentSize.x;
            style.height = m_itemSize * m_currentSize.y;
        }

        private void UpdateChildrenPosition()
        {
            for (int i = 0; i < m_children.Count; i++)
            {
                VisualElement item = m_children[i].Key;

                (int x, int y) = ItemIndexToGridPosition(i);

                item.style.left   = m_itemSize * x;
                item.style.top    = m_itemSize * y;
                item.style.width  = m_itemSize;
                item.style.height = m_itemSize;
            }
        }
    }
}
