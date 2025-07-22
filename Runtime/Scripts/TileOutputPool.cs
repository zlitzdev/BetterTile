using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Zlitz.Extra2D.BetterTile
{
    internal class TileOutputPool
    {
        private List<Entry> m_entries = new List<Entry>();

        private List<TileOutputPool> m_parentPools = new List<TileOutputPool>();

        private float m_totalWeight = 0.0f;

        private List<Entry> m_validEntries;

        public ICollection<TileOutputPool> parentPools => m_parentPools;

        public void Add(TileOutput output, float weight)
        {
            Entry newEntry = new Entry(output, weight);
            m_entries.Add(newEntry);
            m_totalWeight += newEntry.weight;
        }

        public bool Sample(float randomValue, out TileOutput output)
        {
            output = default;

            if (m_validEntries == null)
            {
                m_validEntries = new List<Entry>();
            }
            else
            {
                m_validEntries.Clear();
            }

            float totalWeight = m_totalWeight;

            if (m_entries != null && m_entries.Count > 0)
            {
                m_validEntries.AddRange(m_entries);
            }

            foreach (TileOutputPool parent in m_parentPools)
            {
                if (parent.m_entries != null && parent.m_entries.Count > 0)
                {
                    totalWeight += parent.m_totalWeight;
                    m_validEntries.AddRange(parent.m_entries);
                }
            }

            if (m_validEntries.Count <= 0)
            {
                return false;
            }

            randomValue = Mathf.Clamp01(randomValue) * totalWeight;
            float current = 0.0f;

            bool found = false;
            foreach (Entry entry in m_validEntries)
            {
                current += entry.weight;
                if (current >= randomValue)
                {
                    output = entry.output;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                output = m_entries.Last().output;
            }

            return true;
        }

        private struct Entry
        {
            public float weight { get; private set; }

            public TileOutput output { get; private set; }

            public Entry(TileOutput output, float weight)
            {
                this.weight = Mathf.Max(0.0f, weight);
                this.output = output;
            }
        }
    }
}
