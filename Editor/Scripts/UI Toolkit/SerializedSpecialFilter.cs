using System;
using UnityEditor;
using UnityEngine;

namespace Zlitz.Extra2D.BetterTile
{
    [Serializable]
    internal class SerializedSpecialFilter
    {
        [SerializeField]
        private TileFilterType m_filterType;

        [SerializeField]
        private Color m_filterColor;

        public TileFilterType filterType
        {
            get => m_filterType;
            set => m_filterType = value;
        }

        public Color filterColor
        {
            get => m_filterColor;
            set => m_filterColor = value;
        }

        public bool Update(SerializedProperty specialFilterProperty)
        {
            SerializedProperty filterTypeProperty = specialFilterProperty.FindPropertyRelative("m_type");
            m_filterType = (TileFilterType)filterTypeProperty.enumValueIndex;

            SerializedProperty filterColorProperty = specialFilterProperty.FindPropertyRelative("m_color");
            m_filterColor = filterColorProperty.colorValue;

            return false;
        }

        public void SaveChanges(SerializedProperty specialFilterProperty)
        {
            SerializedProperty filterTypeProperty = specialFilterProperty.FindPropertyRelative("m_type");
            filterTypeProperty.enumValueIndex = (int)m_filterType;

            SerializedProperty filterColorProperty = specialFilterProperty.FindPropertyRelative("m_color");
            filterColorProperty.colorValue = m_filterColor.WithAlpha(1.0f);
        }
    }
}
