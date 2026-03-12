using GameCore.Battle.Behaviors;
using GameCore.Behaviors;

namespace GameCore.Battle;

/// <summary>
/// ActionRequest.Steps를 순서 그대로 Behavior 목록으로 변환해 Sequence를 만든다.
/// 순서는 플레이어가 결정했으므로 ActionBuilder는 조립만 한다.
/// </summary>
public static class ActionBuilder
{
    public static Sequence Build(ActionRequest request, BattleManager manager)
    {
        // 기본 이동 (Steps 없이 MoveDestination만 있는 경우)
        if (!request.Steps.Any() && request.MoveDestination.HasValue)
            return new Sequence(new MoveBehavior(request.CasterId, request.MoveDestination.Value, manager));

        var behaviors = request.Steps
            .Select(s => s.CreateBehavior(request, manager))
            .OfType<Behavior>()
            .ToList();

        return new Sequence(behaviors);
    }
}
