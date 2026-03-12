using GameCore.Behaviors;

namespace GameCore.Battle.Behaviors;

/// <summary>유닛을 목적지 타일로 이동시키는 Behavior.</summary>
public class MoveBehavior : Behavior
{
    private readonly Guid _unitId;
    private readonly (int X, int Y) _destination;
    private readonly BattleManager _manager;

    public MoveBehavior(Guid unitId, (int X, int Y) destination, BattleManager manager)
    {
        _unitId      = unitId;
        _destination = destination;
        _manager     = manager;
    }

    public override void Execute(BehaviorQueue queue)
    {
        _manager.MoveUnit(_unitId, _destination.X, _destination.Y);
        NotifyExecuted(queue.ProcessNext);
    }
}
