using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal class TileCollection : VisualElement
    {
        public event Action<IEnumerable<Tile>> onValueChanged;

        private string m_label;

        private List<SerializedTile> m_options = new List<SerializedTile>();

        private HashSet<Tile> m_selected = new HashSet<Tile>();

        private Button m_button;

        public string label
        {
            get => m_label;
            set
            {
                if (m_label != value)
                {
                    m_label = value;
                    UpdateText();
                }
            }
        }

        public IEnumerable<SerializedTile> options
        {
            get => m_options;
            set
            {
                m_options.Clear();
                if (value != null)
                {
                    m_options.AddRange(value);
                    ValidateValue();
                }
            }
        }

        public IEnumerable<Tile> value
        {
            get => m_selected;
            set
            {
                HashSet<Tile> newSelected = value?.ToHashSet() ?? new HashSet<Tile>();
                if (!m_selected.SetEquals(newSelected))
                {
                    m_selected = newSelected;
                    onValueChanged?.Invoke(m_selected);
                    UpdateText();
                }
            }
        }

        public void SetValueWithoutNotify(IEnumerable<Tile> value)
        {
            m_selected = value?.ToHashSet() ?? new HashSet<Tile>();
            UpdateText();
        }

        public TileCollection()
        {
            Button button = new Button(() =>
            {
                Rect position = m_button.worldBound;
                Vector2 pos1 = GUIUtility.GUIToScreenPoint(position.min);
                Vector2 pos2 = GUIUtility.GUIToScreenPoint(position.max);

                Vector2 min = new Vector2(Mathf.Min(pos1.x, pos2.x), Mathf.Min(pos1.y, pos2.y));
                Vector2 max = new Vector2(Mathf.Max(pos1.x, pos2.x), Mathf.Max(pos1.y, pos2.y));

                position = new Rect(min, max - min);

                TileCollectionPopupWindow.Open(position, m_options, m_selected,
                    (t) =>
                    {
                        if (m_selected.Add(t))
                        {
                            onValueChanged?.Invoke(m_selected);
                            UpdateText();
                        }    
                    },
                    (t) =>
                    {
                        if (m_selected.Remove(t))
                        {
                            onValueChanged?.Invoke(m_selected);
                            UpdateText();
                        }
                    });
            });
            button.style.marginLeft   = 0.0f;
            button.style.marginTop    = 2.0f;
            button.style.marginBottom = 2.0f;
            Add(button);
            m_button = button;
            UpdateText();
        }
    
        private void ValidateValue()
        {
            HashSet<Tile> available = m_options.Select(t => t.tile).Where(t => t != null).ToHashSet();

            int count = m_selected.Count;
            m_selected.IntersectWith(available);
            if (m_selected.Count != count)
            {
                onValueChanged?.Invoke(m_selected);
                UpdateText();
            }
        }

        private void UpdateText()
        {
            m_button.text = string.IsNullOrWhiteSpace(m_label)
                ? $"{m_selected.Count} tile(s)"
                : $"{m_label}: {m_selected.Count} tile(s)";
        }
    }
}
