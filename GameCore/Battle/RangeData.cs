namespace GameCore.Battle;

public enum RangeType { Linear, Diagonal, All, Self }

public class RangeData
{
    public RangeType Type { get; set; }
    public int Range { get; set; }

    public static RangeData Self() => new() { Type = RangeType.Self, Range = 0 };
    public static RangeData Linear(int range) => new() { Type = RangeType.Linear, Range = range };
    public static RangeData Diagonal(int range) => new() { Type = RangeType.Diagonal, Range = range };
    public static RangeData All(int range) => new() { Type = RangeType.All, Range = range };
}
