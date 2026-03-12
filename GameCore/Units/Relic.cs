using GameCore.Units.Movement;

namespace GameCore.Units;

public class Relic : Equipment
{
    public string SoulName { get; set; } = "";
    public ChessPieceType PieceType { get; set; }
    public bool IsBound { get; set; } = false;
    public string GolemSprite { get; set; } = "";
}
