namespace GameCore.Units;

public class DeckCard
{
    public string            CardId           { get; set; } = "";
    public string            Name             { get; set; } = "";
    public SlotType          SlotType         { get; set; }
    public CardType          CardType         { get; set; }
    public TriggerCondition? TriggerCondition { get; set; }  // Trigger 카드만 사용
    public int               GaugeWeight      { get; set; } = 4; // Active 카드만 사용
    public string            SkillClass       { get; set; } = "";
    public string            Description      { get; set; } = "";
}
