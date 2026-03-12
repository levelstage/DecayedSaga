using GameCore.Behaviors;

namespace GameCore.Battle;

/// <summary>
/// Sequence에 끼어들 수 있는 하나의 행동 단위.
/// PublicCard와 VariationCard 모두 구현한다.
/// 플레이어가 클릭한 순서 = 실행 순서.
/// </summary>
public interface IActionStep
{
    bool IsIndependent    { get; }
    bool RequiresMoveInput { get; }
    Behavior? CreateBehavior(ActionRequest request, BattleManager manager);
}
