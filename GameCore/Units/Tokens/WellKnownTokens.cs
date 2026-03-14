namespace GameCore.Units;

/// <summary>MVP 토큰 Id 상수. 새 토큰은 JSON에서 임의 string Id로 정의 가능.</summary>
public static class WellKnownTokens
{
    public const string Durability  = "durability";   // 모든 장비. 0이 되면 브레이크.
    public const string Enhancement = "enhancement";  // 무기 슬롯. 수만큼 공격력 +1.
    public const string Guard       = "guard";        // 장갑 슬롯. 수만큼 피해 -1.
}
