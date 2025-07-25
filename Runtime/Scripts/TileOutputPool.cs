using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

namespace Zlitz.Extra2D.BetterTile
{
    internal class TileOutputPool
    {
        private List<Entry> m_entries = new List<Entry>();

        private List<int> m_alternatingIndices = new List<int>();

        private List<TileOutputPool> m_parentPools = new List<TileOutputPool>();

        private float m_totalWeight = 0.0f;

        private List<Entry> m_validEntries;

        public ICollection<TileOutputPool> parentPools => m_parentPools;

        public void Add(TileOutput output, float weight, int alternatingIndex)
        {
            if (!m_alternatingIndices.Contains(alternatingIndex))
            {
                int insertIndex = 0;
                while (insertIndex < m_alternatingIndices.Count && m_alternatingIndices[insertIndex] < alternatingIndex)
                {
                    insertIndex++;
                }
                m_alternatingIndices.Insert(insertIndex, alternatingIndex);
            }

            Entry newEntry = new Entry(output, weight, alternatingIndex);
            m_entries.Add(newEntry);
            m_totalWeight += newEntry.weight;
        }

        public bool Sample(float randomValue, int alternatingIndex, out TileOutput output)
        {
            output = default;

            (m_validEntries ??= new List<Entry>()).Clear();

            float totalWeight = 0.0f;

            if (m_entries != null && m_entries.Count > 0)
            {
                int altIndex = Remap(alternatingIndex);

                IEnumerable<Entry> valid = m_entries.Where(e => e.alternatingIndex == altIndex);
                totalWeight += valid.Sum(e => e.weight);

                m_validEntries.AddRange(valid);
            }

            foreach (TileOutputPool parent in m_parentPools)
            {
                if (parent.m_entries != null && parent.m_entries.Count > 0)
                {
                    int altIndex = parent.Remap(alternatingIndex);

                    IEnumerable<Entry> valid = parent.m_entries.Where(e => e.alternatingIndex == altIndex);
                    totalWeight += valid.Sum(e => e.weight);

                    m_validEntries.AddRange(valid);
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

        private int Remap(int alternatingIndex)
        {
            alternatingIndex -= Mathf.FloorToInt(alternatingIndex / (float)m_alternatingIndices.Count) * m_alternatingIndices.Count;
            alternatingIndex = m_alternatingIndices[alternatingIndex];
            return alternatingIndex;
        }

        private struct Entry
        {
            public float weight { get; private set; }

            public int alternatingIndex { get; private set; }

            public TileOutput output { get; private set; }

            public Entry(TileOutput output, float weight, int alternatingIndex)
            {
                this.weight = Mathf.Max(0.0f, weight);
                this.alternatingIndex = alternatingIndex;
                this.output = output;
            }
        }
    }
}
