using GameCore.Behaviors;
using GameCore.Battle.Behaviors;
using GameCore.Battle.Gauge;
using GameCore.Battle.Characters;
using GameCore.Events;
using GameCore.Scripting;
using GameCore.Units;

namespace GameCore.Battle;

public class BattleManager
{
    public BattleGrid Grid { get; }
    public BehaviorQueue Queue { get; }
    public EventBus Events { get; }

    private readonly List<Unit> _units = new();
    public IReadOnlyList<Unit> Units => _units;

    public Unit? ActiveUnit { get; private set; }
    public bool IsBattleOver { get; private set; } = false;

    public BattleManager(BattleGrid grid, EventBus events)
    {
        Grid = grid;
        Events = events;
        Queue = new BehaviorQueue();
        Queue.OnDrained += CheckBattleEnd;
    }

    // ── 셋업 ──────────────────────────────────────────────────────────

    public void AddUnit(Unit unit)
    {
        _units.Add(unit);
        var tile = Grid.GetTile(unit.X, unit.Y);
        if (tile != null) tile.OccupantId = unit.Id;
    }

    // ── CTB ───────────────────────────────────────────────────────────

    public void TickAll(float delta)
    {
        foreach (var unit in _units.Where(u => u.IsAlive))
            unit.ActionGauge = Math.Max(0f, unit.ActionGauge - delta);
    }

    public Unit? GetReadyUnit()
        => _units
            .Where(u => u.IsAlive && u.ActionGauge <= 0f)
            .OrderBy(u => u.Id)
            .FirstOrDefault();

    // ── 턴 관리 ───────────────────────────────────────────────────────

    public void BeginTurn(Unit unit)
    {
        ActiveUnit = unit;
        Events.Publish(new TurnStarted(unit.Id));
    }

    public void EndTurn(int gaugeWeight)
    {
        if (ActiveUnit == null) return;
        ActiveUnit.ActionGauge = Math.Min(ActionGauge.MaxValue, gaugeWeight);
        TickEquipmentRepairs(ActiveUnit);
        ActiveUnit = null;
    }

    private void TickEquipmentRepairs(Unit unit)
    {
        TryRepair(unit, unit.WeaponSlot,    SlotType.Weapon);
        TryRepair(unit, unit.ArmorSlot,     SlotType.Armor);
        TryRepair(unit, unit.AccessorySlot, SlotType.Accessory);
    }

    private void TryRepair(Unit unit, Equipment? equipment, SlotType slot)
    {
        if (equipment?.TickRepair() == true)
            Events.Publish(new EquipmentRepaired(unit.Id, slot.ToString()));
    }

    // ── 유닛 조회 ─────────────────────────────────────────────────────1
    public Unit? GetUnit(Guid id) => _units.FirstOrDefault(u => u.Id == id);
    public IEnumerable<Unit> GetAliveUnits() => _units.Where(u => u.IsAlive);
    public IEnumerable<Unit> GetEnemiesOf(Unit unit) => _units.Where(u => u.IsAlive && IsEnemy(unit, u));
    public IEnumerable<Unit> GetAlliesOf(Unit unit)  => _units.Where(u => u.IsAlive && !IsEnemy(unit, u) && u.Id != unit.Id);

    public IEnumerable<Unit> GetUnitsInRange(int cx, int cy, RangeType type, int range)
        => _units.Where(u => u.IsAlive && IsInRange(cx, cy, u.X, u.Y, type, range));

    // ── 상태 변경 ─────────────────────────────────────────────────────

    public void ApplyDamage(Guid attackerId, Guid targetId, int baseDamage)
    {
        var attacker = GetUnit(attackerId);
        var target   = GetUnit(targetId);
        if (target == null || !target.IsAlive) return;

        int enhancement = attacker?.WeaponSlot?.PublicCard?.GetCount(WellKnownTokens.Enhancement) ?? 0;
        int guard       = target.ArmorSlot?.PublicCard?.GetCount(WellKnownTokens.Guard) ?? 0;
        int final       = baseDamage + enhancement - guard;  // 최소값 없음 (0뎀 가능)

        target.Hp = Math.Max(0, target.Hp - final);
        Events.Publish(new DamageDealt(attackerId, targetId, final));

        ApplySuppression(attacker, target);
        ActivateTriggerTokens(targetId, TriggerCondition.OnHit);

        if (!target.IsAlive)
        {
            var tile = Grid.GetTile(target.X, target.Y);
            if (tile != null) tile.OccupantId = null;
            Events.Publish(new UnitDefeated(target.Id));
        }
    }

    public void MoveUnit(Guid unitId, int toX, int toY)
    {
        var unit   = GetUnit(unitId);
        var toTile = Grid.GetTile(toX, toY);
        if (unit == null || toTile == null || !toTile.IsPassable || toTile.IsOccupied) return;

        var fromTile = Grid.GetTile(unit.X, unit.Y);
        if (fromTile != null) fromTile.OccupantId = null;

        unit.X = toX;
        unit.Y = toY;
        toTile.OccupantId = unit.Id;
    }

    public void ReduceDurability(Guid targetId, SlotType slot, int amount)
    {
        var target = GetUnit(targetId);
        if (target == null) return;

        var equipment = slot switch
        {
            SlotType.Weapon    => target.WeaponSlot,
            SlotType.Armor     => target.ArmorSlot,
            SlotType.Accessory => target.AccessorySlot,
            _                  => null
        };
        if (equipment == null) return;

        equipment.PublicCard?.AddCount(WellKnownTokens.Durability, -amount);
        if (equipment.IsBroken)
        {
            equipment.OnBreak();
            Events.Publish(new EquipmentBroken(targetId, slot.ToString()));
        }
    }

    // ── 제압 & 트리거 ─────────────────────────────────────────────────

    private void ApplySuppression(Unit? attacker, Unit target)
    {
        int suppression = attacker?.WeaponSlot?.PublicCard?.Suppression ?? 0;
        if (suppression <= 0) return;

        var attr    = attacker!.WeaponSlot!.PublicCard!.Attribute;
        var weakness = target.ArmorSlot?.PublicCard?.Weakness;
        bool isWeak  = attr.HasValue && attr == weakness;

        ReduceDurability(target.Id, SlotType.Armor, isWeak ? suppression * 2 : suppression);
    }

    /// <summary>조건에 맞는 TriggerToken을 모든 장비 슬롯에서 찾아 실행 후 소모.</summary>
    public void ActivateTriggerTokens(Guid targetId, TriggerCondition condition)
    {
        var target = GetUnit(targetId);
        if (target == null) return;

        foreach (var equip in new[] { target.WeaponSlot, target.ArmorSlot, target.AccessorySlot })
        {
            if (equip?.PublicCard == null) continue;
            var fired = equip.PublicCard.Tokens
                .OfType<TriggerToken>()
                .Where(t => t.TriggerCondition == condition)
                .ToList();

            foreach (var t in fired)
            {
                var skill = SkillRegistry.ResolveTrigger(t.SkillClass);
                if (skill != null)
                {
                    var ctx = new BattleContext { CasterId = targetId, Battle = this };
                    Queue.PushFront(new TriggerSkillBehavior(skill, ctx));
                }
                equip.PublicCard.Tokens.Remove(t);
                Events.Publish(new TriggerTokenActivated(targetId, t.Id));
            }
        }
    }

    // ── 전투 종료 판정 ────────────────────────────────────────────────

    private void CheckBattleEnd()
    {
        bool golemsAlive  = _units.OfType<Golem>().Any(u => u.IsAlive);
        bool enemiesAlive = _units.OfType<Enemy>().Any(u => u.IsAlive);

        if (!golemsAlive || !enemiesAlive)
        {
            IsBattleOver = true;
            Events.Publish(new BattleEnded(golemsAlive));
        }
    }

    // ── 헬퍼 ──────────────────────────────────────────────────────────

    private static bool IsEnemy(Unit a, Unit b) => (a is Golem) != (b is Golem);

    private static bool IsInRange(int cx, int cy, int x, int y, RangeType type, int range)
    {
        int dx = Math.Abs(x - cx);
        int dy = Math.Abs(y - cy);
        return type switch
        {
            RangeType.Linear   => (dx == 0 || dy == 0) && dx + dy <= range && dx + dy > 0,
            RangeType.Diagonal => dx == dy && dx <= range && dx > 0,
            RangeType.All      => Math.Max(dx, dy) <= range && (dx + dy) > 0,
            RangeType.Self     => dx == 0 && dy == 0,
            _                  => false
        };
    }
}
