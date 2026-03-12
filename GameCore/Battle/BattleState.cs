using GameCore.Behaviors;
using GameCore.Units;
using GameCore.Units.Movement;

namespace GameCore.Battle;

public enum BattlePhase
{
    WaitingForTurn,
    DrawPhase,
    SelectAction,
    SelectVariation,
    SelectTarget,
    SelectMove,
    DiscardPhase,
    EnemyThinking,
    ExecutingQueue,
    BattleOver
}

public record ActionRequest(
    Guid               CasterId,
    PublicCard?        Card,
    List<VariationCard> Variations,
    List<Guid>         TargetIds,
    (int X, int Y)?    MoveDestination
);

public class BattleState
{
    public BattlePhase     Phase        { get; private set; } = BattlePhase.WaitingForTurn;
    public Unit?           ActiveUnit   { get; private set; }
    public PublicCard?     PendingCard  { get; private set; }
    public (int X, int Y)? PendingMove  { get; private set; }
    public bool            IsBasicMove  { get; private set; }

    public IReadOnlyList<VariationCard> PendingVariations => _pendingVariations;
    public IReadOnlyList<Guid>          PendingTargets    => _pendingTargets;

    private readonly List<VariationCard> _pendingVariations = new();
    private readonly List<Guid>          _pendingTargets    = new();

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
        PendingCard = card; IsBasicMove = false; PendingMove = null;
        _pendingVariations.Clear(); _pendingTargets.Clear();
        SetPhase(BattlePhase.SelectVariation);
        return true;
    }

    public bool SelectBasicMove()
    {
        if (Phase != BattlePhase.SelectAction) return false;
        PendingCard = null; IsBasicMove = true;
        _pendingVariations.Clear(); _pendingTargets.Clear(); PendingMove = null;
        SetPhase(BattlePhase.SelectMove);
        return true;
    }

    public bool AddVariation(VariationCard card)
    {
        if (Phase != BattlePhase.SelectVariation) return false;
        _pendingVariations.Add(card);
        return true;
    }

    public bool ConfirmVariations()
    {
        if (Phase != BattlePhase.SelectVariation) return false;
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
        if (PendingCard?.HasMoveOption == true) SetPhase(BattlePhase.SelectMove);
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
            ActiveUnit!.Id, PendingCard,
            _pendingVariations.ToList(), _pendingTargets.ToList(), PendingMove
        );
        SetPhase(BattlePhase.ExecutingQueue);
        OnActionCommitted?.Invoke(request);
        if (_manager.Queue.IsEmpty) _manager.Queue.Start();
    }

    private void OnQueueDrained()
    {
        if (_manager.IsBattleOver) { SetPhase(BattlePhase.BattleOver); return; }
        int gaugeWeight = PendingCard?.GaugeWeight ?? GetMoveGaugeWeight();
        if (ActiveUnit is Golem golem) golem.AccumulatedTurns += gaugeWeight / 4f;
        _manager.EndTurn(gaugeWeight);
        if (ActiveUnit is Golem g && g.Deck.Hand.Count >= 3) { SetPhase(BattlePhase.DiscardPhase); return; }
        ResetTurnState();
        SetPhase(BattlePhase.WaitingForTurn);
    }

    private void ResetTurnState()
    {
        ActiveUnit = null; PendingCard = null; IsBasicMove = false;
        _pendingVariations.Clear(); _pendingTargets.Clear(); PendingMove = null;
    }

    private int GetMoveGaugeWeight()
    {
        if (ActiveUnit is not Golem golem) return 4;
        var piece = golem.GetPieceType();
        return piece.HasValue ? MovementPattern.GaugeCost(piece.Value) : 4;
    }

    private void SetPhase(BattlePhase phase) { Phase = phase; OnPhaseChanged?.Invoke(phase); }
}
