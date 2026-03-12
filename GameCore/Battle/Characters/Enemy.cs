using GameCore.Units;

namespace GameCore.Battle.Characters;

public class Enemy : Unit
{
    // AI 카드 우선순위 목록 (순서대로 CanUse 판단)
    public List<string> CardPriority { get; set; } = new();
}
