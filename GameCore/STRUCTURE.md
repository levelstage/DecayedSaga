# GameCore 구조

순수 C# 로직 라이브러리. MonoGame 의존성 없음.

---

## Battle/
전투 관련 핵심 시스템.

| 파일 | 설명 |
|------|------|
| `BattleManager.cs` | 유닛 관리, CTB 틱, 피해/제압/이동/수복 처리. `ApplyDamage` → 제압 자동 적용 + TriggerToken 발동. |
| `BattleState.cs` | 페이즈 머신 (WaitingForTurn→DrawPhase→SelectAction→…→DiscardPhase). 플레이어 입력(SelectCard, Pass, SubmitTrigger 등) 처리. |
| `ActionBuilder.cs` | `ActionRequest`를 받아 `Sequence`로 변환. SkillBehavior + MoveBehavior 조립. |
| `BattleGrid.cs` | N×M 타일 그리드. 타일 점유 추적. |
| `Tile.cs` | 타일 데이터 (IsPassable, OccupantId). |
| `RangeData.cs` | 사거리 타입(Linear/Diagonal/All/Self)과 range 묶음. |

### Battle/Behaviors/
BehaviorQueue에서 실행되는 전투 Behavior.

| 파일 | 설명 |
|------|------|
| `SkillBehavior.cs` | `IActiveSkill`을 큐에서 실행. |
| `TriggerSkillBehavior.cs` | `ITriggerSkill`을 큐에서 실행 (트리거 카드/TriggerToken 발동 시 사용). |
| `MoveBehavior.cs` | 이동 목적지로 유닛을 이동. |

### Battle/Characters/
| 파일 | 설명 |
|------|------|
| `Enemy.cs` | `Unit` 상속. 카드 우선순위 리스트 보유. AI가 순서대로 CanUse 판단. |

### Battle/Gauge/
| 파일 | 설명 |
|------|------|
| `ActionGauge.cs` | MaxValue = 4 상수 정의. |

---

## Behaviors/
씬 무관 범용 실행 파이프라인.

| 파일 | 설명 |
|------|------|
| `Behavior.cs` | 추상 기반 클래스. `Execute(BehaviorQueue)` + `OnExecuted` 이벤트. |
| `BehaviorQueue.cs` | LinkedList 기반 데크. `Enqueue`(뒤)/`PushFront`(앞 삽입). 트리거 인터럽트는 PushFront 사용. |
| `Sequence.cs` | Behavior 컴포지트. Execute 시 자식들을 큐 앞에 삽입. |

---

## Events/
발행-구독 이벤트 버스.

| 파일 | 설명 |
|------|------|
| `EventBus.cs` | `Publish<T>` / `Subscribe<T>` 범용 버스. |
| `GameEvent.cs` | 모든 게임 이벤트 레코드 정의 (DamageDealt, EquipmentBroken, TriggerTokenActivated 등). |

---

## Scripting/
스킬 인터페이스 + 리플렉션 바인딩.

| 파일 | 설명 |
|------|------|
| `ISkill.cs` | `IActiveSkill` / `ITriggerSkill` / `IPassiveSkill` 인터페이스. `BattleContext` 정의. |
| `SkillRegistry.cs` | `SkillClass` 문자열 → `Type.GetType()` → `Activator.CreateInstance()`. JSON 데이터와 연결고리. |

---

## Units/
유닛·장비·카드 모델. 전투 외 진행(맵·성장)에서도 사용.

| 파일 | 설명 |
|------|------|
| `Unit.cs` | 공통 기반 (Id, Name, Hp, MaxHp, X, Y, WeaponSlot, ArmorSlot, AccessorySlot, IsAlive). |
| `Golem.cs` | `Unit` 상속. 슬롯 레벨, AccumulatedTurns, Deck. `BuildDeck()`으로 3슬롯 DeckCards 합산. |
| `Equipment.cs` | 장비 컨테이너. PublicCard + DeckCards + 식별 정보. 상태는 PublicCard에 위임. |
| `Relic.cs` | `Equipment` 상속. SoulName, IsBound, GolemSprite. 장착 시 인형 외형 변화. |
| `Deck.cs` | DrawPile/Hand/DiscardPile 관리. Draw/Discard/Shuffle. |

### Units/Cards/
| 파일 | 설명 |
|------|------|
| `PublicCard.cs` | 장비의 인게임 대리인. 스탯(Suppression/Movement/Attribute/Weakness) + 토큰 보유 + 브레이크/수복 상태 관리. |
| `DeckCard.cs` | 행동 카드. CardType(Active/Trigger), TriggerCondition, GaugeWeight, SkillClass. |

### Units/Tokens/
| 파일 | 설명 |
|------|------|
| `Token.cs` | 토큰 기본 클래스. `string Id` + `int Count`. 데이터 드리븐 (새 토큰 = JSON에서 임의 Id 정의). |
| `TriggerToken.cs` | `Token` 서브클래스. TriggerCondition + SkillClass 추가. 발동 시 소모. |
| `WellKnownTokens.cs` | MVP 토큰 Id 상수 (`durability` / `enhancement` / `guard`). |

### Units/Enums/
| 파일 | 설명 |
|------|------|
| `SlotType.cs` | Weapon / Armor / Accessory |
| `EquipmentTier.cs` | Farmer(1) ~ Hero(5) |
| `AttributeType.cs` | Slash / Impact / Pierce / Special |
| `CardType.cs` | Active / Trigger |
| `TriggerCondition.cs` | OnHit (추후 확장) |

---

## Map/
던전 맵·노드 구조.

| 파일 | 설명 |
|------|------|
| `WorldMap.cs` | 노드 그래프. |
| `DungeonNode.cs` | 전투/이벤트/휴식 등 노드 타입. |
