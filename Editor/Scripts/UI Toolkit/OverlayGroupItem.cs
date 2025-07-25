using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal class OverlayGroupItem : VisualElement
    {
        private TileSetEditorWindow.PaintContext m_context;

        private string m_expandCacheKey;
        private string m_zoomCacheKey;

        private SerializedOverlayGroup m_serializedOverlayGroup;

        private Button m_showButton;

        private Slider m_zoom;
        private TextField m_name;
        private ListView m_overlayPatterns;

        private List<OverlayItem> m_items = new List<OverlayItem>();

        private Action m_onRemoved;
        private Action m_onUpdate;

        private Action<SerializedTile>          m_onTileColorChanged;
        private Action<SerializedCategory>      m_onCategoryColorChanged;
        private Action<SerializedSpecialFilter> m_onFilterColorChanged;

        private Action m_onChanges;

        private Action<float, int> m_onZoomChanged;
        private int m_maxPatternWidth;

        private OverlayRulePaintable m_hoveringRule;

        public void Update()
        {
            m_onUpdate?.Invoke();
        }

        public void Bind(TileSet tileSet, SerializedOverlayGroup serializedOverlayGroup, Action onRemoved)
        {
            m_onTileColorChanged     = null;
            m_onCategoryColorChanged = null;
            m_onFilterColorChanged   = null;

            m_serializedOverlayGroup = serializedOverlayGroup;
            m_onRemoved = onRemoved;

            m_expandCacheKey = $"Zlitz.Extra2D.BetterTile.RuleGroupItem_{tileSet.id}_{serializedOverlayGroup.id}.expanded";
            bool expanded = EditorPrefs.GetBool(m_expandCacheKey, true);
            EditorPrefs.SetBool(m_expandCacheKey, expanded);

            m_zoomCacheKey = $"Zlitz.Extra2D.BetterTile.RuleGroupItem_{tileSet.id}_{serializedOverlayGroup.id}.zoom";
            float zoom = EditorPrefs.GetFloat(m_zoomCacheKey, 0.6f);
            EditorPrefs.SetFloat(m_zoomCacheKey, zoom);

            m_showButton.text = expanded ? "Hide" : "Show";
            m_zoom.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
            m_overlayPatterns.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;

            m_overlayPatterns.itemsSource = m_serializedOverlayGroup.overlayPatterns;

            m_zoom.value = zoom;
            m_onZoomChanged?.Invoke(zoom, m_serializedOverlayGroup.overlayPatterns.Select(o => o.size.x).Max());

            m_name.value = m_serializedOverlayGroup.name;
        }

        public OverlayGroupItem(TileSetEditorWindow.PaintContext context, Action onChanges)
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

            TextField name = new TextField();
            name.label = "Name";
            name.labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;
            name.style.flexGrow = 1.0f;
            header.Add(name);
            name.RegisterValueChangedCallback(e =>
            {
                if (m_serializedOverlayGroup.name != e.newValue)
                {
                    m_serializedOverlayGroup.name = e.newValue;
                    m_onChanges?.Invoke();
                }
            });
            m_name = name;

            Button showButton = new Button(() =>
            {
                bool expanded = EditorPrefs.GetBool(m_expandCacheKey, true);
                expanded = !expanded;

                EditorPrefs.SetBool(m_expandCacheKey, expanded);

                m_showButton.text = expanded ? "Hide" : "Show";
                m_zoom.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                m_overlayPatterns.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
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
                m_onZoomChanged?.Invoke(zoom, m_serializedOverlayGroup.overlayPatterns.Select(o => o.size.x).Max());
            });
            root.Add(zoom);
            m_zoom = zoom;

            ListView overlayPatterns = new ListView();
            overlayPatterns.headerTitle = "Patterns";
            overlayPatterns.showAddRemoveFooter = true;
            overlayPatterns.showFoldoutHeader = true;
            overlayPatterns.showBoundCollectionSize = true;
            overlayPatterns.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            overlayPatterns.reorderable = true;
            overlayPatterns.reorderMode = ListViewReorderMode.Animated;
            overlayPatterns.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            overlayPatterns.selectionType = SelectionType.None;
            overlayPatterns.style.flexGrow = 1.0f;
            root.Add(overlayPatterns);
            m_overlayPatterns = overlayPatterns;

            overlayPatterns.makeItem = () =>
            {
                OverlayItem overlayItem = new OverlayItem(m_context);

                m_onTileColorChanged     += overlayItem.OnTileColorChanged;
                m_onCategoryColorChanged += overlayItem.OnCategoryColorChanged;
                m_onFilterColorChanged   += overlayItem.OnFilterColorChanged;
                m_onZoomChanged          += overlayItem.OnZoomChanged;
                m_onUpdate               += overlayItem.Update;

                overlayItem.onSizeChanged += (size) =>
                {
                    EditorApplication.delayCall += () =>
                    {
                        int maxWidth = m_serializedOverlayGroup.overlayPatterns.Select(o => o.size.x).Max();
                        if (maxWidth != m_maxPatternWidth)
                        {
                            m_maxPatternWidth = maxWidth;
                            m_onZoomChanged?.Invoke(m_zoom.value, m_maxPatternWidth);
                        }
                    };
                };

                m_items.Add(overlayItem);

                overlayItem.style.marginRight = -4.0f;

                return overlayItem;
            };
            overlayPatterns.destroyItem = (e) =>
            {
                if (e is OverlayItem overlayItem)
                {
                    m_onTileColorChanged     -= overlayItem.OnTileColorChanged;
                    m_onCategoryColorChanged -= overlayItem.OnCategoryColorChanged;
                    m_onFilterColorChanged   -= overlayItem.OnFilterColorChanged;
                    m_onZoomChanged          -= overlayItem.OnZoomChanged;
                    m_onUpdate               -= overlayItem.Update;

                    m_items.Remove(overlayItem);
                }
            };
            overlayPatterns.bindItem = (e, i) =>
            {
                (e as OverlayItem)?.Bind(m_serializedOverlayGroup.overlayPatterns[i],
                    () =>
                    {
                        m_onChanges?.Invoke();
                    }, 
                    () =>
                    {
                        (e as OverlayItem)?.OnZoomChanged(m_zoom.value, m_serializedOverlayGroup.overlayPatterns.Select(o => o.size.x).Max());
                    });
            };

            overlayPatterns.itemsAdded += (indices) =>
            {
                if (indices.Any())
                {
                    foreach (int index in indices)
                    {
                        m_serializedOverlayGroup.overlayPatterns[index] = new SerializedOverlay(Guid.NewGuid().ToString());
                    }
                    m_onChanges?.Invoke();
                }
            };
            overlayPatterns.itemsRemoved += (indices) =>
            {
                if (indices.Any())
                {
                    m_onChanges?.Invoke();
                }
            };
            overlayPatterns.itemIndexChanged += (oldIndex, newIndex) =>
            {
                if (oldIndex != newIndex)
                {
                    m_onChanges?.Invoke();
                }
            };
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
