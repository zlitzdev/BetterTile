using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal class TileCollectionPopupWindow : EditorWindow
    {
        private VisualElement m_itemContainer;

        public static void Open(Rect buttonRect, IEnumerable<SerializedTile> options, ICollection<Tile> selected, Action<Tile> onSelect, Action<Tile> onDeselect)
        {
            buttonRect.y -= buttonRect.height;

            TileCollectionPopupWindow window = CreateInstance<TileCollectionPopupWindow>();
            window.ShowAsDropDown(buttonRect, new Vector2(buttonRect.width, 320.0f));

            window.PopulateItems(options, selected, onSelect, onDeselect);
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.borderLeftWidth   = 1.0f;
            root.style.borderRightWidth  = 1.0f;
            root.style.borderTopWidth    = 1.0f;
            root.style.borderBottomWidth = 1.0f;
            root.style.borderLeftColor   = UIColors.border;
            root.style.borderRightColor  = UIColors.border;
            root.style.borderTopColor    = UIColors.border;
            root.style.borderBottomColor = UIColors.border;

            VisualElement header = new VisualElement();
            header.style.backgroundColor = UIColors.brighten;
            header.style.height = 22.0f;
            header.style.minHeight = 22.0f;
            header.style.marginBottom = 2.0f;
            root.Add(header);

            Label title = new Label();
            title.text = "Select Tiles";
            title.style.flexGrow = 1.0f;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.Add(title);

            ScrollView scrollView = new ScrollView();
            root.Add(scrollView);
            m_itemContainer = scrollView;
        }

        private void PopulateItems(IEnumerable<SerializedTile> options, ICollection<Tile> selected, Action<Tile> onSelect, Action<Tile> onDeselect)
        {
            m_itemContainer.Clear();
            foreach (SerializedTile tile in options)
            {
                VisualElement tileItem = CreateTileItem(m_itemContainer, tile, selected.Contains(tile.tile), onSelect, onDeselect);
            }
        }

        private VisualElement CreateTileItem(VisualElement parent, SerializedTile tile, bool selected, Action<Tile> onSelect, Action<Tile> onDeselect)
        {
            SerializedObject serializedObject = new SerializedObject(tile.tile);

            SerializedProperty colorProperty = serializedObject.FindProperty("m_color");

            VisualElement root = new VisualElement();
            root.style.height = 24.0f;
            root.style.justifyContent = Justify.Center;
            parent.Add(root);

            VisualElement container = new VisualElement();
            container.style.height = 22.0f;
            container.style.flexDirection = FlexDirection.Row;
            container.style.backgroundColor = UIColors.darken;
            root.Add(container);

            VisualElement toggleContainer = new VisualElement();
            toggleContainer.style.justifyContent = Justify.Center;
            toggleContainer.style.paddingLeft = 2.0f;
            toggleContainer.style.borderRightWidth = 1.0f;
            toggleContainer.style.borderRightColor = UIColors.brighten;
            container.Add(toggleContainer);

            Toggle selectToggle = new Toggle();
            selectToggle.value = selected;
            selectToggle.style.width = 20.0f;
            selectToggle.style.marginLeft  = 2.0f;
            selectToggle.style.marginRight = 0.0f;
            toggleContainer.Add(selectToggle);

            Label tileName = new Label();
            tileName.text = tile.tileName;
            tileName.style.color = colorProperty.colorValue;
            tileName.style.unityFontStyleAndWeight = FontStyle.Bold;
            tileName.style.unityTextAlign = TextAnchor.MiddleLeft;
            tileName.style.flexGrow = 1.0f;
            tileName.style.marginLeft = 4.0f;
            container.Add(tileName);

            tileName.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == 0)
                {
                    bool newValue = !selectToggle.value;
                    selectToggle.SetValueWithoutNotify(newValue);
                    if (newValue)
                    {
                        onSelect?.Invoke(tile.tile);
                    }
                    else
                    {
                        onDeselect?.Invoke(tile.tile);
                    }
                }
            });

            selectToggle.RegisterValueChangedCallback(e =>
            {
                if (e.newValue)
                {
                    onSelect?.Invoke(tile.tile);
                }
                else
                {
                    onDeselect?.Invoke(tile.tile);
                }
            });

            return container;
        }
    }
}
