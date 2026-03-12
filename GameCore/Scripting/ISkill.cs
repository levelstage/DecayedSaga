using GameCore.Battle;

namespace GameCore.Scripting;

public class BattleContext
{
    public Guid CasterId { get; set; }
    public List<Guid> TargetIds { get; set; } = new();
    public BattleManager Battle { get; set; } = null!;
}

public interface IActiveSkill
{
    bool CanUse(BattleContext context);
    void Execute(BattleContext context);
}

public interface IPassiveSkill
{
    void OnEquip(Guid golemId);
    void OnUnequip(Guid golemId);
}

public interface IDefaultSkill
{
    void Execute(BattleContext context);
}
