namespace GameCore.Units;

/// <summary>
/// 장비 컨테이너. 식별 정보 + PublicCard(인게임 상태) + DeckCards(행동 덱).
/// 브레이크/토큰 등 인게임 상태는 PublicCard가 담당한다.
/// </summary>
public class Equipment
{
    public string        Id       { get; set; } = "";
    public string        Name     { get; set; } = "";
    public SlotType      SlotType { get; set; }
    public EquipmentTier Tier     { get; set; }

    public PublicCard?    PublicCard { get; set; }
    public List<DeckCard> DeckCards  { get; set; } = new();

    // ── PublicCard 위임 ───────────────────────────────────────────────
    public bool IsBroken   => PublicCard?.IsBroken  ?? false;
    public bool TickRepair() => PublicCard?.TickRepair() ?? false;
    public void OnBreak()    => PublicCard?.OnBreak();
}
