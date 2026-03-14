namespace GameCore.Units;

/// <summary>
/// 트리거형 토큰. 액티브 카드 사용으로 공개카드에 장착.
/// 트리거 조건 충족 시 스킬 발동 후 소모.
/// 토큰 제거 스킬(세척 등)로 강제 제거 가능.
/// </summary>
public class TriggerToken : Token
{
    public TriggerCondition TriggerCondition { get; set; }
    public string           SkillClass       { get; set; } = "";

    public TriggerToken(string id, TriggerCondition condition, string skillClass)
        : base(id, 1)
    {
        TriggerCondition = condition;
        SkillClass       = skillClass;
    }
}
