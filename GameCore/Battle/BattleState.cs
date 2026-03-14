using GameCore.Battle.Behaviors;
using GameCore.Behaviors;
using GameCore.Scripting;
using GameCore.Units;

namespace GameCore.Battle;

public enum BattlePhase
{
    WaitingForTurn,
    DrawPhase,
    SelectAction,   // 카드 제출 or 패스
    SelectTarget,   // 무기 액티브 — 타겟 선택
    SelectMove,     // 장갑 액티브 — 이동 목적지 선택
    ExecutingQueue,
    DiscardPhase,   // 핸드 4장 이상 → 1장 버리기
    EnemyThinking,
    BattleOver
}

public record ActionRequest(
    Guid            CasterId,
    DeckCard?       Card,            // null = 패스
    List<Guid>      TargetIds,
    (int X, int Y)? MoveDestination
);

public class BattleState
{
    public BattlePhase     Phase       { get; private set; } = BattlePhase.WaitingForTurn;
    public Unit?           ActiveUnit  { get; private set; }
    public DeckCard?       PendingCard { get; private set; }
    public (int X, int Y)? PendingMove { get; private set; }

    public IReadOnlyList<Guid> PendingTargets => _pendingTargets;
    private readonly List<Guid> _pendingTargets = new();

    private readonly BattleManager _manager;
    private bool _triggerDrawUsed = false;  // 이번 트리거 시퀀스에서 드로우 사용 여부

    public event Action<BattlePhase>?   OnPhaseChanged;
    public event Action<ActionRequest>? OnActionCommitted;

    public BattleState(BattleManager manager)
    {
        _manager = manager;
        _manager.Queue.OnDrained += OnQueueDrained;
    }

    // ── CTB 틱 ────────────────────────────────────────────────────────

    public void Tick(float delta)
    {
        if (Phase != BattlePhase.WaitingForTurn) return;
        _manager.TickAll(delta);
        var ready = _manager.GetReadyUnit();
        if (ready == null) return;
        ActiveUnit = ready;
        _manager.BeginTurn(ready);
        if (ready is Golem) { SetPhase(BattlePhase.DrawPhase); ProcessDraw(); }
        else                  SetPhase(BattlePhase.EnemyThinking);
    }

    private void ProcessDraw()
    {
        if (ActiveUnit is not Golem golem) { SetPhase(BattlePhase.SelectAction); return; }
        while (golem.AccumulatedTurns >= 1f) { golem.Deck.Draw(); golem.AccumulatedTurns -= 1f; }
        SetPhase(BattlePhase.SelectAction);
    }

    // ── 플레이어 입력 ─────────────────────────────────────────────────

    /// <summary>
    /// 액티브 카드 제출.
    /// 무기 → SelectTarget / 장갑 → SelectMove / 소품 → 즉시 커밋
    /// </summary>
    public bool SelectCard(DeckCard card)
    {
        if (Phase != BattlePhase.SelectAction) return false;
        if (IsSlotBroken(card.SlotType)) return false;  // 브레이크 슬롯 카드 사용 불가
        PendingCard = card;
        _pendingTargets.Clear();
        PendingMove = null;

        if (card.SlotType == SlotType.Weapon)
            SetPhase(BattlePhase.SelectTarget);
        else if (card.SlotType == SlotType.Armor)
            SetPhase(BattlePhase.SelectMove);
        else
            CommitAction(); // 소품 — 타겟/이동 없이 즉시

        return true;
    }

    /// <summary>패스 — 드로우 1장 + AG 1.0 재충전</summary>
    public bool Pass()
    {
        if (Phase != BattlePhase.SelectAction) return false;
        if (ActiveUnit is Golem golem) golem.Deck.Draw();
        PendingCard = null;
        _pendingTargets.Clear();
        PendingMove = null;
        CommitAction();
        return true;
    }

    public bool AddTarget(Guid targetId)
    {
        if (Phase != BattlePhase.SelectTarget) return false;
        _pendingTargets.Add(targetId);
        return true;
    }

    public bool ConfirmTargets()
    {
        if (Phase != BattlePhase.SelectTarget) return false;
        CommitAction();
        return true;
    }

    public bool SelectMoveDestination(int x, int y)
    {
        if (Phase != BattlePhase.SelectMove) return false;
        PendingMove = (x, y);
        CommitAction();
        return true;
    }

    public bool SkipMove()
    {
        if (Phase != BattlePhase.SelectMove) return false;
        CommitAction();
        return true;
    }

    /// <summary>
    /// 트리거 카드를 핸드에서 제출. 어느 페이즈에서든 호출 가능.
    /// 첫 제출 시 1드로우. 여러 장 제출해도 드로우는 1번만.
    /// </summary>
    public bool SubmitTrigger(DeckCard card, Golem golem)
    {
        if (card.CardType != CardType.Trigger) return false;
        if (IsSlotBroken(golem, card.SlotType)) return false;
        if (!golem.Deck.Hand.Contains(card)) return false;

        var skill = SkillRegistry.ResolveTrigger(card.SkillClass);
        if (skill == null) return false;

        var ctx = new BattleContext { CasterId = golem.Id, Battle = _manager };
        _manager.Queue.PushFront(new TriggerSkillBehavior(skill, ctx));
        golem.Deck.Discard(card);

        if (!_triggerDrawUsed)
        {
            golem.Deck.Draw();
            _triggerDrawUsed = true;
        }

        return true;
    }

    public bool DiscardCard(DeckCard card)
    {
        if (Phase != BattlePhase.DiscardPhase) return false;
        if (ActiveUnit is not Golem golem) return false;
        golem.Deck.Discard(card);
        ResetTurnState();
        SetPhase(BattlePhase.WaitingForTurn);
        return true;
    }

    // ── 외부 주입 ─────────────────────────────────────────────────────

    public void ExecuteSequence(Sequence sequence)
    {
        _manager.Queue.Enqueue(sequence);
        _manager.Queue.Start();
    }

    public void SubmitEnemySequence(Sequence sequence)
    {
        if (Phase != BattlePhase.EnemyThinking) return;
        _manager.Queue.Enqueue(sequence);
        _manager.Queue.Start();
        SetPhase(BattlePhase.ExecutingQueue);
    }

    // ── 내부 ──────────────────────────────────────────────────────────

    private void CommitAction()
    {
        var request = new ActionRequest(
            ActiveUnit!.Id, PendingCard,
            _pendingTargets.ToList(), PendingMove
        );
        SetPhase(BattlePhase.ExecutingQueue);
        OnActionCommitted?.Invoke(request);
        if (_manager.Queue.IsEmpty) _manager.Queue.Start();
    }

    private void OnQueueDrained()
    {
        if (_manager.IsBattleOver) { SetPhase(BattlePhase.BattleOver); return; }

        // 패스는 null → GaugeWeight 4 (AG 1.0)
        int gaugeWeight = PendingCard?.GaugeWeight ?? 4;
        if (ActiveUnit is Golem golem) golem.AccumulatedTurns += gaugeWeight / 4f;
        _manager.EndTurn(gaugeWeight);

        // 핸드 4장 이상 → 버리기 (실질 상한 3장)
        if (ActiveUnit is Golem g && g.Deck.Hand.Count >= 4)
        {
            SetPhase(BattlePhase.DiscardPhase);
            return;
        }

        ResetTurnState();
        SetPhase(BattlePhase.WaitingForTurn);
    }

    private void ResetTurnState()
    {
        ActiveUnit = null; PendingCard = null;
        _pendingTargets.Clear(); PendingMove = null;
        _triggerDrawUsed = false;
    }

    private bool IsSlotBroken(SlotType slot) => IsSlotBroken(ActiveUnit, slot);

    private static bool IsSlotBroken(Unit? unit, SlotType slot)
    {
        if (unit == null) return false;
        return slot switch
        {
            SlotType.Weapon    => unit.WeaponSlot?.IsBroken    ?? false,
            SlotType.Armor     => unit.ArmorSlot?.IsBroken     ?? false,
            SlotType.Accessory => unit.AccessorySlot?.IsBroken ?? false,
            _ => false
        };
    }

    private void SetPhase(BattlePhase phase) { Phase = phase; OnPhaseChanged?.Invoke(phase); }
}
