namespace GameCore.Units.Movement;

public enum ChessPieceType { King, Knight, Rook, Bishop, Queen }

public static class MovementPattern
{
    public static int GaugeCost(ChessPieceType piece) => piece switch
    {
        ChessPieceType.King   => 2,
        ChessPieceType.Knight => 2,
        ChessPieceType.Rook   => 4,
        ChessPieceType.Bishop => 4,
        ChessPieceType.Queen  => 6,
        _ => 4
    };

    public static bool IsValidMove(ChessPieceType piece, int fromX, int fromY, int toX, int toY)
    {
        int dx = toX - fromX;
        int dy = toY - fromY;
        if (dx == 0 && dy == 0) return false;

        return piece switch
        {
            ChessPieceType.King   => Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1,
            ChessPieceType.Knight => (Math.Abs(dx) == 2 && Math.Abs(dy) == 1) ||
                                     (Math.Abs(dx) == 1 && Math.Abs(dy) == 2),
            ChessPieceType.Rook   => dx == 0 || dy == 0,
            ChessPieceType.Bishop => Math.Abs(dx) == Math.Abs(dy),
            ChessPieceType.Queen  => dx == 0 || dy == 0 || Math.Abs(dx) == Math.Abs(dy),
            _ => false
        };
    }

    public static bool IgnoresObstacles(ChessPieceType piece) => piece == ChessPieceType.Knight;
}
