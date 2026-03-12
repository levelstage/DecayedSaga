namespace GameCore.Battle;

public enum TileType { Normal, Wall, Pit, EffectTile }

public class Tile
{
    public int X { get; set; }
    public int Y { get; set; }
    public TileType Type { get; set; } = TileType.Normal;

    public Guid? OccupantId { get; set; }

    public bool IsOccupied => OccupantId.HasValue;
    public bool IsPassable => Type is TileType.Normal or TileType.EffectTile;
}
