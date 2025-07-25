using System;

using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    public class MouseCapturer : VisualElement
    {
        private bool m_mouseDown;

        public bool mouseDown => m_mouseDown;

        public event Action<int, bool> onMouseDown;
        public event Action<int, bool> onMouseUp;

        public MouseCapturer()
        {
            RegisterCallback<PointerDownEvent>(e =>
            {
                if (!m_mouseDown)
                {
                    this.CapturePointer(PointerId.mousePointerId);
                    m_mouseDown = true;
                    onMouseDown?.Invoke(e.button, e.shiftKey);
                }
            });
            RegisterCallback<PointerUpEvent>(e =>
            {
                if (m_mouseDown)
                {
                    this.ReleasePointer(PointerId.mousePointerId);
                    m_mouseDown = false;
                    onMouseUp?.Invoke(e.button, e.shiftKey);
                }
            });
            RegisterCallback<PointerLeaveEvent>(e => 
            {
                if (m_mouseDown)
                {
                    this.ReleasePointer(PointerId.mousePointerId);
                    m_mouseDown = false;
                    onMouseUp?.Invoke(e.button, e.shiftKey);
                }
            });
        }
    }
}
