using GameCore.Behaviors;
using GameCore.Scripting;

namespace GameCore.Battle.Behaviors;

/// <summary>IActiveSkill을 BehaviorQueue 안에서 실행하는 Behavior.</summary>
public class SkillBehavior : Behavior
{
    private readonly IActiveSkill  _skill;
    private readonly BattleContext _ctx;

    public SkillBehavior(IActiveSkill skill, BattleContext ctx)
    {
        _skill = skill;
        _ctx   = ctx;
    }

    public override void Execute(BehaviorQueue queue)
    {
        if (_skill.CanUse(_ctx))
            _skill.Execute(_ctx);

        NotifyExecuted(queue.ProcessNext);
    }
}
