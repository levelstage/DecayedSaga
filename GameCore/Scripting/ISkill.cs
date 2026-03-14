using GameCore.Battle;

namespace GameCore.Scripting;

public class BattleContext
{
    public Guid             CasterId        { get; set; }
    public List<Guid>       TargetIds       { get; set; } = new();
    public BattleManager    Battle          { get; set; } = null!;
    public (int X, int Y)?  MoveDestination { get; set; }  // 장갑 액티브 카드 이동 목적지
}

public interface IActiveSkill
{
    bool CanUse(BattleContext context);
    void Execute(BattleContext context);
}

public interface ITriggerSkill
{
    bool CanTrigger(BattleContext context);
    void Execute(BattleContext context);
}

public interface IPassiveSkill
{
    void OnEquip(Guid golemId);
    void OnUnequip(Guid golemId);
}
