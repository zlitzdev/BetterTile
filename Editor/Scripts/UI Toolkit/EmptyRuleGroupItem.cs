using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal class EmptyRuleGroupItem : VisualElement
    {
        private TileSetEditorWindow.PaintContext m_context;

        private List<SerializedTileRule> m_serializedRules;

        private string m_expandCacheKey;
        private string m_zoomCacheKey;

        private Button m_showButton;
        private Slider m_zoom;
        private MouseCapturer m_body;
        private GridView m_itemContainer;

        private TileRulePaintable m_hoveringRule;

        private Action<SerializedTile>          m_onTileColorChanged;
        private Action<SerializedCategory>      m_onCategoryColorChanged;
        private Action<SerializedSpecialFilter> m_onFilterColorChanged;

        private Action m_onChanges;

        public void Bind(TileSet tileSet, List<SerializedTileRule> serializedRules)
        {
            m_serializedRules = serializedRules;

            m_onTileColorChanged     = null;
            m_onCategoryColorChanged = null;
            m_onFilterColorChanged   = null;

            m_expandCacheKey = $"Zlitz.Extra2D.BetterTile.EmptyRuleGroupItem_{tileSet.id}.expanded";
            bool expanded = EditorPrefs.GetBool(m_expandCacheKey, true);
            EditorPrefs.SetBool(m_expandCacheKey, expanded);

            m_zoomCacheKey = $"Zlitz.Extra2D.BetterTile.EmptyRuleGroupItem_{tileSet.id}.zoom";
            float zoom = EditorPrefs.GetFloat(m_zoomCacheKey, 0.6f);
            EditorPrefs.SetFloat(m_zoomCacheKey, zoom);

            m_showButton.text = expanded ? "Hide" : "Show";
            m_zoom.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
            m_body.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;

            m_zoom.value = zoom;

            m_itemContainer.style.width = Length.Percent(zoom * 100.0f);

            CreateTileRuleEditors(m_onChanges);
        }

        public EmptyRuleGroupItem(TileSetEditorWindow.PaintContext context, Action onChanges)
        {
            m_context = context;

            m_onChanges = onChanges;

            context.onTileColorChanged     += OnTileColorChanged;
            context.onCategoryColorChanged += OnCategoryColorChanged;
            context.onFilterColorChanged   += OnFilterColorChanged;

            RegisterCallback<DetachFromPanelEvent>(e =>
            {
                context.onTileColorChanged     -= OnTileColorChanged;
                context.onCategoryColorChanged -= OnCategoryColorChanged;
                context.onFilterColorChanged   -= OnFilterColorChanged;
            });

            VisualElement root = new VisualElement();
            root.style.marginLeft   = 0.0f;
            root.style.marginRight  = 0.0f;
            root.style.marginTop    = 0.0f;
            root.style.marginBottom = 0.0f;
            root.style.backgroundColor = UIColors.gray;
            root.style.borderLeftWidth   = 1.0f;
            root.style.borderRightWidth  = 1.0f;
            root.style.borderTopWidth    = 1.0f;
            root.style.borderBottomWidth = 1.0f;
            root.style.borderLeftColor   = UIColors.brighten;
            root.style.borderRightColor  = UIColors.brighten;
            root.style.borderTopColor    = UIColors.brighten;
            root.style.borderBottomColor = UIColors.brighten;
            Add(root);

            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.backgroundColor = UIColors.brighten;
            header.style.borderBottomColor = UIColors.border;
            header.style.borderBottomWidth = 2.0f;
            header.style.paddingTop    = 2.0f;
            header.style.paddingBottom = 4.0f;
            root.Add(header);

            Label label = new Label();
            label.text = "Empty Sprites";
            label.style.flexGrow = 1.0f;
            label.style.marginLeft   = 4.0f;
            label.style.marginTop    = 2.0f;
            label.style.marginBottom = 2.0f;
            header.Add(label);

            Button showButton = new Button(() =>
            {
                bool expanded = EditorPrefs.GetBool(m_expandCacheKey, true);
                expanded = !expanded;

                EditorPrefs.SetBool(m_expandCacheKey, expanded);

                m_showButton.text = expanded ? "Hide" : "Show";
                m_body.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                m_zoom.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
            });
            showButton.style.width = 60.0f;
            showButton.style.marginLeft   = 0.0f;
            showButton.style.marginRight  = 0.0f;
            showButton.style.marginTop    = -1.0f;
            showButton.style.marginBottom = -1.0f;
            showButton.style.backgroundColor = UIColors.blue;
            header.Add(showButton);
            m_showButton = showButton;

            Slider zoom = new Slider();
            zoom.label = "Zoom";
            zoom.lowValue = 0.1f;
            zoom.highValue = 1.0f;
            zoom.RegisterValueChangedCallback(e =>
            {
                float zoom = Mathf.Clamp(e.newValue, 0.1f, 1.0f);
                EditorPrefs.SetFloat(m_zoomCacheKey, zoom);
                m_itemContainer.style.width = Length.Percent(zoom * 100.0f);
            });
            root.Add(zoom);
            m_zoom = zoom;

            MouseCapturer body = new MouseCapturer();
            body.style.flexDirection = FlexDirection.Row;
            body.style.paddingTop    = 8.0f;
            body.style.paddingBottom = 8.0f;
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
                        TileRulePaintable hoveringRule = m_hoveringRule;

                        GenericMenu menu = new GenericMenu();

                        menu.AddItem(new GUIContent("Animate"), false, () =>
                        {
                            TileAnimationPopupWindow.Open(EditorGUIUtility.GetMainWindowPosition().center, hoveringRule.serializedRule.output, () =>
                            {
                                onChanges?.Invoke();
                            });
                        });

                        bool isStatic = hoveringRule.serializedRule.isStatic;
                        menu.AddItem(new GUIContent("Mark as static"), isStatic, () =>
                        {
                            hoveringRule.isStatic = !hoveringRule.isStatic;
                            onChanges?.Invoke();
                        });

                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Remove"), false, () =>
                        {
                            int index = m_serializedRules.IndexOf(hoveringRule.serializedRule);
                            if (index >= 0)
                            {
                                m_serializedRules.RemoveAt(index);
                                m_itemContainer.RemoveChild(index);
                                onChanges?.Invoke();
                            }
                        });

                        menu.ShowAsContext();
                    }
                }
            };
            body.onMouseUp += (b,e) =>
            {
                m_context.active = false;
            };
            root.Add(body);
            m_body = body;

            GridView itemContainer = new GridView(2.0f);
            itemContainer.style.backgroundColor = UIColors.brighten;
            itemContainer.style.borderLeftWidth   = 1.0f;
            itemContainer.style.borderRightWidth  = 1.0f;
            itemContainer.style.borderTopWidth    = 1.0f;
            itemContainer.style.borderBottomWidth = 1.0f;
            itemContainer.style.borderLeftColor   = UIColors.border;
            itemContainer.style.borderRightColor  = UIColors.border;
            itemContainer.style.borderTopColor    = UIColors.border;
            itemContainer.style.borderBottomColor = UIColors.border;
            body.Add(itemContainer);
            m_itemContainer = itemContainer;
        }

        private void CreateTileRuleEditors(Action onChanges)
        {
            m_itemContainer.ClearChildren();

            foreach (SerializedTileRule serializedRule in m_serializedRules)
            {
                TileRulePaintable rule = new TileRulePaintable(m_context);
                rule.style.flexGrow = 1.0f;
                m_itemContainer.InsertChild(rule);

                rule.Bind(serializedRule, onChanges);

                m_onTileColorChanged     += rule.OnTileColorChanged;
                m_onCategoryColorChanged += rule.OnCategoryColorChanged;
                m_onFilterColorChanged   += rule.OnFilterColorChanged;

                rule.RegisterCallback<MouseEnterEvent>(e =>
                {
                    m_hoveringRule = rule;
                    m_hoveringRule.style.backgroundColor = UIColors.lightBlue;
                });
                rule.RegisterCallback<MouseLeaveEvent>(e =>
                {
                    if (m_hoveringRule == rule)
                    {
                        m_hoveringRule.style.backgroundColor = UIColors.transparent;
                        m_hoveringRule = null;
                    }
                });
                rule.RegisterCallback<DetachFromPanelEvent>(e =>
                {
                    if (m_hoveringRule == rule)
                    {
                        m_hoveringRule.style.backgroundColor = UIColors.transparent;
                        m_hoveringRule = null;
                    }
                });
            }

            VisualElement addButton = CreateAddButton();
            addButton.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == 0)
                {
                    int index = m_serializedRules.Count;

                    SerializedTileRule serializedRule = new SerializedTileRule();

                    m_serializedRules.Add(serializedRule);
                    onChanges?.Invoke();

                    TileRulePaintable rule = new TileRulePaintable(m_context);
                    rule.style.flexGrow = 1.0f;
                    m_itemContainer.InsertChild(rule, index);

                    rule.Bind(serializedRule, onChanges);

                    m_onTileColorChanged += rule.OnTileColorChanged;
                    m_onCategoryColorChanged += rule.OnCategoryColorChanged;
                    m_onFilterColorChanged += rule.OnFilterColorChanged;

                    rule.RegisterCallback<MouseEnterEvent>(e =>
                    {
                        m_hoveringRule = rule;
                        m_hoveringRule.style.backgroundColor = UIColors.lightBlue;
                    });
                    rule.RegisterCallback<MouseLeaveEvent>(e =>
                    {
                        if (m_hoveringRule == rule)
                        {
                            m_hoveringRule.style.backgroundColor = UIColors.transparent;
                            m_hoveringRule = null;
                        }
                    });
                    rule.RegisterCallback<DetachFromPanelEvent>(e =>
                    {
                        if (m_hoveringRule == rule)
                        {
                            m_hoveringRule.style.backgroundColor = UIColors.transparent;
                            m_hoveringRule = null;
                        }
                    });
                }
            });
            m_itemContainer.InsertChild(addButton);
        }

        private VisualElement CreateAddButton()
        {
            VisualElement addButton = new VisualElement();
            addButton.style.flexGrow = 1.0f;
            addButton.style.backgroundColor = UIColors.gray;
            addButton.style.borderLeftWidth   = 2.0f;
            addButton.style.borderRightWidth  = 2.0f;
            addButton.style.borderTopWidth    = 2.0f;
            addButton.style.borderBottomWidth = 2.0f;
            addButton.style.borderLeftColor   = UIColors.border;
            addButton.style.borderRightColor  = UIColors.border;
            addButton.style.borderTopColor    = UIColors.border;
            addButton.style.borderBottomColor = UIColors.border;

            VisualElement inner = new VisualElement();
            inner.style.flexGrow = 1.0f;
            inner.style.backgroundColor = UIColors.brighten;
            inner.style.marginLeft   = Length.Percent(12.0f);
            inner.style.marginRight  = Length.Percent(12.0f);
            inner.style.marginTop    = Length.Percent(12.0f);
            inner.style.marginBottom = Length.Percent(12.0f);
            addButton.Add(inner);

            IMGUIContainer icon = new IMGUIContainer();
            icon.style.flexGrow = 1.0f;
            icon.onGUIHandler = () =>
            {
                DrawPlusIcon(icon.contentRect, 0.7f, 0.1f, Color.white);
            };
            inner.Add(icon);

            return addButton;
        }
       
        private void OnTileColorChanged(SerializedTile serializedTile)
        {
            m_onTileColorChanged?.Invoke(serializedTile);
        }

        private void OnCategoryColorChanged(SerializedCategory serializedCategory)
        {
            m_onCategoryColorChanged?.Invoke(serializedCategory);
        }

        private void OnFilterColorChanged(SerializedSpecialFilter serializedSpecialFilter)
        {
            m_onFilterColorChanged?.Invoke(serializedSpecialFilter);
        }

        private void DrawPlusIcon(Rect rect, float sizeScalar, float thicknessScalar, Color color)
        {
            Vector2 center = rect.center;

            float baseSize = Mathf.Min(rect.width, rect.height);
            float iconSize = baseSize * sizeScalar;
            float thickness = baseSize * thicknessScalar;

            Color originalColor = GUI.color;
            GUI.color = color;

            Rect horizontal = new Rect(center.x - iconSize / 2, center.y - thickness / 2, iconSize, thickness);
            EditorGUI.DrawRect(horizontal, color);

            Rect vertical = new Rect(center.x - thickness / 2, center.y - iconSize / 2, thickness, iconSize);
            EditorGUI.DrawRect(vertical, color);

            GUI.color = originalColor;
        }
        private static float RemapClamped(float value, float inMin, float inMax, float outMin, float outMax)
        {
            if (inMin == inMax)
            {
                return outMin;
            }

            value = Mathf.Clamp(value, inMin, inMax);
            float t = (value - inMin) / (inMax - inMin);
            return outMin + t * (outMax - outMin);
        }
    }
}
