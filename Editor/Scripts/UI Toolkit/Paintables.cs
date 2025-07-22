using System;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal abstract class Paintable : VisualElement
    {
        private TileSetEditorWindow.PaintContext m_context;

        private bool m_enabled;
        private bool m_hover;
        private bool m_inverted;

        protected TileSetEditorWindow.PaintContext context => m_context;

        protected bool enabled => m_enabled;

        protected abstract bool CheckBrush(TileSetEditorWindow.Brush brush);

        protected abstract void OnPainted(TileSetEditorWindow.Brush brush, bool inverted);

        protected abstract void OnMouseEnter();

        protected abstract void OnMouseLeave();

        protected abstract void OnPaintingEnabled();

        protected abstract void OnPaintingDisabled();

        protected Paintable(TileSetEditorWindow.PaintContext context)
        {
            m_context = context;

            m_context.onBrushChanged += OnBrushChanged;
            m_context.onPaintStarted += OnPaintStarted;
            RegisterCallback<DetachFromPanelEvent>(e =>
            {
                m_context.onBrushChanged -= OnBrushChanged;
                m_context.onPaintStarted -= OnPaintStarted;
            });

            RegisterCallback<MouseEnterEvent>(e =>
            {
                if (!m_hover)
                {
                    OnMouseEnter();
                    if (context.active && enabled)
                    {
                        OnPainted(context.currentBrush, m_inverted);
                    }
                    m_hover = true;
                }
            });

            RegisterCallback<MouseLeaveEvent>(e =>
            {
                if (m_hover)
                {
                    OnMouseLeave();
                    m_hover = false;
                }
            });

            EditorApplication.delayCall += () => OnBrushChanged(context.currentBrush);
        }

        private void OnBrushChanged(TileSetEditorWindow.Brush brush)
        {
            bool validBrush = CheckBrush(brush);
            if (m_enabled != validBrush)
            {
                if (!m_enabled)
                {
                    OnPaintingEnabled();
                }
                else
                {
                    OnPaintingDisabled();
                }
                m_enabled = validBrush;
            }
        }

        private void OnPaintStarted(TileSetEditorWindow.Brush brush, bool inverted)
        {
            m_inverted = inverted;
            if (m_hover && m_enabled)
            {
                OnPainted(brush, m_inverted);
            }
        }
    }

    internal abstract class BasePaintable : Paintable
    {
        private VisualElement m_overlay;

        protected override void OnMouseEnter()
        {
            m_overlay.style.backgroundColor = UIColors.brighten;

            style.borderLeftColor = UIColors.border;
            style.borderRightColor = UIColors.border;
            style.borderTopColor = UIColors.border;
            style.borderBottomColor = UIColors.border;
        }

        protected override void OnMouseLeave()
        {
            m_overlay.style.backgroundColor = UIColors.transparent;

            style.borderLeftColor = UIColors.brighten;
            style.borderRightColor = UIColors.brighten;
            style.borderTopColor = UIColors.brighten;
            style.borderBottomColor = UIColors.brighten;
        }

        protected override void OnPaintingEnabled()
        {
            style.borderLeftWidth = 1.0f;
            style.borderRightWidth = 1.0f;
            style.borderTopWidth = 1.0f;
            style.borderBottomWidth = 1.0f;

            if (m_overlay != null)
            {
                m_overlay.style.display = DisplayStyle.Flex;
            }
        }

        protected override void OnPaintingDisabled()
        {
            style.borderLeftWidth = 0.0f;
            style.borderRightWidth = 0.0f;
            style.borderTopWidth = 0.0f;
            style.borderBottomWidth = 0.0f;

            if (m_overlay != null)
            {
                m_overlay.style.display = DisplayStyle.None;
            }
        }

        protected BasePaintable(TileSetEditorWindow.PaintContext context) :
            base(context)
        {
            style.borderLeftColor = UIColors.brighten;
            style.borderRightColor = UIColors.brighten;
            style.borderTopColor = UIColors.brighten;
            style.borderBottomColor = UIColors.brighten;

            m_overlay = new VisualElement();
            m_overlay.style.position = Position.Absolute;
            m_overlay.style.left = 0.0f;
            m_overlay.style.right = 0.0f;
            m_overlay.style.top = 0.0f;
            m_overlay.style.bottom = 0.0f;
            Add(m_overlay);
        }
    }

    internal class TileIdentityPaintable : BasePaintable
    {
        private SerializedTileIdentity m_serializedIdentity;
        private Action m_onChanges;

        protected override bool CheckBrush(TileSetEditorWindow.Brush brush)
        {
            if (brush == null || brush is not TileSetEditorWindow.TileFilterBrush tileFilterBrush)
            {
                return false;
            }

            if (tileFilterBrush.filterType == TileFilterType.Tile && tileFilterBrush.tile != null)
            {
                return true;
            }

            return tileFilterBrush.filterType != TileFilterType.Tile && tileFilterBrush.filterType != TileFilterType.TileCategory;
        }

        protected override void OnPainted(TileSetEditorWindow.Brush brush, bool inverted)
        {
            if (m_serializedIdentity == null || brush is not TileSetEditorWindow.TileFilterBrush tileFilterBrush)
            {
                return;
            }

            if (tileFilterBrush.filterType == TileFilterType.Tile)
            {
                if (m_serializedIdentity.SetTile(tileFilterBrush.tile))
                {
                    style.backgroundColor = context.GetSerializedTile(tileFilterBrush.tile).tileColor.WithAlpha(0.3f);
                    m_onChanges?.Invoke();
                }
                return;
            }

            if (tileFilterBrush.filterType != TileFilterType.TileCategory)
            {
                if (m_serializedIdentity.SetSpecialFilter(tileFilterBrush.filterType))
                {
                    style.backgroundColor = tileFilterBrush.filterType == TileFilterType.Any
                        ? UIColors.transparent
                        : context.GetSerializedSpecialFilter(tileFilterBrush.filterType).filterColor.WithAlpha(0.3f);
                    m_onChanges?.Invoke();
                }
                return;
            }
        }

        public void Bind(SerializedTileIdentity serializedIdentity, Action onChanges)
        {
            m_serializedIdentity = serializedIdentity;
            m_onChanges = onChanges;

            if (serializedIdentity.type == TileIdentityType.None)
            {
                style.backgroundColor = UIColors.transparent;
            }
            else if (serializedIdentity.type == TileIdentityType.Tile)
            {
                style.backgroundColor = context.GetSerializedTile(serializedIdentity.tile).tileColor.WithAlpha(0.3f);
            }
            else if (serializedIdentity.type == TileIdentityType.Other)
            {
                style.backgroundColor = serializedIdentity.filterType == TileFilterType.Any
                    ? UIColors.transparent
                    : context.GetSerializedSpecialFilter(serializedIdentity.filterType).filterColor.WithAlpha(0.3f);
            }
        }

        public void OnTileColorChanged(SerializedTile tile)
        {
            if (m_serializedIdentity.type == TileIdentityType.Tile && m_serializedIdentity.tile == tile.tile)
            {
                style.backgroundColor = tile.tileColor.WithAlpha(0.3f);
            }
        }

        public void OnFilterColorChanged(SerializedSpecialFilter filter)
        {
            if (m_serializedIdentity.type == TileIdentityType.Other && m_serializedIdentity.filterType == filter.filterType)
            {
                style.backgroundColor = filter.filterColor.WithAlpha(0.3f);
            }
        }

        public TileIdentityPaintable(TileSetEditorWindow.PaintContext context) :
            base(context)
        {
        }
    }

    internal class TileFilterPaintable : BasePaintable
    {
        private SerializedTileFilter m_serializedFilter;
        private Action m_onChanges;

        private VisualElement m_cross;

        private bool m_allowInverted = true;

        public bool allowInverted
        {
            get => m_allowInverted;
            set
            {
                m_allowInverted = value;
                if (!m_allowInverted)
                {
                    if (m_serializedFilter != null && m_serializedFilter.inverted)
                    {
                        m_serializedFilter.inverted = false;
                        m_onChanges?.Invoke();
                    }
                }
            }
        }

        protected override bool CheckBrush(TileSetEditorWindow.Brush brush)
        {
            if (brush == null || brush is not TileSetEditorWindow.TileFilterBrush tileFilterBrush)
            {
                return false;
            }

            if (tileFilterBrush.filterType == TileFilterType.Tile && tileFilterBrush.tile != null)
            {
                return true;
            }

            if (tileFilterBrush.filterType == TileFilterType.TileCategory && tileFilterBrush.category != null)
            {
                return true;
            }

            return tileFilterBrush.filterType != TileFilterType.Tile && tileFilterBrush.filterType != TileFilterType.TileCategory;
        }

        protected override void OnPainted(TileSetEditorWindow.Brush brush, bool inverted)
        {
            if (m_serializedFilter == null || brush is not TileSetEditorWindow.TileFilterBrush tileFilterBrush)
            {
                return;
            }

            if (!m_allowInverted)
            {
                inverted = false;
            }

            if (tileFilterBrush.filterType == TileFilterType.Tile)
            {
                if (m_serializedFilter.SetTile(tileFilterBrush.tile, inverted))
                {
                    style.backgroundColor = context.GetSerializedTile(tileFilterBrush.tile).tileColor.WithAlpha(0.3f);
                    m_cross.style.display = m_serializedFilter.inverted ? DisplayStyle.Flex : DisplayStyle.None;
                    m_onChanges?.Invoke();
                }
                return;
            }

            if (tileFilterBrush.filterType == TileFilterType.TileCategory)
            {
                if (m_serializedFilter.SetCategory(tileFilterBrush.category, inverted))
                {
                    style.backgroundColor = context.GetSerializedCategory(tileFilterBrush.category).categoryColor.WithAlpha(0.3f);
                    m_cross.style.display = m_serializedFilter.inverted ? DisplayStyle.Flex : DisplayStyle.None;
                    m_onChanges?.Invoke();
                }
                return;
            }

            if (m_serializedFilter.SetSpecialFilter(tileFilterBrush.filterType, inverted))
            {
                style.backgroundColor = tileFilterBrush.filterType == TileFilterType.Any
                    ? UIColors.transparent
                    : context.GetSerializedSpecialFilter(tileFilterBrush.filterType).filterColor.WithAlpha(0.3f);
                m_cross.style.display = m_serializedFilter.inverted ? DisplayStyle.Flex : DisplayStyle.None;
                m_onChanges?.Invoke();
            }

            return;
        }

        public void Bind(SerializedTileFilter serializedFilter, Action onChanges)
        {
            m_serializedFilter = serializedFilter;
            m_onChanges = onChanges;

            if (!m_allowInverted && m_serializedFilter.inverted)
            {
                m_serializedFilter.inverted = false;
                onChanges?.Invoke();
            }

            if (m_serializedFilter.type == TileFilterType.Tile)
            {
                style.backgroundColor = context.GetSerializedTile(m_serializedFilter.tile).tileColor.WithAlpha(0.3f);
            }
            else if (m_serializedFilter.type == TileFilterType.TileCategory)
            {
                style.backgroundColor = context.GetSerializedCategory(m_serializedFilter.category).categoryColor.WithAlpha(0.3f);
            }
            else
            {
                style.backgroundColor = m_serializedFilter.type == TileFilterType.Any
                    ? UIColors.transparent
                    : context.GetSerializedSpecialFilter(m_serializedFilter.type).filterColor.WithAlpha(0.3f);
            }

            m_cross.style.display = m_serializedFilter.inverted ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void OnTileColorChanged(SerializedTile tile)
        {
            if (m_serializedFilter.type == TileFilterType.Tile && m_serializedFilter.tile == tile.tile)
            {
                style.backgroundColor = tile.tileColor.WithAlpha(0.3f);
            }
        }

        public void OnCategoryColorChanged(SerializedCategory category)
        {
            if (m_serializedFilter.type == TileFilterType.TileCategory && m_serializedFilter.category == category.category)
            {
                style.backgroundColor = category.categoryColor.WithAlpha(0.3f);
            }
        }

        public void OnFilterColorChanged(SerializedSpecialFilter filter)
        {
            if (m_serializedFilter.type == filter.filterType)
            {
                style.backgroundColor = filter.filterColor.WithAlpha(0.3f);
            }
        }

        public TileFilterPaintable(TileSetEditorWindow.PaintContext context) :
            base(context)
        {
            m_cross = new Cross(Color.red);
            m_cross.style.display = DisplayStyle.None;
            m_cross.style.position = Position.Absolute;
            m_cross.style.left = 0.0f;
            m_cross.style.right = 0.0f;
            m_cross.style.top = 0.0f;
            m_cross.style.bottom = 0.0f;
            Add(m_cross);
        }

        private class Cross : IMGUIContainer
        {
            public Cross(Color color)
            {
                onGUIHandler = () =>
                {
                    Rect rect = contentRect;

                    Color handleColor = Handles.color;

                    Handles.BeginGUI();
                    Handles.color = color;

                    Handles.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMax, rect.yMax));
                    Handles.DrawLine(new Vector3(rect.xMin, rect.yMax), new Vector3(rect.xMax, rect.yMin));

                    Handles.color = handleColor;
                    Handles.EndGUI();
                };
            }
        }
    }

    internal class WeightPaintable : BasePaintable
    {
        private SerializedTileRule m_serializedTileRule;
        private Action m_onChanges;

        private Label m_weight;

        protected override void OnPaintingEnabled()
        {
            pickingMode = PickingMode.Position;
            style.display = DisplayStyle.Flex;
            base.OnPaintingEnabled();
        }

        protected override void OnPaintingDisabled()
        {
            pickingMode = PickingMode.Ignore;
            style.display = DisplayStyle.None;
            base.OnPaintingDisabled();
        }

        protected override bool CheckBrush(TileSetEditorWindow.Brush brush)
        {
            if (brush == null || brush is not TileSetEditorWindow.WeightBrush weightBrush)
            {
                return false;
            }

            return true;
        }

        protected override void OnPainted(TileSetEditorWindow.Brush brush, bool inverted)
        {
            if (m_serializedTileRule == null || brush is not TileSetEditorWindow.WeightBrush weightBrush)
            {
                return;
            }

            if (m_serializedTileRule.weight != weightBrush.weight)
            {
                m_serializedTileRule.weight = weightBrush.weight;
                m_onChanges?.Invoke();
            }
            m_weight.text = m_serializedTileRule.weight.ToString("0.#");
        }

        public void Bind(SerializedTileRule serializedTileRule, Action onChanges)
        {
            m_serializedTileRule = serializedTileRule;
            m_onChanges = onChanges;

            m_weight.text = m_serializedTileRule.weight.ToString("0.#");
        }

        public WeightPaintable(TileSetEditorWindow.PaintContext context) :
            base(context)
        {
            pickingMode = PickingMode.Ignore;
            style.display = DisplayStyle.None;

            m_weight = new Label();
            m_weight.style.flexGrow = 1.0f;
            m_weight.style.unityTextAlign = TextAnchor.MiddleCenter;
            m_weight.style.fontSize = 24.0f;
            m_weight.style.unityFontStyleAndWeight = FontStyle.Bold;
            m_weight.style.textShadow = new TextShadow()
            {
                offset = new Vector2(2.0f, 2.0f),
                blurRadius = 2.0f
            };
            Add(m_weight);
        }
    }
}
