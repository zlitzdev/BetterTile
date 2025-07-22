using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using UnityTile = UnityEngine.Tilemaps.Tile;

namespace Zlitz.Extra2D.BetterTile
{
    internal class TileItem : SelectableElement
    {
        private VisualElement m_root;

        private VisualElement m_representSprite;
        private VisualElement m_representSpriteContainer;

        private TextField m_tileName;

        private ColorField m_tileColor;

        private EnumField m_colliderType;

        private ObjectField m_sprite;

        private SerializedTile m_serializedTile;
        private IEnumerable<SerializedTile> m_allTiles;

        public SerializedTile serializedTile => m_serializedTile;

        public override void OnSelected()
        {
            SetSelected(true);
        }

        public override void OnDeselect()
        {
            SetSelected(false);
        }

        public void Bind(SerializedTile tile, IEnumerable<SerializedTile> allTiles)
        {
            m_serializedTile = tile;
            m_allTiles       = allTiles;

            m_tileName.value     = m_serializedTile.tileName;
            m_tileColor.value    = m_serializedTile.tileColor;
            m_colliderType.value = m_serializedTile.colliderType;
            m_sprite.value       = m_serializedTile.sprite;

            m_representSpriteContainer.style.backgroundColor = m_serializedTile.tileColor.WithAlpha(0.4f);

            m_representSprite.style.backgroundImage = new StyleBackground(m_serializedTile.sprite);
        }

        public TileItem(SelectionGroup selectionGroup, Action onChanges, Action<SerializedTile> onNameChanged, Action<SerializedTile> onColorChanged)
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
            tileName.style.flexGrow = 1.0f;
            tileName.style.marginLeft = 0.0f;
            tileName.style.marginRight = 0.0f;
            infoLine1.Add(tileName);
            tileName.RegisterValueChangedCallback(e =>
            {
                string[] existingNames = m_allTiles.Where(t => t != m_serializedTile).Select(t => t.tileName).ToArray();
                string newName = ObjectNames.GetUniqueName(existingNames, e.newValue);

                if (newName != m_serializedTile.tileName)
                {
                    m_tileName.SetValueWithoutNotify(newName);
                    m_serializedTile.tileName = newName;
                    onChanges?.Invoke();
                    onNameChanged?.Invoke(m_serializedTile);
                }
            });
            m_tileName = tileName;

            ColorField tileColor = new ColorField();
            tileColor.showEyeDropper = false;
            tileColor.showAlpha = false;
            tileColor.style.width = 20.0f;
            infoLine1.Add(tileColor);
            tileColor.RegisterValueChangedCallback(e =>
            {
                if (m_serializedTile.tileColor != e.newValue)
                {
                    m_serializedTile.tileColor = e.newValue;

                    Color tileColor = m_serializedTile.tileColor;
                    tileColor.a = 0.4f;
                    m_representSpriteContainer.style.backgroundColor = tileColor;

                    onChanges?.Invoke();
                    onColorChanged?.Invoke(m_serializedTile);
                }
            });
            m_tileColor = tileColor;

            EnumField colliderType = new EnumField(UnityTile.ColliderType.None);
            colliderType.labelElement.style.width      = 80.0f;
            colliderType.labelElement.style.minWidth   = 80.0f;
            colliderType.labelElement.style.flexShrink = 0.0f;
            colliderType.label = "Collider Type";
            infoContainer.Add(colliderType);
            colliderType.RegisterValueChangedCallback(e =>
            {
                UnityTile.ColliderType newColliderType = (UnityTile.ColliderType)e.newValue;
                if (newColliderType != m_serializedTile.colliderType)
                {
                    m_serializedTile.colliderType = newColliderType;
                    onChanges?.Invoke();
                }
            });
            m_colliderType = colliderType;

            ObjectField sprite = new ObjectField();
            sprite.objectType = typeof(Sprite);
            sprite.labelElement.style.width      = 80.0f;
            sprite.labelElement.style.minWidth   = 80.0f;
            sprite.labelElement.style.flexShrink = 0.0f;
            sprite.label = "Sprite";
            infoContainer.Add(sprite);
            sprite.RegisterValueChangedCallback(e =>
            {
                Sprite newSprite = e.newValue as Sprite;
                if (newSprite != m_serializedTile.sprite)
                {
                    m_serializedTile.sprite = newSprite;

                    m_representSprite.style.backgroundImage = new StyleBackground(newSprite);

                    onChanges?.Invoke();
                }
            });
            m_sprite = sprite;
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
