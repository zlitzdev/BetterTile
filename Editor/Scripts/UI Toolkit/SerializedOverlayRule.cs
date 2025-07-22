using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace Zlitz.Extra2D.BetterTile
{
    [Serializable]
    internal class SerializedOverlayRule
    {
        [SerializeField]
        private SerializedTileOutput m_output = new SerializedTileOutput();

        [SerializeField]
        private SerializedTileFilter m_c = new SerializedTileFilter();

        [SerializeField]
        private SerializedTileFilter m_nx = new SerializedTileFilter();

        [SerializeField]
        private SerializedTileFilter m_px = new SerializedTileFilter();

        [SerializeField]
        private SerializedTileFilter m_ny = new SerializedTileFilter();

        [SerializeField]
        private SerializedTileFilter m_py = new SerializedTileFilter();

        [SerializeField]
        private SerializedTileFilter m_nxny = new SerializedTileFilter();

        [SerializeField]
        private SerializedTileFilter m_pxny = new SerializedTileFilter();

        [SerializeField]
        private SerializedTileFilter m_nxpy = new SerializedTileFilter();

        [SerializeField]
        private SerializedTileFilter m_pxpy = new SerializedTileFilter();

        public SerializedTileOutput output => m_output;

        public SerializedTileFilter c => m_c;

        public SerializedTileFilter nx => m_nx;

        public SerializedTileFilter px => m_px;

        public SerializedTileFilter ny => m_ny;

        public SerializedTileFilter py => m_py;

        public SerializedTileFilter nxny => m_nxny;

        public SerializedTileFilter pxny => m_pxny;

        public SerializedTileFilter nxpy => m_nxpy;

        public SerializedTileFilter pxpy => m_pxpy;

        public bool Update(SerializedProperty overlayRuleProperty)
        {
            bool shouldSave = false;

            SerializedProperty outputProperty = overlayRuleProperty.FindPropertyRelative("m_output");
            if (m_output.Update(outputProperty))
            {
                shouldSave = true;
            }

            SerializedProperty cProperty = overlayRuleProperty.FindPropertyRelative("m_c");
            if (m_c.Update(cProperty))
            {
                shouldSave = true;
            }

            SerializedProperty nxProperty = overlayRuleProperty.FindPropertyRelative("m_nx");
            if (m_nx.Update(nxProperty))
            {
                shouldSave = true;
            }

            SerializedProperty pxProperty = overlayRuleProperty.FindPropertyRelative("m_px");
            if (m_px.Update(pxProperty))
            {
                shouldSave = true;
            }

            SerializedProperty nyProperty = overlayRuleProperty.FindPropertyRelative("m_ny");
            if (m_ny.Update(nyProperty))
            {
                shouldSave = true;
            }

            SerializedProperty pyProperty = overlayRuleProperty.FindPropertyRelative("m_py");
            if (m_py.Update(pyProperty))
            {
                shouldSave = true;
            }

            SerializedProperty nxnyProperty = overlayRuleProperty.FindPropertyRelative("m_nxny");
            if (m_nxny.Update(nxnyProperty))
            {
                shouldSave = true;
            }

            SerializedProperty pxnyProperty = overlayRuleProperty.FindPropertyRelative("m_pxny");
            if (m_pxny.Update(pxnyProperty))
            {
                shouldSave = true;
            }

            SerializedProperty nxpyProperty = overlayRuleProperty.FindPropertyRelative("m_nxpy");
            if (m_nxpy.Update(nxpyProperty))
            {
                shouldSave = true;
            }

            SerializedProperty pxpyProperty = overlayRuleProperty.FindPropertyRelative("m_pxpy");
            if (m_pxpy.Update(pxpyProperty))
            {
                shouldSave = true;
            }

            return shouldSave;
        }

        public bool OnTilesDeleted(IEnumerable<SerializedTile> tiles)
        {
            bool changed = false;

            if (m_c.OnTilesDeleted(tiles))
            {
                changed = true;
            }
            if (m_nx.OnTilesDeleted(tiles))
            {
                changed = true;
            }
            if (m_px.OnTilesDeleted(tiles))
            {
                changed = true;
            }
            if (m_ny.OnTilesDeleted(tiles))
            {
                changed = true;
            }
            if (m_py.OnTilesDeleted(tiles))
            {
                changed = true;
            }
            if (m_nxny.OnTilesDeleted(tiles))
            {
                changed = true;
            }
            if (m_pxny.OnTilesDeleted(tiles))
            {
                changed = true;
            }
            if (m_nxpy.OnTilesDeleted(tiles))
            {
                changed = true;
            }
            if (m_pxpy.OnTilesDeleted(tiles))
            {
                changed = true;
            }

            return changed;
        }

        public bool OnCategoriesDeleted(IEnumerable<SerializedCategory> categories)
        {
            bool changed = false;

            if (m_c.OnCategoriesDeleted(categories))
            {
                changed = true;
            }
            if (m_nx.OnCategoriesDeleted(categories))
            {
                changed = true;
            }
            if (m_px.OnCategoriesDeleted(categories))
            {
                changed = true;
            }
            if (m_ny.OnCategoriesDeleted(categories))
            {
                changed = true;
            }
            if (m_py.OnCategoriesDeleted(categories))
            {
                changed = true;
            }
            if (m_nxny.OnCategoriesDeleted(categories))
            {
                changed = true;
            }
            if (m_pxny.OnCategoriesDeleted(categories))
            {
                changed = true;
            }
            if (m_nxpy.OnCategoriesDeleted(categories))
            {
                changed = true;
            }
            if (m_pxpy.OnCategoriesDeleted(categories))
            {
                changed = true;
            }

            return changed;
        }

        public void SaveChanges(SerializedProperty overlayRuleProperty)
        {
            SerializedProperty outputProperty = overlayRuleProperty.FindPropertyRelative("m_output");
            m_output.SaveChanges(outputProperty);

            SerializedProperty cProperty = overlayRuleProperty.FindPropertyRelative("m_c");
            m_c.SaveChanges(cProperty);

            SerializedProperty nxProperty = overlayRuleProperty.FindPropertyRelative("m_nx");
            m_nx.SaveChanges(nxProperty);

            SerializedProperty pxProperty = overlayRuleProperty.FindPropertyRelative("m_px");
            m_px.SaveChanges(pxProperty);

            SerializedProperty nyProperty = overlayRuleProperty.FindPropertyRelative("m_ny");
            m_ny.SaveChanges(nyProperty);

            SerializedProperty pyProperty = overlayRuleProperty.FindPropertyRelative("m_py");
            m_py.SaveChanges(pyProperty);

            SerializedProperty nxnyProperty = overlayRuleProperty.FindPropertyRelative("m_nxny");
            m_nxny.SaveChanges(nxnyProperty);

            SerializedProperty pxnyProperty = overlayRuleProperty.FindPropertyRelative("m_pxny");
            m_pxny.SaveChanges(pxnyProperty);

            SerializedProperty nxpyProperty = overlayRuleProperty.FindPropertyRelative("m_nxpy");
            m_nxpy.SaveChanges(nxpyProperty);

            SerializedProperty pxpyProperty = overlayRuleProperty.FindPropertyRelative("m_pxpy");
            m_pxpy.SaveChanges(pxpyProperty);
        }
    }
}
