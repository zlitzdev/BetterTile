using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Zlitz.Extra2D.BetterTile
{
    internal class OverlayRulePaintable : VisualElement
    {
        private TileSetEditorWindow.PaintContext m_context;

        private SerializedOverlayRule m_serializedRule;

        private VisualElement m_highlight;

        private TileFilterPaintable m_c;
        private TileFilterPaintable m_nx;
        private TileFilterPaintable m_px;
        private TileFilterPaintable m_ny;
        private TileFilterPaintable m_py;
        private TileFilterPaintable m_nxny;
        private TileFilterPaintable m_pxny;
        private TileFilterPaintable m_nxpy;
        private TileFilterPaintable m_pxpy;

        public SerializedOverlayRule serializedRule => m_serializedRule;

        public VisualElement highlight => m_highlight;

        public void Bind(SerializedOverlayRule serializedRule, Action onChanges)
        {
            m_serializedRule = serializedRule;

            m_c.Bind(m_serializedRule.c, onChanges);
            m_nx.Bind(m_serializedRule.nx, onChanges);
            m_px.Bind(m_serializedRule.px, onChanges);
            m_ny.Bind(m_serializedRule.ny, onChanges);
            m_py.Bind(m_serializedRule.py, onChanges);
            m_nxny.Bind(m_serializedRule.nxny, onChanges);
            m_pxny.Bind(m_serializedRule.pxny, onChanges);
            m_nxpy.Bind(m_serializedRule.nxpy, onChanges);
            m_pxpy.Bind(m_serializedRule.pxpy, onChanges);

            SetBackground(m_serializedRule.output.sprite);
        }

        public void OnTileColorChanged(SerializedTile tile)
        {
            m_c.OnTileColorChanged(tile);
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
            m_c.OnCategoryColorChanged(category);
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
            m_c.OnFilterColorChanged(filter);
            m_nx.OnFilterColorChanged(filter);
            m_px.OnFilterColorChanged(filter);
            m_ny.OnFilterColorChanged(filter);
            m_py.OnFilterColorChanged(filter);
            m_nxny.OnFilterColorChanged(filter);
            m_pxny.OnFilterColorChanged(filter);
            m_nxpy.OnFilterColorChanged(filter);
            m_pxpy.OnFilterColorChanged(filter);
        }

        public void SetBackground(Sprite sprite)
        {
            if (sprite == null)
            {
                style.backgroundImage = null;
                return;
            }

            Texture2D texture = sprite.texture;
            float unitSize = sprite.pixelsPerUnit;

            Rect spriteRect = new Rect(
                sprite.rect.x + sprite.pivot.x - unitSize * 0.5f,
                sprite.rect.y + sprite.pivot.y - unitSize * 0.5f,
                unitSize,
                unitSize
            );

            style.backgroundImage = texture;

            style.backgroundSize = new BackgroundSize(
                Length.Percent((100.0f * texture.width) / spriteRect.width),
                Length.Percent((100.0f * texture.height) / spriteRect.height)
            );

            style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Left, Length.Percent(spriteRect.x * 100.0f / (texture.width - spriteRect.width)));
            style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Bottom, Length.Percent(spriteRect.y * 100.0f / (texture.height - spriteRect.height)));
        }

        public OverlayRulePaintable(TileSetEditorWindow.PaintContext context)
        {
            m_context = context;

            style.borderLeftColor   = UIColors.yellow;
            style.borderRightColor  = UIColors.yellow;
            style.borderTopColor    = UIColors.yellow;
            style.borderBottomColor = UIColors.yellow;

            VisualElement highlight = new VisualElement();
            highlight.style.position = Position.Absolute;
            highlight.style.left   = Length.Percent(0.0f);
            highlight.style.top    = Length.Percent(0.0f);
            highlight.style.width  = Length.Percent(0.0f);
            highlight.style.height = Length.Percent(0.0f);
            m_highlight = highlight;

            TileFilterPaintable c = new TileFilterPaintable(context);
            c.allowInverted = false;
            c.style.position = Position.Absolute;
            c.style.left   = Length.Percent(25.0f);
            c.style.top    = Length.Percent(25.0f);
            c.style.width  = Length.Percent(50.0f);
            c.style.height = Length.Percent(50.0f);
            Add(c);
            m_c = c;

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
        }
    }
}
