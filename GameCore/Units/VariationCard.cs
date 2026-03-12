using GameCore.Battle;
using GameCore.Behaviors;

namespace GameCore.Units;

public abstract class VariationCard : IActionStep
{
    public string     CardId        { get; set; } = "";
    public string     Name          { get; set; } = "";
    public ActionType VariationType { get; set; }
    public bool       IsIndependent { get; set; } = false;
    public string     Description   { get; set; } = "";

    // ── IActionStep ───────────────────────────────────────────────────

    public virtual bool RequiresMoveInput => false;

    public abstract Behavior? CreateBehavior(ActionRequest request, BattleManager manager);
}
