namespace GameCore.Units;

public enum SlotType { Weapon, Armor, Accessory }
public enum EquipmentTier { Farmer = 1, Rogue, Mercenary, Knight, Hero }

public class Equipment
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public SlotType SlotType { get; set; }
    public EquipmentTier Tier { get; set; }

    public PublicCard? PublicCard { get; set; }
    public List<VariationCard> VariationCards { get; set; } = new();

    // 내구도 토큰 (브레이크 기준)
    public int DurabilityToken { get; set; }
    public int MaxDurabilityToken { get; set; }
    public bool IsBroken => DurabilityToken <= 0;

    // 강화 토큰: 무기 슬롯 전용. 공격력 +1/토큰
    public int EnhancementToken { get; set; }

    // 방호 토큰: 장갑 슬롯 전용. 피해 -1/토큰
    public int GuardToken { get; set; }

    // 브레이크 수복 비용 (변주 N장)
    public int RepairCost { get; set; }
}
