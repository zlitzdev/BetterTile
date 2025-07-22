using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Zlitz.Extra2D.BetterTile
{
    [Serializable]
    internal class SerializedOverlayGroup
    {
        [SerializeField]
        private string m_id;

        [SerializeField]
        private string m_name;

        [SerializeField]
        private List<SerializedOverlay> m_overlayPatterns = new List<SerializedOverlay>();

        public string id => m_id;

        public string name
        {
            get => m_name;
            set => m_name = value;
        }

        public List<SerializedOverlay> overlayPatterns => m_overlayPatterns;

        public SerializedOverlayGroup()
        {
        }

        public SerializedOverlayGroup(string id)
        {
            m_id = id;
        }

        public bool Update(SerializedProperty ruleGroupProperty)
        {
            bool shouldSave = false;

            SerializedProperty idProperty = ruleGroupProperty.FindPropertyRelative("m_id");
            m_id = idProperty.stringValue;

            SerializedProperty nameProperty = ruleGroupProperty.FindPropertyRelative("m_debugName");
            m_name = nameProperty.stringValue;

            SerializedProperty overlayPatternsProperty = ruleGroupProperty.FindPropertyRelative("m_overlayPatterns");
            m_overlayPatterns.Clear();
            for (int i = 0; i < overlayPatternsProperty.arraySize; i++)
            {
                SerializedProperty overlayPatternProperty = overlayPatternsProperty.GetArrayElementAtIndex(i);

                SerializedOverlay serializedOverlayPattern = new SerializedOverlay(Guid.NewGuid().ToString());
                if (serializedOverlayPattern.Update(overlayPatternProperty))
                {
                    shouldSave = true;
                }

                m_overlayPatterns.Add(serializedOverlayPattern);
            }

            return shouldSave;
        }

        public bool OnTilesDeleted(IEnumerable<SerializedTile> tiles)
        {
            bool changed = false;

            foreach (SerializedOverlay overlayPattern in m_overlayPatterns)
            {
                if (overlayPattern.OnTilesDeleted(tiles))
                {
                    changed = true;
                }
            }
            return changed;
        }

        public bool OnCategoriesDeleted(IEnumerable<SerializedCategory> categories)
        {
            bool changed = false;

            foreach (SerializedOverlay overlayPattern in m_overlayPatterns)
            {
                if (overlayPattern.OnCategoriesDeleted(categories))
                {
                    changed = true;
                }
            }

            return changed;
        }

        public void SaveChanges(TileSet tileSet, SerializedProperty ruleGroupProperty)
        {
            SerializedProperty idProperty = ruleGroupProperty.FindPropertyRelative("m_id");
            idProperty.stringValue = m_id;

            SerializedProperty nameProperty = ruleGroupProperty.FindPropertyRelative("m_debugName");
            nameProperty.stringValue = m_name;

            SerializedProperty overlayPatternsProperty = ruleGroupProperty.FindPropertyRelative("m_overlayPatterns");
            overlayPatternsProperty.arraySize = m_overlayPatterns.Count;
            for (int i = 0; i < m_overlayPatterns.Count; i++)
            {
                SerializedProperty overlayPatternProperty = overlayPatternsProperty.GetArrayElementAtIndex(i);

                m_overlayPatterns[i].SaveChanges(tileSet, overlayPatternProperty);
            }
        }
    }

}
