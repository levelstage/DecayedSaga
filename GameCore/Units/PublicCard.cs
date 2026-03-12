namespace GameCore.Units;

public enum ActionType { Attack, Defense, Accessory }

public class PublicCard
{
    public string       CardId        { get; set; } = "";
    public string       Name          { get; set; } = "";
    public ActionType   ActionType    { get; set; }
    public int          GaugeWeight   { get; set; } = 4;
    public string       SkillClass    { get; set; } = "";
    public List<string> PassiveTokens { get; set; } = new();
    public int          RepairCost    { get; set; }
    public bool         HasMoveOption { get; set; } = false;
    public string       Description   { get; set; } = "";
}
