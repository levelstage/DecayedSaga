using GameCore.Battle;
using GameCore.Battle.Behaviors;
using GameCore.Behaviors;
using GameCore.Scripting;

namespace GameCore.Units;

public enum ActionType { Attack, Defense, Accessory }

public class PublicCard : IActionStep
{
    public string     CardId        { get; set; } = "";
    public string     Name          { get; set; } = "";
    public ActionType ActionType    { get; set; }

    public int          GaugeWeight   { get; set; } = 4;
    public string       SkillClass    { get; set; } = "";
    public List<string> PassiveTokens { get; set; } = new();
    public int          RepairCost    { get; set; }
    public bool         HasMoveOption { get; set; } = false;
    public string       Description   { get; set; } = "";

    // ── IActionStep ───────────────────────────────────────────────────

    bool IActionStep.IsIndependent     => false; // 공개카드는 턴당 1장
    bool IActionStep.RequiresMoveInput => HasMoveOption;

    Behavior? IActionStep.CreateBehavior(ActionRequest request, BattleManager manager)
    {
        var skill = SkillRegistry.Resolve(SkillClass);
        if (skill == null) return null;

        var ctx = new BattleContext
        {
            CasterId  = request.CasterId,
            TargetIds = request.TargetIds,
            Battle    = manager
        };
        return new SkillBehavior(skill, ctx);
    }
}
