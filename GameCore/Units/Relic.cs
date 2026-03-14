namespace GameCore.Units;

/// <summary>
/// 유니크 장비. 무기/장갑/소품 슬롯 중 하나를 차지.
/// 일반 장비보다 토큰 풍부 + 특수 스킬 보유.
/// 장착 시 해당 영혼의 미니어처 외형으로 변화.
/// </summary>
public class Relic : Equipment
{
    public string SoulName    { get; set; } = "";
    public bool   IsBound     { get; set; } = false;
    public string GolemSprite { get; set; } = "";
}
