using System;

using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal class TileRulePaintable : VisualElement
    {
        private TileSetEditorWindow.PaintContext m_context;

        private SerializedTileRule m_serializedRule;

        private TileIdentityPaintable m_identity;

        private TileFilterPaintable m_nx;
        private TileFilterPaintable m_px;
        private TileFilterPaintable m_ny;
        private TileFilterPaintable m_py;
        private TileFilterPaintable m_nxny;
        private TileFilterPaintable m_pxny;
        private TileFilterPaintable m_nxpy;
        private TileFilterPaintable m_pxpy;

        private WeightPaintable m_weight;

        private AlternatingIndexPaintable m_alternatingIndex;

        public SerializedTileRule serializedRule => m_serializedRule;

        public bool isStatic
        {
            get => m_serializedRule.isStatic;
            set
            {
                m_serializedRule.isStatic = value;

                float borderWidth = value ? 2.0f : 0.0f;

                style.borderLeftWidth   = borderWidth;
                style.borderRightWidth  = borderWidth;
                style.borderTopWidth    = borderWidth;
                style.borderBottomWidth = borderWidth;
            }
        }

        public void Bind(SerializedTileRule serializedRule, Action onChanges)
        {
            m_serializedRule = serializedRule;

            isStatic = m_serializedRule.isStatic;

            m_identity.Bind(m_serializedRule.identity, onChanges);

            m_nx.Bind(m_serializedRule.nx, onChanges);
            m_px.Bind(m_serializedRule.px, onChanges);
            m_ny.Bind(m_serializedRule.ny, onChanges);
            m_py.Bind(m_serializedRule.py, onChanges);
            m_nxny.Bind(m_serializedRule.nxny, onChanges);
            m_pxny.Bind(m_serializedRule.pxny, onChanges);
            m_nxpy.Bind(m_serializedRule.nxpy, onChanges);
            m_pxpy.Bind(m_serializedRule.pxpy, onChanges);

            m_weight.Bind(m_serializedRule, onChanges);

            m_alternatingIndex.Bind(m_serializedRule, onChanges);
        }

        public void OnTileColorChanged(SerializedTile tile)
        {
            m_identity.OnTileColorChanged(tile);

            m_nx.OnTileColorChanged(tile);
            m_px.OnTileColorChanged(tile);
            m_ny.OnTileColorChanged(tile);
            m_py.OnTileColorChanged(tile);
            m_nxny.OnTileColorChanged(tile);
            m_pxny.OnTileColorChanged(tile);
            m_nxpy.OnTileColorChanged(tile);
            m_pxpy.OnTileColorChanged(tile);
        }

        public void OnCategoryColorChanged(SerializedCategory category)
        {
            m_nx.OnCategoryColorChanged(category);
            m_px.OnCategoryColorChanged(category);
            m_ny.OnCategoryColorChanged(category);
            m_py.OnCategoryColorChanged(category);
            m_nxny.OnCategoryColorChanged(category);
            m_pxny.OnCategoryColorChanged(category);
            m_nxpy.OnCategoryColorChanged(category);
            m_pxpy.OnCategoryColorChanged(category);
        }

        public void OnFilterColorChanged(SerializedSpecialFilter filter)
        {
            m_identity.OnFilterColorChanged(filter);

            m_nx.OnFilterColorChanged(filter);
            m_px.OnFilterColorChanged(filter);
            m_ny.OnFilterColorChanged(filter);
            m_py.OnFilterColorChanged(filter);
            m_nxny.OnFilterColorChanged(filter);
            m_pxny.OnFilterColorChanged(filter);
            m_nxpy.OnFilterColorChanged(filter);
            m_pxpy.OnFilterColorChanged(filter);
        }

        public TileRulePaintable(TileSetEditorWindow.PaintContext context)
        {
            m_context = context;

            style.borderLeftColor   = UIColors.yellow;
            style.borderRightColor  = UIColors.yellow;
            style.borderTopColor    = UIColors.yellow;
            style.borderBottomColor = UIColors.yellow;

            TileIdentityPaintable identity = new TileIdentityPaintable(context);
            identity.style.position = Position.Absolute;
            identity.style.left   = Length.Percent(25.0f);
            identity.style.top    = Length.Percent(25.0f);
            identity.style.width  = Length.Percent(50.0f);
            identity.style.height = Length.Percent(50.0f);
            Add(identity);
            m_identity = identity;

            TileFilterPaintable nx = new TileFilterPaintable(context);
            nx.style.position = Position.Absolute;
            nx.style.left   = Length.Percent(0.0f);
            nx.style.top    = Length.Percent(25.0f);
            nx.style.width  = Length.Percent(25.0f);
            nx.style.height = Length.Percent(50.0f);
            Add(nx);
            m_nx = nx;

            TileFilterPaintable px = new TileFilterPaintable(context);
            px.style.position = Position.Absolute;
            px.style.left   = Length.Percent(75.0f);
            px.style.top    = Length.Percent(25.0f);
            px.style.width  = Length.Percent(25.0f);
            px.style.height = Length.Percent(50.0f);
            Add(px);
            m_px = px;

            TileFilterPaintable ny = new TileFilterPaintable(context);
            ny.style.position = Position.Absolute;
            ny.style.left   = Length.Percent(25.0f);
            ny.style.top    = Length.Percent(75.0f);
            ny.style.width  = Length.Percent(50.0f);
            ny.style.height = Length.Percent(25.0f);
            Add(ny);
            m_ny = ny;

            TileFilterPaintable py = new TileFilterPaintable(context);
            py.style.position = Position.Absolute;
            py.style.left   = Length.Percent(25.0f);
            py.style.top    = Length.Percent(0.0f);
            py.style.width  = Length.Percent(50.0f);
            py.style.height = Length.Percent(25.0f);
            Add(py);
            m_py = py;

            TileFilterPaintable nxny = new TileFilterPaintable(context);
            nxny.style.position = Position.Absolute;
            nxny.style.left   = Length.Percent(0.0f);
            nxny.style.top    = Length.Percent(75.0f);
            nxny.style.width  = Length.Percent(25.0f);
            nxny.style.height = Length.Percent(25.0f);
            Add(nxny);
            m_nxny = nxny;

            TileFilterPaintable pxny = new TileFilterPaintable(context);
            pxny.style.position = Position.Absolute;
            pxny.style.left   = Length.Percent(75.0f);
            pxny.style.top    = Length.Percent(75.0f);
            pxny.style.width  = Length.Percent(25.0f);
            pxny.style.height = Length.Percent(25.0f);
            Add(pxny);
            m_pxny = pxny;

            TileFilterPaintable nxpy = new TileFilterPaintable(context);
            nxpy.style.position = Position.Absolute;
            nxpy.style.left   = Length.Percent(0.0f);
            nxpy.style.top    = Length.Percent(0.0f);
            nxpy.style.width  = Length.Percent(25.0f);
            nxpy.style.height = Length.Percent(25.0f);
            Add(nxpy);
            m_nxpy = nxpy;

            TileFilterPaintable pxpy = new TileFilterPaintable(context);
            pxpy.style.position = Position.Absolute;
            pxpy.style.left   = Length.Percent(75.0f);
            pxpy.style.top    = Length.Percent(0.0f);
            pxpy.style.width  = Length.Percent(25.0f);
            pxpy.style.height = Length.Percent(25.0f);
            Add(pxpy);
            m_pxpy = pxpy;

            WeightPaintable weight = new WeightPaintable(context);
            weight.style.position = Position.Absolute;
            weight.style.left   = Length.Percent(0.0f);
            weight.style.top    = Length.Percent(0.0f);
            weight.style.width  = Length.Percent(100.0f);
            weight.style.height = Length.Percent(100.0f);
            Add(weight);
            m_weight = weight;

            AlternatingIndexPaintable alternatingIndex = new AlternatingIndexPaintable(context);
            alternatingIndex.style.position = Position.Absolute;
            alternatingIndex.style.left   = Length.Percent(0.0f);
            alternatingIndex.style.top    = Length.Percent(0.0f);
            alternatingIndex.style.width  = Length.Percent(100.0f);
            alternatingIndex.style.height = Length.Percent(100.0f);
            Add(alternatingIndex);
            m_alternatingIndex = alternatingIndex;
        }
    }
}
