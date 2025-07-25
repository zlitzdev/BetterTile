using System;

using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal class SelectionGroup : VisualElement
    {
        public event Action<SelectableElement> onCurrentElementChanged;

        private SelectableElement m_current;

        public SelectableElement current => m_current;

        public void Select(SelectableElement element)
        {
            if (m_current != element)
            {
                m_current?.OnDeselect();
                m_current = element;
                m_current?.OnSelected();
                onCurrentElementChanged?.Invoke(m_current);
            }
            else if (m_current != null && m_current.deselectWhenReselect)
            {
                m_current.OnDeselect();
                m_current = null;
                onCurrentElementChanged?.Invoke(m_current);
            }
        }
    }
}
