namespace GameCore.Units;

/// <summary>
/// 장비의 인게임 대리인.
/// 스탯 표시 + 토큰 보유 + 브레이크 상태 + 자동 수복 처리를 담당.
/// Equipment는 이 카드를 통해 인게임 상태를 관리한다.
/// </summary>
public class PublicCard
{
    public string   CardId   { get; set; } = "";
    public string   Name     { get; set; } = "";
    public SlotType SlotType { get; set; }

    // ── 슬롯별 스탯 ───────────────────────────────────────────────────
    /// <summary>무기 슬롯: 제압 수치 (공격 시 적 장갑 내구도 감소)</summary>
    public int Suppression { get; set; }
    /// <summary>장갑 슬롯: 이동력</summary>
    public int Movement    { get; set; }

    // ── 속성 ──────────────────────────────────────────────────────────
    /// <summary>무기 슬롯: 공격 속성 (참격/충격/관통/특수)</summary>
    public AttributeType? Attribute { get; set; }
    /// <summary>장갑 슬롯: 취약 속성 — 해당 속성 공격 시 제압 2배</summary>
    public AttributeType? Weakness  { get; set; }

    // ── 브레이크 & 수복 ───────────────────────────────────────────────
    public int   MaxDurability      { get; set; }
    /// <summary>자동 수복까지 필요한 자기 턴 수 (0이면 수복 불가)</summary>
    public float BreakRecoveryTurns { get; set; }
    public float BrokenTurnsElapsed { get; private set; }

    // ── 토큰 (런타임 상태) ────────────────────────────────────────────
    public List<Token> Tokens { get; } = new();

    /// <summary>장착 시 자동 부착할 토큰 Id 목록 (데이터용)</summary>
    public List<string> InitialTokenIds { get; set; } = new();

    public string Description { get; set; } = "";

    // ── 브레이크 판정 ─────────────────────────────────────────────────
    /// <summary>내구도 토큰이 존재하고 0이면 브레이크 (토큰 없으면 브레이크 아님)</summary>
    public bool IsBroken => GetToken(WellKnownTokens.Durability) is { Count: <= 0 };

    // ── 토큰 접근 ─────────────────────────────────────────────────────
    public Token? GetToken(string id) => Tokens.FirstOrDefault(t => t.Id == id);
    public int    GetCount(string id) => GetToken(id)?.Count ?? 0;

    public void SetCount(string id, int value)
    {
        var token = GetToken(id);
        if (token == null) { token = new Token(id); Tokens.Add(token); }
        token.Count = Math.Max(0, value);
    }

    public void AddCount(string id, int amount) => SetCount(id, GetCount(id) + amount);

    // ── 브레이크 & 수복 처리 ──────────────────────────────────────────
    public void OnBreak()
    {
        Tokens.Clear();
        BrokenTurnsElapsed = 0;
    }

    /// <summary>
    /// 이 장비를 장착한 유닛의 턴 종료 시 호출.
    /// BreakRecoveryTurns == 0이면 자동 수복 불가.
    /// </summary>
    public bool TickRepair()
    {
        if (!IsBroken || BreakRecoveryTurns <= 0) return false;
        BrokenTurnsElapsed += 1f;
        if (BrokenTurnsElapsed >= BreakRecoveryTurns)
        {
            Repair();
            return true;
        }
        return false;
    }

    /// <summary>수복: 내구도 복원 + 초기 토큰 재부착</summary>
    public void Repair()
    {
        BrokenTurnsElapsed = 0;
        SetCount(WellKnownTokens.Durability, MaxDurability);
        ApplyInitialTokens();
    }

    /// <summary>장착 시 호출. InitialTokenIds의 토큰을 자동 부착 (이미 있으면 스킵).</summary>
    public void ApplyInitialTokens()
    {
        foreach (var id in InitialTokenIds)
            if (GetToken(id) == null) Tokens.Add(new Token(id));
    }
}
