using System.Collections.Generic;

using UnityEngine.Tilemaps;

namespace Zlitz.Extra2D.BetterTile
{
    internal class RuleSet
    {
        private readonly List<Entry> m_entries = new List<Entry>();

        public void Clear()
        {
            m_entries.Clear();
        }

        public void Insert(TileRule tileRule, TileBase assignedTile = null)
        {
            Rule rule = new Rule()
            {
                nx   = tileRule.nx,
                px   = tileRule.px,
                ny   = tileRule.ny,
                py   = tileRule.py,
                nxny = tileRule.nxny,
                pxny = tileRule.pxny,
                nxpy = tileRule.nxpy,
                pxpy = tileRule.pxpy,
            };

            Entry matchedEntry = null;
            foreach (Entry entry in m_entries)
            {
                if (entry.rule.IsSame(rule))
                {
                    matchedEntry = entry;
                    break;
                }
            }

            if (matchedEntry == null)
            {
                matchedEntry = CreateEntry(rule);
            }

            TileOutput output = tileRule.output;
            output.assignedTile = assignedTile;

            matchedEntry.outputs.Add(output, tileRule.weight);
            if (tileRule.isStatic)
            {
                matchedEntry.staticOutputs.Add(output, tileRule.weight);
            }
        }

        public bool Sample(TileContext context, float randomValue, out TileOutput output)
        {
            output = default;

            Entry matchedEntry = null;
            foreach (Entry entry in m_entries)
            {
                if (entry.rule.MatchContext(context) && (matchedEntry == null || matchedEntry.rule.Contains(entry.rule) || matchedEntry.generalizationLevel < entry.generalizationLevel))
                {
                    matchedEntry = entry;
                }
            }

            if (matchedEntry != null)
            {
                return matchedEntry.outputs.Sample(randomValue, out output);
            }

            return false;
        }

        private Entry CreateEntry(Rule rule)
        {
            Entry entry = new Entry(rule);

            foreach (Entry existingEntry in m_entries)
            {
                if (existingEntry.rule.Contains(entry.rule) && !existingEntry.rule.IsSame(entry.rule))
                {
                    entry.outputs.parentPools.Add(existingEntry.staticOutputs);
                }
            }

            m_entries.Add(entry);
            return entry;
        }

        private class Entry
        {
            public Rule rule { get; private set; }

            public int generalizationLevel { get; private set; }

            public readonly TileOutputPool outputs = new TileOutputPool();

            public readonly TileOutputPool staticOutputs = new TileOutputPool();

            public Entry(Rule rule)
            {
                this.rule = rule;
                this.generalizationLevel = rule.generalizationLevel;
            }
        }

        private struct Rule
        {
            public TileFilter nx;
            public TileFilter px;
            public TileFilter ny;
            public TileFilter py;
            public TileFilter nxny;
            public TileFilter pxny;
            public TileFilter nxpy;
            public TileFilter pxpy;

            public int generalizationLevel =>
                nx.generalizationLevel * 1 +
                px.generalizationLevel * 3 +
                ny.generalizationLevel * 9 +
                py.generalizationLevel * 27 +
                nxny.generalizationLevel * 81 +
                pxny.generalizationLevel * 243 +
                nxpy.generalizationLevel * 729 +
                pxpy.generalizationLevel * 2187;

            public bool IsSame(Rule other)
            {
                return
                    nx.IsSame(other.nx) &&
                    px.IsSame(other.px) &&
                    ny.IsSame(other.ny) &&
                    py.IsSame(other.py) &&
                    nxny.IsSame(other.nxny) &&
                    pxny.IsSame(other.pxny) &&
                    nxpy.IsSame(other.nxpy) &&
                    pxpy.IsSame(other.pxpy);
            }

            public bool Contains(Rule other)
            {
                return
                    nx.Contains(other.nx) &&
                    px.Contains(other.px) &&
                    ny.Contains(other.ny) &&
                    py.Contains(other.py) &&
                    nxny.Contains(other.nxny) &&
                    pxny.Contains(other.pxny) &&
                    nxpy.Contains(other.nxpy) &&
                    pxpy.Contains(other.pxpy);
            }

            public bool MatchContext(TileContext context)
            {
                return
                    nx.MatchTile(context.nx) &&
                    px.MatchTile(context.px) &&
                    ny.MatchTile(context.ny) &&
                    py.MatchTile(context.py) &&
                    nxny.MatchTile(context.nxny) &&
                    pxny.MatchTile(context.pxny) &&
                    nxpy.MatchTile(context.nxpy) &&
                    pxpy.MatchTile(context.pxpy);
            }
        }
    }
}
