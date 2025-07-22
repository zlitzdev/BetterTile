using System;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal class SpecialFilterItem : SelectableElement
    {
        private VisualElement m_root;

        private VisualElement m_colorDisplay;

        private TextField m_filterName;

        private ColorField m_filterColor;

        private HelpBox m_infoBox;

        private SerializedSpecialFilter m_serializedSpecialFilter;

        public SerializedSpecialFilter serializedSpecialFilter => m_serializedSpecialFilter;

        public void Bind(SerializedSpecialFilter specialFilter)
        {
            m_serializedSpecialFilter = specialFilter;

            m_filterName.value = ObjectNames.NicifyVariableName(m_serializedSpecialFilter.filterType.ToString());
            m_infoBox.text = GetFilterTypeInfo(m_serializedSpecialFilter.filterType);

            m_filterColor.value = m_serializedSpecialFilter.filterColor;

            Color filterColor = m_serializedSpecialFilter.filterColor;
            filterColor.a = 0.4f;
            m_colorDisplay.style.backgroundColor = filterColor;
        }

        public override void OnSelected()
        {
            SetSelected(true);
        }

        public override void OnDeselect()
        {
            SetSelected(false);
        }

        public SpecialFilterItem(SelectionGroup selectionGroup, Action onChanges, Action<SerializedSpecialFilter> onColorChanged)
            : base(selectionGroup)
        {
            VisualElement root = new VisualElement();
            root.style.flexGrow = 1.0f;
            root.style.flexDirection = FlexDirection.Row;
            Add(root);
            m_root = root;
            SetSelected(false);

            VisualElement colorDisplay = new VisualElement();
            colorDisplay.pickingMode = PickingMode.Ignore;
            colorDisplay.style.width  = 64.0f;
            colorDisplay.style.height = 64.0f;
            colorDisplay.style.borderLeftWidth   = 1.0f;
            colorDisplay.style.borderRightWidth  = 1.0f;
            colorDisplay.style.borderTopWidth    = 1.0f;
            colorDisplay.style.borderBottomWidth = 1.0f;
            colorDisplay.style.marginLeft        = 6.0f;
            colorDisplay.style.marginRight       = 2.0f;
            colorDisplay.style.marginTop         = 6.0f;
            colorDisplay.style.marginBottom      = 6.0f;
            colorDisplay.style.borderLeftColor   = UIColors.border;
            colorDisplay.style.borderRightColor  = UIColors.border;
            colorDisplay.style.borderTopColor    = UIColors.border;
            colorDisplay.style.borderBottomColor = UIColors.border;
            colorDisplay.style.backgroundColor   = UIColors.darken;
            root.Add(colorDisplay);
            m_colorDisplay = colorDisplay;

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

            TextField filterName = new TextField();
            filterName.style.flexGrow = 1.0f;
            filterName.style.marginLeft  = 0.0f;
            filterName.style.marginRight = 0.0f;
            infoLine1.Add(filterName);
            m_filterName = filterName;

            VisualElement filterNameBlock = new VisualElement();
            filterNameBlock.style.top      = 0.0f;
            filterNameBlock.style.left     = 0.0f;
            filterNameBlock.style.right    = 20.0f;
            filterNameBlock.style.bottom   = 0.0f;
            filterNameBlock.style.position = Position.Absolute;
            filterNameBlock.pickingMode    = PickingMode.Position;
            infoLine1.Add(filterNameBlock);
            filterNameBlock.BringToFront();

            ColorField filterColor = new ColorField();
            filterColor.showEyeDropper = false;
            filterColor.showAlpha = false;
            filterColor.style.width = 20.0f;
            infoLine1.Add(filterColor);
            filterColor.RegisterValueChangedCallback(e =>
            {
                if (m_serializedSpecialFilter.filterColor != e.newValue)
                {
                    m_serializedSpecialFilter.filterColor = e.newValue;

                    Color filterColor = m_serializedSpecialFilter.filterColor;
                    filterColor.a = 0.4f;
                    m_colorDisplay.style.backgroundColor = filterColor;

                    onChanges?.Invoke();
                    onColorChanged?.Invoke(m_serializedSpecialFilter);
                }
            });
            m_filterColor = filterColor;

            HelpBox infoBox = new HelpBox();
            infoBox.messageType = HelpBoxMessageType.Info;
            infoBox.style.flexGrow = 1.0f;
            infoBox.style.marginLeft   = 0.0f;
            infoBox.style.marginTop    = 4.0f;
            infoContainer.Add(infoBox);
            m_infoBox = infoBox;
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
        
        private static string GetFilterTypeInfo(TileFilterType filterType)
        {
            switch (filterType)
            {
                case TileFilterType.Decorator:
                    {
                        return "Apply to empty tiles.";
                    }
            }

            return "";
        }
    }
}
