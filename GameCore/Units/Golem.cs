namespace GameCore.Units;

public class Golem : Unit
{
    public Golem() => Name = "이름 없는 인형";

    public int WeaponSlotLevel { get; set; } = 1;
    public int ArmorSlotLevel { get; set; } = 1;
    public int AccessorySlotLevel { get; set; } = 1;

    public int Level => Math.Max(WeaponSlotLevel, Math.Max(ArmorSlotLevel, AccessorySlotLevel));

    public float AccumulatedTurns { get; set; } = 0f;
}
