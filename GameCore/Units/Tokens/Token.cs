namespace GameCore.Units;

/// <summary>
/// 장비의 공개카드에 올려지는 토큰. Id(종류 식별자)와 Count(수량) 보유.
/// 패시브 동작은 추후 TokenRegistry (Id → PassiveClass 리플렉션) 로 처리 예정.
/// </summary>
public class Token
{
    public string Id    { get; set; } = "";
    public int    Count { get; set; }

    public Token(string id, int count = 0) { Id = id; Count = count; }
}
