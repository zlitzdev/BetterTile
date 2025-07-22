using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Zlitz.Extra2D.BetterTile
{
    [Serializable]
    internal class SerializedRuleGroup
    {
        [SerializeField]
        private string m_id;

        [SerializeField]
        private Texture2D m_texture;

        [SerializeField]
        private List<SerializedTileRule> m_rules = new List<SerializedTileRule>();

        public string id => m_id;

        public Texture2D texture
        {
            get => m_texture;
            set => m_texture = value;
        }

        public List<SerializedTileRule> rules => m_rules;

        public SerializedRuleGroup()
        {
        }

        public SerializedRuleGroup(string id)
        {
            m_id = id;
        }

        public bool Update(SerializedProperty ruleGroupProperty)
        {
            bool shouldSave = false;

            SerializedProperty idProperty = ruleGroupProperty.FindPropertyRelative("m_id");
            m_id = idProperty.stringValue;

            SerializedProperty textureProperty = ruleGroupProperty.FindPropertyRelative("m_texture");
            m_texture = textureProperty.objectReferenceValue as Texture2D;

            HashSet<Sprite> existingSprite = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(m_texture)).OfType<Sprite>().ToHashSet();

            SerializedProperty rulesProperty = ruleGroupProperty.FindPropertyRelative("m_rules");
            m_rules.Clear();
            for (int i = 0; i < rulesProperty.arraySize; i++)
            {
                SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(i);

                SerializedTileRule serializedRule = new SerializedTileRule();
                if (serializedRule.Update(ruleProperty))
                {
                    shouldSave = true;
                }

                m_rules.Add(serializedRule);
            }

            return shouldSave;
        }

        public bool OnTilesDeleted(IEnumerable<SerializedTile> tiles)
        {
            bool changed = false;

            foreach (SerializedTileRule rule in m_rules)
            {
                if (rule.OnTilesDeleted(tiles))
                {
                    changed = true;
                }
            }
            return changed;
        }

        public bool OnCategoriesDeleted(IEnumerable<SerializedCategory> categories)
        {
            bool changed = false;

            foreach (SerializedTileRule rule in m_rules)
            {
                if (rule.OnCategoriesDeleted(categories))
                {
                    changed = true;
                }
            }

            return changed;
        }

        public void SaveChanges(SerializedProperty ruleGroupProperty)
        {
            SerializedProperty idProperty = ruleGroupProperty.FindPropertyRelative("m_id");
            idProperty.stringValue = m_id;

            SerializedProperty textureProperty = ruleGroupProperty.FindPropertyRelative("m_texture");
            textureProperty.objectReferenceValue = m_texture;

            SerializedProperty rulesProperty = ruleGroupProperty.FindPropertyRelative("m_rules");
            rulesProperty.arraySize = m_rules.Count;
            for (int i = 0; i < m_rules.Count; i++)
            {
                SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(i);

                m_rules[i].SaveChanges(ruleProperty);
            }
        }
    }

}
