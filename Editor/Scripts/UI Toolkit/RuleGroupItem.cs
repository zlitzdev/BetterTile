using System;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal class RuleGroupItem : VisualElement
    {
        private TileSetEditorWindow.PaintContext m_context;

        private string m_expandCacheKey;
        private string m_zoomCacheKey;

        private SerializedRuleGroup m_serializedRuleGroup;

        private ObjectField m_texture;

        private Button m_showButton;

        private MouseCapturer m_body;
        private Slider m_zoom;
        private AspectRatioPanel m_textureContainer;
        private VisualElement m_textureDisplay;

        private VisualElement m_rulesContainer;

        private TileRulePaintable m_hoveringRule;

        private Action m_onRemoved;

        private Action<SerializedTile>          m_onTileColorChanged;
        private Action<SerializedCategory>      m_onCategoryColorChanged;
        private Action<SerializedSpecialFilter> m_onFilterColorChanged;

        private Action m_onChanges;

        public void Bind(TileSet tileSet, SerializedRuleGroup serializedRuleGroup, Action onRemoved)
        {
            m_onTileColorChanged     = null;
            m_onCategoryColorChanged = null;
            m_onFilterColorChanged   = null;

            m_serializedRuleGroup = serializedRuleGroup;
            m_onRemoved = onRemoved;

            m_expandCacheKey = $"Zlitz.Extra2D.BetterTile.RuleGroupItem_{tileSet.id}_{serializedRuleGroup.id}.expanded";
            bool expanded = EditorPrefs.GetBool(m_expandCacheKey, true);
            EditorPrefs.SetBool(m_expandCacheKey, expanded);

            m_zoomCacheKey = $"Zlitz.Extra2D.BetterTile.RuleGroupItem_{tileSet.id}_{serializedRuleGroup.id}.zoom";
            float zoom = EditorPrefs.GetFloat(m_zoomCacheKey, 0.6f);
            EditorPrefs.SetFloat(m_zoomCacheKey, zoom);

            m_showButton.text = expanded ? "Hide" : "Show";
            m_zoom.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
            m_body.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;

            m_zoom.value = zoom;

            m_texture.label = $"{m_serializedRuleGroup.rules.Count} sprite(s)";
            m_texture.value = m_serializedRuleGroup.texture;

            float aspectRatio = (float)m_serializedRuleGroup.texture.width / m_serializedRuleGroup.texture.height;

            m_textureContainer.style.width = Length.Percent(zoom * 100.0f);
            m_textureContainer.aspectRatio = aspectRatio;
            m_textureDisplay.style.backgroundImage = m_serializedRuleGroup.texture;

            PopulateRules(m_onChanges);
        }

        public RuleGroupItem(TileSetEditorWindow.PaintContext context, Action onChanges)
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

            ObjectField texture = new ObjectField();
            texture.labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;
            texture.objectType = typeof(Texture2D);
            texture.style.flexGrow = 1.0f;
            texture.SetEnabled(false);
            header.Add(texture);
            m_texture = texture;

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

            Button removeButton = new Button(() =>
            {
                m_onRemoved?.Invoke();
            });
            removeButton.text = "Remove";
            removeButton.style.width = 60.0f;
            removeButton.style.marginRight  = 0.0f;
            removeButton.style.marginTop    = -1.0f;
            removeButton.style.marginBottom = -1.0f;
            removeButton.style.backgroundColor = UIColors.red;
            header.Add(removeButton);

            Slider zoom = new Slider();
            zoom.label = "Zoom";
            zoom.lowValue = 0.1f;
            zoom.highValue = 1.0f;
            zoom.RegisterValueChangedCallback(e =>
            {
                float zoom = Mathf.Clamp(e.newValue, 0.1f, 1.0f);
                EditorPrefs.SetFloat(m_zoomCacheKey, zoom);
                m_textureContainer.style.width = Length.Percent(zoom * 100.0f);
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

            AspectRatioPanel textureContainer = new AspectRatioPanel();
            textureContainer.lockHeight = false;
            textureContainer.style.backgroundColor = UIColors.brighten;
            textureContainer.style.borderLeftWidth   = 1.0f;
            textureContainer.style.borderRightWidth  = 1.0f;
            textureContainer.style.borderTopWidth    = 1.0f;
            textureContainer.style.borderBottomWidth = 1.0f;
            textureContainer.style.borderLeftColor   = UIColors.border;
            textureContainer.style.borderRightColor  = UIColors.border;
            textureContainer.style.borderTopColor    = UIColors.border;
            textureContainer.style.borderBottomColor = UIColors.border;
            body.Add(textureContainer);
            m_textureContainer = textureContainer;

            VisualElement textureDisplay = new VisualElement();
            textureDisplay.style.flexGrow = 1.0f;
            textureContainer.Add(textureDisplay);
            m_textureDisplay = textureDisplay;

            VisualElement rulesContainer = new VisualElement();
            rulesContainer.style.position = Position.Absolute;
            rulesContainer.style.left   = 0.0f;
            rulesContainer.style.right  = 0.0f;
            rulesContainer.style.top    = 0.0f;
            rulesContainer.style.bottom = 0.0f;
            textureContainer.Add(rulesContainer);
            m_rulesContainer = rulesContainer;
        }

        private void PopulateRules(Action onChanges)
        {
            m_rulesContainer.Clear();

            foreach (SerializedTileRule serializedRule in m_serializedRuleGroup.rules)
            {
                Sprite sprite = serializedRule.output.sprite;
                
                Vector2 spriteSize = sprite.rect.size;

                Vector2 spriteOffset = sprite.rect.position; 
                spriteOffset.y = m_serializedRuleGroup.texture.height - spriteSize.y - spriteOffset.y;

                TileRulePaintable rule = new TileRulePaintable(m_context);
                rule.style.position = Position.Absolute;
                rule.style.left   = Length.Percent(spriteOffset.x / m_serializedRuleGroup.texture.width * 100.0f);
                rule.style.top    = Length.Percent(spriteOffset.y / m_serializedRuleGroup.texture.height * 100.0f);
                rule.style.width  = Length.Percent(spriteSize.x / m_serializedRuleGroup.texture.width * 100.0f);
                rule.style.height = Length.Percent(spriteSize.y / m_serializedRuleGroup.texture.height * 100.0f);
                m_rulesContainer.Add(rule);

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
    }
}
