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
            var skill = SkillRegistry.Resolve(request.Card.SkillClass);
            if (skill != null)
            {
                var ctx = new BattleContext
                {
                    CasterId  = request.CasterId,
                    TargetIds = request.TargetIds,
                    Battle    = manager,
                    Variations = request.Variations
                };
                behaviors.Add(new SkillBehavior(skill, ctx));
            }
        }

        if (request.MoveDestination.HasValue)
            behaviors.Add(new MoveBehavior(request.CasterId, request.MoveDestination.Value, manager));

        return new Sequence(behaviors);
    }
}
