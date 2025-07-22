using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal class TileCategoryItem : SelectableElement
    {
        private VisualElement m_root;

        private VisualElement m_representSprite;
        private VisualElement m_representSpriteContainer;

        private TextField m_tileName;

        private ColorField m_categoryColor;

        private TileCollection m_tiles;

        private SerializedCategory m_serializedCategory;
        private IEnumerable<SerializedCategory> m_allCategories;

        private IVisualElementScheduledItem m_currentTask;

        public SerializedCategory serializedCategory => m_serializedCategory;

        public void SetAvailableTiles(IEnumerable<SerializedTile> tiles)
        {
            m_tiles.options = tiles;
        }

        public override void OnSelected()
        {
            SetSelected(true);
        }

        public override void OnDeselect()
        {
            SetSelected(false);
        }

        public void Bind(SerializedCategory category, IEnumerable<SerializedCategory> allCategories, IEnumerable<SerializedTile> tiles)
        {
            m_serializedCategory = category;
            m_allCategories      = allCategories;

            m_tileName.value      = m_serializedCategory.categoryName;
            m_categoryColor.value = m_serializedCategory.categoryColor;
            m_tiles.SetValueWithoutNotify(m_serializedCategory.tiles);

            Color categoryColor = m_serializedCategory.categoryColor;
            categoryColor.a = 0.4f;
            m_representSpriteContainer.style.backgroundColor = categoryColor;

            if (m_currentTask != null && m_currentTask.isActive)
            {
                m_currentTask.Pause();
            }
            m_representSprite.style.backgroundImage = new StyleBackground(AnimatedSprite(tiles));
            m_currentTask = m_representSprite.schedule.Execute(() => m_representSprite.style.backgroundImage = new StyleBackground(AnimatedSprite(tiles))).Every(1000);
        }

        public TileCategoryItem(SelectionGroup selectionGroup, Action onChanges, Action<SerializedCategory> onNameChanged, Action<SerializedCategory> onColorChanged)
            : base(selectionGroup)
        {
            VisualElement root = new VisualElement();
            root.style.flexGrow = 1.0f;
            root.style.flexDirection = FlexDirection.Row;
            Add(root);
            m_root = root;
            SetSelected(false);

            VisualElement representSpriteContainer = new VisualElement();
            representSpriteContainer.pickingMode = PickingMode.Ignore;
            representSpriteContainer.style.width  = 64.0f;
            representSpriteContainer.style.height = 64.0f;
            representSpriteContainer.style.borderLeftWidth   = 1.0f;
            representSpriteContainer.style.borderRightWidth  = 1.0f;
            representSpriteContainer.style.borderTopWidth    = 1.0f;
            representSpriteContainer.style.borderBottomWidth = 1.0f;
            representSpriteContainer.style.marginLeft        = 6.0f;
            representSpriteContainer.style.marginRight       = 2.0f;
            representSpriteContainer.style.marginTop         = 6.0f;
            representSpriteContainer.style.marginBottom      = 6.0f;
            representSpriteContainer.style.borderLeftColor   = UIColors.border;
            representSpriteContainer.style.borderRightColor  = UIColors.border;
            representSpriteContainer.style.borderTopColor    = UIColors.border;
            representSpriteContainer.style.borderBottomColor = UIColors.border;
            representSpriteContainer.style.backgroundColor   = UIColors.darken;
            root.Add(representSpriteContainer);
            m_representSpriteContainer = representSpriteContainer;

            VisualElement representSprite = new VisualElement();
            representSprite.pickingMode = PickingMode.Ignore;
            representSprite.style.flexGrow = 1.0f;
            representSprite.style.marginLeft   = 8.0f;
            representSprite.style.marginRight  = 8.0f;
            representSprite.style.marginTop    = 8.0f;
            representSprite.style.marginBottom = 8.0f;
            representSpriteContainer.Add(representSprite);
            m_representSprite = representSprite;

            VisualElement infoContainer = new VisualElement();
            infoContainer.style.flexGrow = 1.0f;
            infoContainer.style.marginLeft   = 2.0f;
            infoContainer.style.marginRight  = 6.0f;
            infoContainer.style.marginTop    = 6.0f;
            infoContainer.style.marginBottom = 6.0f;
            root.Add(infoContainer);

            VisualElement infoLine1 = new VisualElement();
            infoLine1.style.flexDirection = FlexDirection.Row;
            infoContainer.Add(infoLine1);

            TextField tileName = new TextField();
            tileName.style.flexGrow    = 1.0f;
            tileName.style.marginLeft  = 0.0f;
            tileName.style.marginRight = 0.0f;
            infoLine1.Add(tileName);
            tileName.RegisterValueChangedCallback(e =>
            {
                string[] existingNames = m_allCategories.Where(t => t != m_serializedCategory).Select(c => c.categoryName).ToArray();
                string newName = ObjectNames.GetUniqueName(existingNames, e.newValue);

                if (newName != m_serializedCategory.categoryName)
                {
                    m_tileName.SetValueWithoutNotify(newName);
                    m_serializedCategory.categoryName = newName;
                    onChanges?.Invoke();
                    onNameChanged?.Invoke(m_serializedCategory);
                }
            });
            m_tileName = tileName;

            ColorField categoryColor = new ColorField();
            categoryColor.showEyeDropper = false;
            categoryColor.showAlpha = false;
            categoryColor.style.width = 20.0f;
            infoLine1.Add(categoryColor);
            categoryColor.RegisterValueChangedCallback(e =>
            {
                if (m_serializedCategory.categoryColor != e.newValue)
                {
                    m_serializedCategory.categoryColor = e.newValue;

                    Color categoryColor = m_serializedCategory.categoryColor;
                    categoryColor.a = 0.4f;
                    m_representSpriteContainer.style.backgroundColor = categoryColor;

                    onChanges?.Invoke();
                    onColorChanged?.Invoke(m_serializedCategory);
                }
            });
            m_categoryColor = categoryColor;

            TileCollection tiles = new TileCollection();
            tiles.label = "Content";
            tiles.onValueChanged += (newTiles) =>
            {
                m_serializedCategory.tiles.Clear();
                m_serializedCategory.tiles.AddRange(newTiles);

                onChanges?.Invoke();
            };
            infoContainer.Add(tiles);
            m_tiles = tiles;
        }
    
        private Sprite AnimatedSprite(IEnumerable<SerializedTile> tiles)
        {
            if (m_serializedCategory.tiles == null || m_serializedCategory.tiles.Count <= 0)
            {
                return null;
            }

            int index = (int)(EditorApplication.timeSinceStartup * 0.5f);
            Tile tile = m_serializedCategory.tiles[index % m_serializedCategory.tiles.Count];

            SerializedTile serializedTile = null;
            if (tiles != null)
            {
                foreach (SerializedTile t in tiles)
                {
                    if (t.tile == tile)
                    {
                        serializedTile = t;
                        break;
                    }
                }
            }

            return serializedTile?.sprite;
        }
    
        private void SetSelected(bool selected)
        {
            float borderWidth = selected ? 2.0f : 1.0f;
            Color borderColor = selected ? UIColors.border : UIColors.brighten;

            m_root.style.backgroundColor = selected ? UIColors.lightGray : UIColors.gray;

            m_root.style.borderLeftWidth   = borderWidth;
            m_root.style.borderRightWidth  = borderWidth;
            m_root.style.borderTopWidth    = borderWidth;
            m_root.style.borderBottomWidth = borderWidth;

            m_root.style.borderLeftColor   = borderColor;
            m_root.style.borderRightColor  = borderColor;
            m_root.style.borderTopColor    = borderColor;
            m_root.style.borderBottomColor = borderColor;
        }
    }
}
