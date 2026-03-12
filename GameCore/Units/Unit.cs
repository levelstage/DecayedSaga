namespace GameCore.Units;

public abstract class Unit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";

    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public bool IsAlive => Hp > 0;

    public int X { get; set; }
    public int Y { get; set; }

    public Equipment? WeaponSlot { get; set; }
    public Equipment? ArmorSlot { get; set; }
    public Equipment? AccessorySlot { get; set; }

    public float ActionGauge { get; set; } = 0f;
}
