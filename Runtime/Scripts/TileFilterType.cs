namespace Zlitz.Extra2D.BetterTile
{
    public enum TileFilterType
    {
        Any,
        Tile,
        TileCategory,
        Decorator
    }

    public static class TileFilterTypes
    {
        public static readonly TileFilterType[] specialFilterTypes = new TileFilterType[]
        {
            TileFilterType.Decorator
        };
    }
}
