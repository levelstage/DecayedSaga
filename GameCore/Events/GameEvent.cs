namespace GameCore.Events;

public abstract record GameEvent;

public record BattleStarted(IReadOnlyList<Guid> GolemIds, IReadOnlyList<Guid> EnemyIds) : GameEvent;
public record TurnStarted(Guid UnitId) : GameEvent;
public record DamageDealt(Guid AttackerId, Guid TargetId, int Amount) : GameEvent;
public record UnitDefeated(Guid UnitId) : GameEvent;
public record EquipmentBroken(Guid UnitId, string SlotType) : GameEvent;
public record BattleEnded(bool PlayerWon) : GameEvent;

public record NodeEntered(string NodeId) : GameEvent;
public record NodeCleared(string NodeId) : GameEvent;
