using GameCore.Behaviors;
using GameCore.Scripting;

namespace GameCore.Battle.Behaviors;

/// <summary>ITriggerSkill을 BehaviorQueue 안에서 실행하는 Behavior.</summary>
public class TriggerSkillBehavior : Behavior
{
    private readonly ITriggerSkill _skill;
    private readonly BattleContext _ctx;

    public TriggerSkillBehavior(ITriggerSkill skill, BattleContext ctx)
    {
        _skill = skill;
        _ctx   = ctx;
    }

    public override void Execute(BehaviorQueue queue)
    {
        if (_skill.CanTrigger(_ctx))
            _skill.Execute(_ctx);

        NotifyExecuted(queue.ProcessNext);
    }
}
