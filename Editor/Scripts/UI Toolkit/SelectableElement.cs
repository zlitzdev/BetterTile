using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal abstract class SelectableElement : VisualElement
    {
        private SelectionGroup m_group;

        public virtual bool deselectWhenReselect => true;

        public virtual void OnSelected()
        {
        }

        public virtual void OnDeselect()
        {
        }

        protected SelectableElement(SelectionGroup selectionGroup)
        {
            m_group = selectionGroup;

            RegisterCallback<MouseDownEvent>(e =>
            {
                m_group.Select(this);
            });

            RegisterCallback<DetachFromPanelEvent>(e =>
            {
                if (m_group.current == this)
                {
                    m_group.Select(null);
                }
            });
        }
    }
}
