# Battle/
전투 관련 핵심 시스템.

| 파일 | 설명 |
|------|------|
| `BattleManager.cs` | 유닛 관리, CTB 틱, 피해·제압·이동·수복 처리. `ApplyDamage` → 제압 자동 적용 + OnHit TriggerToken 발동. |
| `BattleState.cs` | 페이즈 머신. 플레이어 입력(SelectCard / Pass / SubmitTrigger / DiscardCard) 처리. |
| `ActionBuilder.cs` | `ActionRequest` → `Sequence` 변환. SkillBehavior + MoveBehavior 조립. |
| `BattleGrid.cs` | N×M 타일 그리드. 타일 점유 추적. |
| `Tile.cs` | 타일 데이터 (IsPassable, OccupantId). |
| `RangeData.cs` | 사거리 타입(Linear/Diagonal/All/Self) + range 묶음. |

## 페이즈 흐름
```
WaitingForTurn → DrawPhase → SelectAction
  SelectAction → SelectTarget (무기 카드)
  SelectAction → SelectMove   (장갑 카드)
  SelectAction → ExecutingQueue (소품 카드 / 패스)
ExecutingQueue → DiscardPhase (핸드 4장↑)
             → WaitingForTurn
```

| 하위 폴더 | 역할 |
|-----------|------|
| `Behaviors/` | BehaviorQueue 안에서 실행되는 전투 Behavior |
| `Characters/` | Enemy 유닛 |
| `Gauge/` | ActionGauge 상수 |
