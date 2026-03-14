# Events/
발행-구독 이벤트 버스.

| 파일 | 설명 |
|------|------|
| `EventBus.cs` | `Publish<T>` / `Subscribe<T>`. GameApp에서 구독해 렌더링·사운드 처리. |
| `GameEvent.cs` | 모든 게임 이벤트 레코드 정의. |

## 이벤트 목록
| 이벤트 | 발생 시점 |
|--------|-----------|
| `BattleStarted` | 전투 시작 |
| `TurnStarted` | 유닛 턴 획득 |
| `DamageDealt` | 피해 적용 후 |
| `UnitDefeated` | HP 0 |
| `EquipmentBroken` | 내구도 0 |
| `EquipmentRepaired` | 자동 수복 완료 |
| `TriggerTokenActivated` | TriggerToken 발동·소모 |
| `BattleEnded` | 전투 종료 |
| `NodeEntered` / `NodeCleared` | 던전 노드 진입·클리어 |
