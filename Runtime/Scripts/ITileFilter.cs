using UnityEngine.Tilemaps;

namespace Zlitz.Extra2D.BetterTile
{
    public interface ITileFilter
    {
        bool Match(Tile sourceTile, TileBase tile);

        bool IsGeneralizedOf(ITileFilter other);
    }
}
