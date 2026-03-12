namespace GameCore.Battle;

public class BattleGrid
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    private readonly Tile[,] _tiles;

    public BattleGrid(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new Tile[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _tiles[x, y] = new Tile { X = x, Y = y };
    }

    public Tile? GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return null;
        return _tiles[x, y];
    }

    public bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public IEnumerable<Tile> GetLinePath(int fromX, int fromY, int toX, int toY)
    {
        int dx = Math.Sign(toX - fromX);
        int dy = Math.Sign(toY - fromY);
        int cx = fromX + dx, cy = fromY + dy;
        while (cx != toX || cy != toY)
        {
            var tile = GetTile(cx, cy);
            if (tile != null) yield return tile;
            cx += dx;
            cy += dy;
        }
    }
}
