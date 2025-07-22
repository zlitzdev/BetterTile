using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zlitz.Extra2D.BetterTile
{
    internal struct TileContext
    {
        public TileBase nx { get; private set; }

        public TileBase px { get; private set; }

        public TileBase ny { get; private set; }

        public TileBase py { get; private set; }

        public TileBase nxny { get; private set; }

        public TileBase pxny { get; private set; }

        public TileBase nxpy { get; private set; }

        public TileBase pxpy { get; private set; }

        public bool empty =>
            nx == null &&
            px == null &&
            ny == null &&
            py == null &&
            nxny == null &&
            pxny == null &&
            nxpy == null &&
            pxpy == null;

        public TileContext(ITilemap tilemap, Vector3Int position, bool includeCenter = false)
        {
            nx   = tilemap.GetTile(position + new Vector3Int(-1,  0));
            px   = tilemap.GetTile(position + new Vector3Int( 1,  0));
            ny   = tilemap.GetTile(position + new Vector3Int( 0, -1));
            py   = tilemap.GetTile(position + new Vector3Int( 0,  1));
            nxny = tilemap.GetTile(position + new Vector3Int(-1, -1));
            pxny = tilemap.GetTile(position + new Vector3Int( 1, -1));
            nxpy = tilemap.GetTile(position + new Vector3Int(-1,  1));
            pxpy = tilemap.GetTile(position + new Vector3Int( 1,  1));
        }
    }
}
