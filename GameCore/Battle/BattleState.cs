using GameCore.Behaviors;
using GameCore.Units;
using GameCore.Units.Movement;

namespace GameCore.Battle;

public enum BattlePhase
{
    WaitingForTurn,  // CTB 게이지 감소 중
    DrawPhase,       // 드로우 처리 (자동 진행)
    SelectAction,    // 공개카드 OR 기본이동 선택
    SelectVariation, // 시퀀스 구성 (공개카드+변주를 클릭 순서로 쌓음)
    SelectTarget,    // 타겟 선택
    SelectMove,      // 이동 목적지 선택
    DiscardPhase,    // 핸드 3장 이상 시 1장 버리기
    EnemyThinking,   // 적 AI 처리
    ExecutingQueue,  // BehaviorQueue 실행 중
    BattleOver
}

/// <summary>
/// Steps = 플레이어가 클릭한 순서 그대로의 IActionStep 목록.
/// </summary>
public record ActionRequest(
    Guid              CasterId,
    PublicCard?       SelectedCard,
    List<IActionStep> Steps,
    List<Guid>        TargetIds,
    (int X, int Y)?   MoveDestination
);

public class BattleState
{
    public BattlePhase     Phase        { get; private set; } = BattlePhase.WaitingForTurn;
    public Unit?           ActiveUnit   { get; private set; }
    public PublicCard?     SelectedCard { get; private set; }
    public (int X, int Y)? PendingMove  { get; private set; }
    public bool            IsBasicMove  { get; private set; }

    public IReadOnlyList<IActionStep> PendingSteps   => _pendingSteps;
    public IReadOnlyList<Guid>        PendingTargets => _pendingTargets;

    private readonly List<IActionStep> _pendingSteps   = new();
    private readonly List<Guid>        _pendingTargets = new();

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

    public bool SelectCard(PublicCard card)
    {
        if (Phase != BattlePhase.SelectAction) return false;
        SelectedCard = card; IsBasicMove = false; PendingMove = null;
        _pendingSteps.Clear(); _pendingTargets.Clear();
        SetPhase(BattlePhase.SelectVariation);
        return true;
    }

    public bool SelectBasicMove()
    {
        if (Phase != BattlePhase.SelectAction) return false;
        SelectedCard = null; IsBasicMove = true;
        _pendingSteps.Clear(); _pendingTargets.Clear(); PendingMove = null;
        SetPhase(BattlePhase.SelectMove);
        return true;
    }

    /// <summary>공개카드의 기본 행동을 현재 위치에 삽입. 중복 불가.</summary>
    public bool AddCardAction()
    {
        if (Phase != BattlePhase.SelectVariation || SelectedCard == null) return false;
        if (_pendingSteps.Contains(SelectedCard)) return false;
        _pendingSteps.Add(SelectedCard);
        return true;
    }

    /// <summary>변주 카드를 시퀀스 끝에 추가.</summary>
    public bool AddVariation(VariationCard card)
    {
        if (Phase != BattlePhase.SelectVariation) return false;
        if (card.IsIndependent && _pendingSteps.OfType<VariationCard>().Any(v => v.IsIndependent))
            return false;
        _pendingSteps.Add(card);
        return true;
    }

    /// <summary>시퀀스 확정 → SelectTarget. 공개카드 미삽입 시 자동으로 맨 뒤에 추가.</summary>
    public bool ConfirmVariations()
    {
        if (Phase != BattlePhase.SelectVariation) return false;
        if (SelectedCard != null && !_pendingSteps.Contains(SelectedCard))
            _pendingSteps.Add(SelectedCard);
        SetPhase(BattlePhase.SelectTarget);
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
        if (_pendingSteps.Any(s => s.RequiresMoveInput)) SetPhase(BattlePhase.SelectMove);
        else CommitAction();
        return true;
    }

    public bool SelectMoveDestination(int x, int y)
    {
        if (Phase != BattlePhase.SelectMove) return false;
        PendingMove = (x, y); CommitAction();
        return true;
    }

    public bool SkipMove()
    {
        if (Phase != BattlePhase.SelectMove) return false;
        CommitAction();
        return true;
    }

    public bool DiscardCard(VariationCard card)
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
            ActiveUnit!.Id, SelectedCard,
            _pendingSteps.ToList(), _pendingTargets.ToList(), PendingMove
        );
        SetPhase(BattlePhase.ExecutingQueue);
        OnActionCommitted?.Invoke(request);
        if (_manager.Queue.IsEmpty) _manager.Queue.Start();
    }

    private void OnQueueDrained()
    {
        if (_manager.IsBattleOver) { SetPhase(BattlePhase.BattleOver); return; }

        int gaugeWeight = SelectedCard?.GaugeWeight ?? GetMoveGaugeWeight();
        if (ActiveUnit is Golem golem) golem.AccumulatedTurns += gaugeWeight / 4f;
        _manager.EndTurn(gaugeWeight);

        if (ActiveUnit is Golem g && g.Deck.Hand.Count >= 3) { SetPhase(BattlePhase.DiscardPhase); return; }

        ResetTurnState();
        SetPhase(BattlePhase.WaitingForTurn);
    }

    private void ResetTurnState()
    {
        ActiveUnit = null; SelectedCard = null; IsBasicMove = false;
        _pendingSteps.Clear(); _pendingTargets.Clear(); PendingMove = null;
    }

    private int GetMoveGaugeWeight()
    {
        if (ActiveUnit is not Golem golem) return 4;
        var piece = golem.GetPieceType();
        return piece.HasValue ? MovementPattern.GaugeCost(piece.Value) : 4;
    }

    private void SetPhase(BattlePhase phase) { Phase = phase; OnPhaseChanged?.Invoke(phase); }
}
