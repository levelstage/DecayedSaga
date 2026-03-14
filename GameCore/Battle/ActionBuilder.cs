using GameCore.Battle.Behaviors;
using GameCore.Behaviors;
using GameCore.Scripting;

namespace GameCore.Battle;

public static class ActionBuilder
{
    public static Sequence Build(ActionRequest request, BattleManager manager)
    {
        var behaviors = new List<Behavior>();

        if (request.Card != null)
        {
            var skill = SkillRegistry.ResolveActive(request.Card.SkillClass);
            if (skill != null)
            {
                var ctx = new BattleContext
                {
                    CasterId        = request.CasterId,
                    TargetIds       = request.TargetIds,
                    Battle          = manager,
                    MoveDestination = request.MoveDestination
                };
                behaviors.Add(new SkillBehavior(skill, ctx));
            }
        }

        return new Sequence(behaviors);
    }
}
