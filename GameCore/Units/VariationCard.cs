namespace GameCore.Units;

public class VariationCard
{
    public string     CardId        { get; set; } = "";
    public string     Name          { get; set; } = "";
    public ActionType VariationType { get; set; }
    public string     SkillClass    { get; set; } = "";
    public string     Description   { get; set; } = "";
}
