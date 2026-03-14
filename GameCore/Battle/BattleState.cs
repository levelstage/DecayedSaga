using GameCore.Behaviors;
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
    }

    private bool IsSlotBroken(SlotType slot)
    {
        if (ActiveUnit == null) return false;
        return slot switch
        {
            SlotType.Weapon    => ActiveUnit.WeaponSlot?.IsBroken    ?? false,
            SlotType.Armor     => ActiveUnit.ArmorSlot?.IsBroken     ?? false,
            SlotType.Accessory => ActiveUnit.AccessorySlot?.IsBroken ?? false,
            _ => false
        };
    }

    private void SetPhase(BattlePhase phase) { Phase = phase; OnPhaseChanged?.Invoke(phase); }
}
