# Units/
유닛·장비·카드 데이터 모델. 전투 외 맵·성장에서도 사용.

| 파일 | 설명 |
|------|------|
| `Unit.cs` | 공통 기반. Id, Name, Hp, MaxHp, X, Y, WeaponSlot, ArmorSlot, AccessorySlot, IsAlive. |
| `Golem.cs` | `Unit` 상속. 슬롯 레벨, AccumulatedTurns, Deck. `BuildDeck()`으로 3슬롯 DeckCards 합산. |
| `Equipment.cs` | 장비 컨테이너. PublicCard(인게임 상태) + DeckCards + 식별 정보. 상태는 PublicCard에 위임. |
| `Relic.cs` | `Equipment` 상속. SoulName, IsBound, GolemSprite. 인형당 1개 제한. 장착 시 인형 외형 변화. |
| `Deck.cs` | DrawPile / Hand / DiscardPile 관리. Draw / Discard / Shuffle. |

| 하위 폴더 | 역할 |
|-----------|------|
| `Cards/` | PublicCard (인게임 대리인), DeckCard (행동 카드) |
| `Tokens/` | Token, TriggerToken, WellKnownTokens |
| `Enums/` | SlotType, EquipmentTier, AttributeType, CardType, TriggerCondition |
