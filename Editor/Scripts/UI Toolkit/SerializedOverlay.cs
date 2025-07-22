using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using UnityTile = UnityEngine.Tilemaps.Tile;

namespace Zlitz.Extra2D.BetterTile
{
    [Serializable]
    internal class SerializedOverlay
    {
        [SerializeField]
        private string m_id;

        [SerializeField]
        private Vector2Int m_size = Vector2Int.one;

        [SerializeField]
        private Vector2Int m_cellSize = Vector2Int.one;

        [SerializeField]
        private Vector2Int m_spacing;

        [SerializeField]
        private float m_density = 0.5f;

        [SerializeField]
        private List<SerializedOverlayRule> m_rules = new List<SerializedOverlayRule>();

        public string id => m_id;

        public Vector2Int size
        {
            get => m_size;
            set
            {
                Vector2Int oldSize = m_size;
                m_size = value;
                ValidateSize(oldSize);
            }
        }

        public Vector2Int cellSize
        {
            get => m_cellSize;
            set => m_cellSize = value;
        }

        public Vector2Int spacing
        {
            get => m_spacing;
            set => m_spacing = value;
        }

        public float density
        {
            get => m_density;
            set => m_density = value;
        }

        public List<SerializedOverlayRule> rules => m_rules;

        public bool Update(SerializedProperty overlayProperty)
        {
            bool shouldSave = false;

            SerializedProperty idProperty = overlayProperty.FindPropertyRelative("m_id");
            m_id = idProperty.stringValue;

            SerializedProperty sizeProperty = overlayProperty.FindPropertyRelative("m_size");
            m_size = sizeProperty.vector2IntValue;
            Vector2Int size = m_size;
            size.x = Mathf.Max(1, size.x);
            size.y = Mathf.Max(1, size.y);
            if (size != m_size)
            {
                m_size = size;
                sizeProperty.vector2IntValue = size;
                shouldSave = true;
            }

            SerializedProperty cellSizeProperty = overlayProperty.FindPropertyRelative("m_cellSize");
            m_cellSize = cellSizeProperty.vector2IntValue;
            Vector2Int cellSize = m_cellSize;
            cellSize.x = Mathf.Max(cellSize.x, m_size.x);
            cellSize.y = Mathf.Max(cellSize.y, m_size.y);
            if (cellSize != m_cellSize)
            {
                m_cellSize = cellSize;
                cellSizeProperty.vector2IntValue = cellSize;
                shouldSave = true;
            }

            SerializedProperty spacingProperty = overlayProperty.FindPropertyRelative("m_spacing");
            m_spacing = spacingProperty.vector2IntValue;
            Vector2Int spacing = m_spacing;
            spacing.x = Mathf.Max(0, spacing.x);
            spacing.y = Mathf.Max(0, spacing.y);
            if (spacing != m_spacing)
            {
                m_spacing = spacing;
                spacingProperty.vector2IntValue = spacing;
                shouldSave = true;
            }

            SerializedProperty densityProperty = overlayProperty.FindPropertyRelative("m_density");
            m_density = densityProperty.floatValue;
            if (m_density < 0.0f || m_density > 1.0f)
            {
                m_density = Mathf.Clamp01(m_density);
                densityProperty.floatValue = m_density;
                shouldSave = true;
            }

            SerializedProperty rulesProperty = overlayProperty.FindPropertyRelative("m_rules");
            m_rules.Clear();
            for (int i = 0; i < rulesProperty.arraySize; i++)
            {
                SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(i);

                SerializedOverlayRule serializedRule = new SerializedOverlayRule();
                if (serializedRule.Update(ruleProperty))
                {
                    shouldSave = true;
                }

                m_rules.Add(serializedRule);
            }

            if (ValidateSize())
            {
                shouldSave = true;
            }

            return shouldSave;
        }

        public bool OnTilesDeleted(IEnumerable<SerializedTile> tiles)
        {
            bool changed = false;

            foreach (SerializedOverlayRule rule in m_rules)
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

            foreach (SerializedOverlayRule rule in m_rules)
            {
                if (rule.OnCategoriesDeleted(categories))
                {
                    changed = true;
                }
            }

            return changed;
        }

        public void SaveChanges(TileSet tileSet, SerializedProperty overlayProperty)
        {
            SerializedProperty idProperty = overlayProperty.FindPropertyRelative("m_id");
            idProperty.stringValue = m_id;

            SerializedProperty sizeProperty = overlayProperty.FindPropertyRelative("m_size");
            m_size.x = Mathf.Max(1, m_size.x);
            m_size.y = Mathf.Max(1, m_size.y);
            sizeProperty.vector2IntValue = m_size;

            SerializedProperty cellSizeProperty = overlayProperty.FindPropertyRelative("m_cellSize");
            m_cellSize.x = Mathf.Max(m_cellSize.x, m_size.x);
            m_cellSize.y = Mathf.Max(m_cellSize.y, m_size.y);
            cellSizeProperty.vector2IntValue = m_cellSize;

            SerializedProperty spacingProperty = overlayProperty.FindPropertyRelative("m_spacing");
            m_spacing.x = Mathf.Max(0, m_spacing.x);
            m_spacing.y = Mathf.Max(0, m_spacing.y);
            spacingProperty.vector2IntValue = m_spacing;

            SerializedProperty densityProperty = overlayProperty.FindPropertyRelative("m_density");
            densityProperty.floatValue = Mathf.Clamp01(m_density);

            SerializedProperty rulesProperty = overlayProperty.FindPropertyRelative("m_rules");
            rulesProperty.arraySize = m_rules.Count;
            for (int i = 0; i < m_rules.Count; i++)
            {
                SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(i);

                m_rules[i].SaveChanges(ruleProperty);
            }

            HashSet<SimpleTile> oldOverlayTiles = new HashSet<SimpleTile>();

            SerializedProperty overlayTilesProperty = overlayProperty.FindPropertyRelative("m_overlayTiles");
            for (int i = 0; i < overlayTilesProperty.arraySize; i++)
            {
                SimpleTile overlayTile = overlayTilesProperty.GetArrayElementAtIndex(i).objectReferenceValue as SimpleTile;
                if (overlayTile != null)
                {
                    oldOverlayTiles.Add(overlayTile);
                }
            }

            List<SimpleTile> newOverlayTiles = new List<SimpleTile>();
            int index = 0;
            foreach (SerializedOverlayRule rule in m_rules)
            {
                int x = index % m_size.x;
                int y = index / m_size.x;

                string id = $"overlay_{m_id}_{x}_{y}";

                SerializedTileOutput output = rule.output;

                SimpleTile overlayTile = oldOverlayTiles.FirstOrDefault(o => o.id == id);
                if (overlayTile != null)
                {
                    oldOverlayTiles.Remove(overlayTile);
                }
                else
                {
                    overlayTile = ScriptableObject.CreateInstance<SimpleTile>();
                    overlayTile.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(overlayTile, tileSet);
                }
                newOverlayTiles.Add(overlayTile);

                SerializedObject serializedObject = new SerializedObject(overlayTile);

                SerializedProperty tileIdProperty = serializedObject.FindProperty("m_id");
                tileIdProperty.stringValue = id;

                SerializedProperty colliderTypeProperty = serializedObject.FindProperty("m_colliderType");
                colliderTypeProperty.enumValueIndex = (int)UnityTile.ColliderType.None;

                SerializedProperty outputProperty = serializedObject.FindProperty("m_output");
                output.SaveChanges(outputProperty);

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                
                index++;
            }

            foreach (SimpleTile oldOverlayTile in oldOverlayTiles)
            {
                ScriptableObject.DestroyImmediate(oldOverlayTile, true);
            }

            overlayTilesProperty.arraySize = newOverlayTiles.Count;
            for (int i = 0; i < newOverlayTiles.Count; i++)
            {
                overlayTilesProperty.GetArrayElementAtIndex(i).objectReferenceValue = newOverlayTiles[i];
            }
        }

        public SerializedOverlay()
        {
            cellSize = Vector2Int.one;
            size = Vector2Int.one;
            spacing = Vector2Int.zero;
            density = 0.5f;
        }

        public SerializedOverlay(string id)
        {
            cellSize = Vector2Int.one;
            size = Vector2Int.one;
            spacing = Vector2Int.zero;
            density = 0.5f;

            m_id = id;
        }


        private bool ValidateSize(Vector2Int? oldSize = null)
        {
            if (oldSize.HasValue && m_rules.Count == oldSize.Value.x * oldSize.Value.y)
            {
                SerializedOverlayRule[] oldRules = m_rules.ToArray();

                m_rules = new List<SerializedOverlayRule>(new SerializedOverlayRule[m_size.x * m_size.y]);
                for (int x = 0; x < m_size.x; x++)
                {
                    for (int y = 0; y < m_size.y; y++)
                    {
                        SerializedOverlayRule rule = (x < oldSize.Value.x && y < oldSize.Value.y) ? oldRules[y * oldSize.Value.x + x] : new SerializedOverlayRule();
                        m_rules[y * m_size.x + x] = rule;
                    }
                }

                return true;
            }

            bool changed = false;

            while (m_rules.Count > m_size.x * m_size.y)
            {
                m_rules.RemoveAt(m_rules.Count - 1);
                changed = true;
            }
            while (m_rules.Count < m_size.x * m_size.y)
            {
                m_rules.Add(new SerializedOverlayRule());
                changed = true;
            }

            return changed;
        }
    }
}
