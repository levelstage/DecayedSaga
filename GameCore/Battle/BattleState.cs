using GameCore.Behaviors;
using GameCore.Units;
using GameCore.Units.Movement;

namespace GameCore.Battle;

public enum BattlePhase
{
    WaitingForTurn,  // CTB 게이지 감소 중
    DrawPhase,       // 드로우 처리 (자동 진행)
    SelectAction,    // 공개카드 OR 기본이동 선택
    SelectVariation, // 변주 선택 (선택적, ConfirmVariations로 스킵 가능)
    SelectTarget,    // 타겟 선택
    SelectMove,      // 이동 목적지 선택
    DiscardPhase,    // 핸드 3장 이상 시 1장 버리기
    EnemyThinking,   // 적 AI 처리
    ExecutingQueue,  // BehaviorQueue 실행 중
    BattleOver
}

/// <summary>
/// ActionBuilder(GameApp 또는 GameCore)가 Sequence를 만들어 ExecuteSequence에 전달해야 한다.
/// </summary>
public record ActionRequest(
    Guid CasterId,
    PublicCard? Card,              // null이면 기본 이동
    List<VariationCard> Variations,
    List<Guid> TargetIds,
    (int X, int Y)? MoveDestination
);

public class BattleState
{
    public BattlePhase Phase       { get; private set; } = BattlePhase.WaitingForTurn;
    public Unit?       ActiveUnit  { get; private set; }
    public PublicCard? PendingCard { get; private set; }
    public (int X, int Y)? PendingMove { get; private set; }

    public IReadOnlyList<VariationCard> PendingVariations => _pendingVariations;
    public IReadOnlyList<Guid>          PendingTargets    => _pendingTargets;

    private readonly List<VariationCard> _pendingVariations = new();
    private readonly List<Guid>          _pendingTargets    = new();
    public bool IsBasicMove { get; private set; }

    private readonly BattleManager _manager;

    /// <summary>페이즈 전환 알림. GameApp이 구독해 UI 갱신.</summary>
    public event Action<BattlePhase>?   OnPhaseChanged;

    /// <summary>
    /// 플레이어 행동 확정. 구독자(ActionBuilder)가 Sequence를 만들어
    /// ExecuteSequence()를 호출해야 큐가 실행된다.
    /// </summary>
    public event Action<ActionRequest>? OnActionCommitted;

    public BattleState(BattleManager manager)
    {
        _manager = manager;
        _manager.Queue.OnDrained += OnQueueDrained;
    }

    // ── CTB 틱 ────────────────────────────────────────────────────────

    /// <summary>GameApp의 Update() 루프에서 매 프레임 호출.</summary>
    public void Tick(float delta)
    {
        if (Phase != BattlePhase.WaitingForTurn) return;

        _manager.TickAll(delta);

        var ready = _manager.GetReadyUnit();
        if (ready == null) return;

        ActiveUnit = ready;
        _manager.BeginTurn(ready);

        if (ready is Golem)
        {
            SetPhase(BattlePhase.DrawPhase);
            ProcessDraw();
        }
        else
        {
            SetPhase(BattlePhase.EnemyThinking);
        }
    }

    // ── 드로우 (자동) ─────────────────────────────────────────────────

    private void ProcessDraw()
    {
        if (ActiveUnit is not Golem golem)
        {
            SetPhase(BattlePhase.SelectAction);
            return;
        }

        while (golem.AccumulatedTurns >= 1f)
        {
            golem.Deck.Draw();
            golem.AccumulatedTurns -= 1f;
        }

        SetPhase(BattlePhase.SelectAction);
    }

    // ── 플레이어 입력 ─────────────────────────────────────────────────

    /// <summary>공개카드 선택 → SelectVariation으로 전환.</summary>
    public bool SelectCard(PublicCard card)
    {
        if (Phase != BattlePhase.SelectAction) return false;

        PendingCard    = card;
        IsBasicMove   = false;
        PendingMove    = null;
        _pendingVariations.Clear();
        _pendingTargets.Clear();

        SetPhase(BattlePhase.SelectVariation);
        return true;
    }

    /// <summary>기본 이동 선택 → SelectMove로 전환.</summary>
    public bool SelectBasicMove()
    {
        if (Phase != BattlePhase.SelectAction) return false;

        PendingCard  = null;
        IsBasicMove = true;
        _pendingVariations.Clear();
        _pendingTargets.Clear();
        PendingMove  = null;

        SetPhase(BattlePhase.SelectMove);
        return true;
    }

    /// <summary>변주 카드 추가 (여러 장 가능, IsIndependent면 1장만).</summary>
    public bool AddVariation(VariationCard card)
    {
        if (Phase != BattlePhase.SelectVariation) return false;

        if (card.IsIndependent && _pendingVariations.Any(v => v.IsIndependent))
            return false; // 독립 키워드 중복 불가

        _pendingVariations.Add(card);
        return true;
    }

    /// <summary>변주 확정 → SelectTarget으로 전환.</summary>
    public bool ConfirmVariations()
    {
        if (Phase != BattlePhase.SelectVariation) return false;

        SetPhase(BattlePhase.SelectTarget);
        return true;
    }

    /// <summary>타겟 추가.</summary>
    public bool AddTarget(Guid targetId)
    {
        if (Phase != BattlePhase.SelectTarget) return false;

        _pendingTargets.Add(targetId);
        return true;
    }

    /// <summary>타겟 확정. 이동 옵션 있으면 SelectMove, 없으면 바로 커밋.</summary>
    public bool ConfirmTargets()
    {
        if (Phase != BattlePhase.SelectTarget) return false;

        if (PendingCard?.HasMoveOption == true)
            SetPhase(BattlePhase.SelectMove);
        else
            CommitAction();

        return true;
    }

    /// <summary>이동 목적지 선택 → 커밋.</summary>
    public bool SelectMoveDestination(int x, int y)
    {
        if (Phase != BattlePhase.SelectMove) return false;

        PendingMove = (x, y);
        CommitAction();
        return true;
    }

    /// <summary>이동 스킵 → 커밋.</summary>
    public bool SkipMove()
    {
        if (Phase != BattlePhase.SelectMove) return false;

        CommitAction();
        return true;
    }

    /// <summary>핸드 정리: 버릴 변주 카드 선택 (핸드 3장 이상 시만 유효).</summary>
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

    /// <summary>
    /// ActionBuilder가 OnActionCommitted를 받아 만든 Sequence를 여기로 전달.
    /// </summary>
    public void ExecuteSequence(Sequence sequence)
    {
        _manager.Queue.Enqueue(sequence);
        _manager.Queue.Start();
    }

    /// <summary>적 AI가 만든 Sequence를 전달.</summary>
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
            ActiveUnit!.Id,
            PendingCard,
            _pendingVariations.ToList(),
            _pendingTargets.ToList(),
            PendingMove
        );
        SetPhase(BattlePhase.ExecutingQueue);
        OnActionCommitted?.Invoke(request);
        // 구독자가 ExecuteSequence를 호출하지 않으면 큐가 비어있어 OnQueueDrained 즉시 발동
        if (_manager.Queue.IsEmpty)
            _manager.Queue.Start();
    }

    private void OnQueueDrained()
    {
        if (_manager.IsBattleOver)
        {
            SetPhase(BattlePhase.BattleOver);
            return;
        }

        // 게이지 재충전 + AccumulatedTurns 누적
        int gaugeWeight = PendingCard?.GaugeWeight ?? GetMoveGaugeWeight();
        if (ActiveUnit is Golem golem)
            golem.AccumulatedTurns += gaugeWeight / 4f;

        _manager.EndTurn(gaugeWeight);

        // 핸드 3장 이상이면 버리기 페이즈
        if (ActiveUnit is Golem g && g.Deck.Hand.Count >= 3)
        {
            SetPhase(BattlePhase.DiscardPhase);
            return;
        }

        ResetTurnState();
        SetPhase(BattlePhase.WaitingForTurn);
    }

    private void ResetTurnState()
    {
        ActiveUnit = null;
        PendingCard = null;
        IsBasicMove = false;
        _pendingVariations.Clear();
        _pendingTargets.Clear();
        PendingMove = null;
    }

    private int GetMoveGaugeWeight()
    {
        if (ActiveUnit is not Golem golem) return 4;
        var piece = golem.GetPieceType();
        return piece.HasValue ? MovementPattern.GaugeCost(piece.Value) : 4;
    }

    private void SetPhase(BattlePhase phase)
    {
        Phase = phase;
        OnPhaseChanged?.Invoke(phase);
    }
}
