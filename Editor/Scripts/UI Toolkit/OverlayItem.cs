using System;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal class OverlayItem : VisualElement
    {
        private TileSetEditorWindow.PaintContext m_context;

        private SerializedOverlay m_serializedOverlay;

        private Vector2IntField m_size;
        private Vector2IntField m_cellSize;
        private Vector2IntField m_spacing;
        private FloatField m_density;
        private AspectRatioPanel m_patternContainer;

        private Action<SerializedTile>          m_onTileColorChanged;
        private Action<SerializedCategory>      m_onCategoryColorChanged;
        private Action<SerializedSpecialFilter> m_onFilterColorChanged;

        private OverlayRulePaintable m_hoveringRule;
        private OverlayRulePaintable m_activeRule;

        private Action m_onChanges;
        private Action m_onUpdate;

        public event Action<Vector2Int> onSizeChanged;

        private Sprite m_selectedSprite;

        public void Update()
        {
            m_onUpdate?.Invoke();
        }

        public void Bind(SerializedOverlay serializedOverlay, Action onChanges, Action updateSize)
        {
            bool changed = serializedOverlay != m_serializedOverlay;
            
            m_onChanges = onChanges;

            m_serializedOverlay = serializedOverlay;

            m_size.value = m_serializedOverlay.size;
            m_cellSize.value = m_serializedOverlay.cellSize;
            m_spacing.value = m_serializedOverlay.spacing;
            m_density.value = m_serializedOverlay.density;

            m_patternContainer.aspectRatio = m_serializedOverlay.size.y == 0.0f ? 0.0f : (float)m_serializedOverlay.size.x / m_serializedOverlay.size.y;
            onSizeChanged?.Invoke(m_serializedOverlay.size);

            EditorApplication.delayCall += () => 
            {
                updateSize?.Invoke();
            };

            if (changed)
            {
                PopulateRules();
            }
        }

        public void OnZoomChanged(float zoom, int maxPatternWidth)
        {
            if (m_serializedOverlay == null || m_patternContainer == null)
            {
                return;
            }

            float r = maxPatternWidth == 0 ? 0.0f : (float)m_serializedOverlay.size.x / maxPatternWidth;
            m_patternContainer.style.width = Length.Percent(zoom * r * 100.0f);
        }

        public void OnTileColorChanged(SerializedTile tile)
        {
            m_onTileColorChanged?.Invoke(tile);
        }

        public void OnCategoryColorChanged(SerializedCategory category)
        {
            m_onCategoryColorChanged?.Invoke(category);
        }

        public void OnFilterColorChanged(SerializedSpecialFilter filter)
        {
            m_onFilterColorChanged?.Invoke(filter);
        }

        public OverlayItem(TileSetEditorWindow.PaintContext context)
        {
            m_context = context;

            style.backgroundColor = UIColors.gray;
            style.borderLeftWidth   = 1.0f;
            style.borderRightWidth  = 1.0f;
            style.borderTopWidth    = 1.0f;
            style.borderBottomWidth = 1.0f;
            style.borderLeftColor   = UIColors.brighten;
            style.borderRightColor  = UIColors.brighten;
            style.borderTopColor    = UIColors.brighten;
            style.borderBottomColor = UIColors.brighten;
            style.paddingLeft   = 6.0f;
            style.paddingRight  = 6.0f;
            style.paddingTop    = 6.0f;
            style.paddingBottom = 6.0f;

            Vector2IntField size = new Vector2IntField();
            size.label = "Size";
            size.style.width = 360.0f;
            size.RegisterValueChangedCallback(e =>
            {
                bool changed = false;

                Vector2Int newSize = e.newValue;
                newSize.x = Mathf.Max(1, newSize.x);
                newSize.y = Mathf.Max(1, newSize.y);
                size.SetValueWithoutNotify(newSize);

                if (m_serializedOverlay.size != newSize) 
                {
                    m_serializedOverlay.size = newSize;
                    changed = true;

                    m_patternContainer.aspectRatio = m_serializedOverlay.size.y == 0.0f ? 0.0f : (float)m_serializedOverlay.size.x / m_serializedOverlay.size.y;
                    onSizeChanged?.Invoke(m_serializedOverlay.size);
                }

                Vector2Int newCellSize = m_serializedOverlay.cellSize;
                newCellSize.x = Mathf.Max(m_serializedOverlay.size.x, newCellSize.x);
                newCellSize.y = Mathf.Max(m_serializedOverlay.size.y, newCellSize.y);
                m_cellSize.SetValueWithoutNotify(newCellSize);

                if (m_serializedOverlay.cellSize != newCellSize)
                {
                    m_serializedOverlay.cellSize = newCellSize;
                    changed = true;
                }

                if (changed)
                {
                    m_onChanges?.Invoke();
                    PopulateRules();
                }
            });
            Add(size);
            m_size = size;

            Vector2IntField cellSize = new Vector2IntField();
            cellSize.label = "Cell Size";
            cellSize.style.width = 360.0f;
            cellSize.RegisterValueChangedCallback(e =>
            {
                Vector2Int newCellSize = e.newValue;
                newCellSize.x = Mathf.Max(m_serializedOverlay.size.x, newCellSize.x);
                newCellSize.y = Mathf.Max(m_serializedOverlay.size.y, newCellSize.y);
                cellSize.SetValueWithoutNotify(newCellSize);

                if (m_serializedOverlay.cellSize != newCellSize)
                {
                    m_serializedOverlay.cellSize = newCellSize;
                    m_onChanges?.Invoke();
                }
            });
            Add(cellSize);
            m_cellSize = cellSize;

            Vector2IntField spacing = new Vector2IntField();
            spacing.label = "Spacing";
            spacing.style.width = 360.0f;
            spacing.RegisterValueChangedCallback(e =>
            {
                Vector2Int newSpacing = e.newValue;
                newSpacing.x = Mathf.Max(0, newSpacing.x);
                newSpacing.y = Mathf.Max(0, newSpacing.y);
                spacing.SetValueWithoutNotify(newSpacing);

                if (m_serializedOverlay.spacing != newSpacing)
                {
                    m_serializedOverlay.spacing = newSpacing;
                    m_onChanges?.Invoke();
                }
            });
            Add(spacing);
            m_spacing = spacing;

            FloatField density = new FloatField();
            density.label = "Density";
            density.style.width = 360.0f;
            density.RegisterValueChangedCallback(e =>
            {
                float newDensity = Mathf.Clamp01(e.newValue);
                density.SetValueWithoutNotify(newDensity);

                if (m_serializedOverlay.density != newDensity)
                {
                    m_serializedOverlay.density = newDensity;
                    m_onChanges?.Invoke();
                }
            });
            Add(density);
            m_density = density;

            MouseCapturer body = new MouseCapturer();
            body.style.flexDirection = FlexDirection.Row;
            body.style.flexGrow = 1.0f;
            body.style.marginTop = 4.0f;
            body.onMouseDown += (b, e) =>
            {
                if (b == 0)
                {
                    m_context.inverted = e;
                    m_context.active = true;
                }
                else if (b == 1)
                {
                    if (m_hoveringRule != null)
                    {
                        OverlayRulePaintable hoveringRule = m_hoveringRule;
                        m_activeRule = hoveringRule;

                        GenericMenu menu = new GenericMenu();

                        menu.AddItem(new GUIContent("Set Sprite"), false, () =>
                        {
                            EditorGUIUtility.ShowObjectPicker<Sprite>(null, false, "", 0);
                            m_onUpdate += WaitForObjectPicker;
                        });

                        menu.AddItem(new GUIContent("Animate"), false, () =>
                        {
                            TileAnimationPopupWindow.Open(EditorGUIUtility.GetMainWindowPosition().center, hoveringRule.serializedRule.output, () =>
                            {
                                m_onChanges?.Invoke();
                            });
                        });

                        menu.ShowAsContext();
                    }
                }
            };
            body.onMouseUp += (b, e) =>
            {
                m_context.active = false;
            };
            Add(body);

            AspectRatioPanel patternContainer = new AspectRatioPanel();
            patternContainer.style.backgroundColor = UIColors.brighten;
            patternContainer.style.borderLeftWidth   = 1.0f;
            patternContainer.style.borderRightWidth  = 1.0f;
            patternContainer.style.borderTopWidth    = 1.0f;
            patternContainer.style.borderBottomWidth = 1.0f;
            patternContainer.style.borderLeftColor   = UIColors.border;
            patternContainer.style.borderRightColor  = UIColors.border;
            patternContainer.style.borderTopColor    = UIColors.border;
            patternContainer.style.borderBottomColor = UIColors.border;
            body.Add(patternContainer);
            m_patternContainer = patternContainer;
        }
    
        private void PopulateRules()
        {
            m_patternContainer.Clear();

            Vector2 ruleSize = new Vector2(
                100.0f / m_serializedOverlay.size.x,
                100.0f / m_serializedOverlay.size.y
            );
            
            for (int x = 0; x < m_serializedOverlay.size.x; x++)
            {
                for (int y = 0; y < m_serializedOverlay.size.y; y++)
                {
                    int index = y * m_serializedOverlay.size.x + x;

                    while (index >= m_serializedOverlay.rules.Count)
                    {
                        m_serializedOverlay.rules.Add(new SerializedOverlayRule());
                    }

                    SerializedOverlayRule rule = m_serializedOverlay.rules[index];
                    if (rule == null)
                    {
                        rule = new SerializedOverlayRule();
                        m_serializedOverlay.rules[index] = rule;
                    }

                    OverlayRulePaintable rulePaintable = new OverlayRulePaintable(m_context);
                    rulePaintable.style.position = Position.Absolute;
                    rulePaintable.style.left   = Length.Percent(x * ruleSize.x);
                    rulePaintable.style.bottom = Length.Percent(y * ruleSize.y);
                    rulePaintable.style.width  = Length.Percent(ruleSize.x);
                    rulePaintable.style.height = Length.Percent(ruleSize.y);
                    m_patternContainer.Add(rulePaintable);
                    
                    rulePaintable.Bind(rule, m_onChanges);

                    m_onTileColorChanged     += rulePaintable.OnTileColorChanged;
                    m_onCategoryColorChanged += rulePaintable.OnCategoryColorChanged;
                    m_onFilterColorChanged   += rulePaintable.OnFilterColorChanged;

                    rulePaintable.RegisterCallback<MouseEnterEvent>(e =>
                    {
                        m_hoveringRule = rulePaintable;
                        m_hoveringRule.highlight.style.backgroundColor = UIColors.lightBlue;
                    });
                    rulePaintable.RegisterCallback<MouseLeaveEvent>(e =>
                    {
                        if (m_hoveringRule == rulePaintable)
                        {
                            m_hoveringRule.highlight.style.backgroundColor = UIColors.transparent;
                            m_hoveringRule = null;
                        }
                    });
                    rulePaintable.RegisterCallback<DetachFromPanelEvent>(e =>
                    {
                        if (m_hoveringRule == rulePaintable)
                        {
                            m_hoveringRule.highlight.style.backgroundColor = UIColors.transparent;
                            m_hoveringRule = null;
                        }
                    });
                }
            }
        }
    
        private void WaitForObjectPicker()
        {
            if (Event.current == null || Event.current.type != EventType.ExecuteCommand)
            {
                return;
            }

            if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                m_selectedSprite = EditorGUIUtility.GetObjectPickerObject() as Sprite;
            }

            if (Event.current.commandName == "ObjectSelectorClosed")
            {
                if (m_activeRule.serializedRule.output.sprite != m_selectedSprite)
                {
                    m_activeRule.serializedRule.output.sprite = m_selectedSprite;
                    m_activeRule.SetBackground(m_selectedSprite);
                    m_onChanges?.Invoke();
                }

                m_activeRule = null;

                m_selectedSprite = null;
                m_onUpdate -= WaitForObjectPicker;
            }
        }
    }
}
