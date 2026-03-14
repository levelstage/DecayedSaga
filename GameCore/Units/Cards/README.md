# Units/Cards/

| 파일 | 설명 |
|------|------|
| `PublicCard.cs` | 장비의 인게임 대리인. 스탯(Suppression/Movement/Attribute/Weakness) + 토큰 목록 + 브레이크·수복 상태 담당. |
| `DeckCard.cs` | 행동 카드. CardType(Active/Trigger), TriggerCondition, GaugeWeight, SkillClass. |

## PublicCard 핵심 로직
- `IsBroken` — 내구도 토큰이 존재하고 Count ≤ 0
- `OnBreak()` — 토큰 전체 소멸
- `TickRepair()` — 매 자기 턴 호출. BreakRecoveryTurns 도달 시 `Repair()` 자동 호출
- `Repair()` — 내구도 복원 + InitialTokenIds 재부착
