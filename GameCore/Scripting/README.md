# Scripting/
스킬 인터페이스 정의 + JSON SkillClass 문자열 → 인스턴스 리플렉션 바인딩.

| 파일 | 설명 |
|------|------|
| `ISkill.cs` | `IActiveSkill` / `ITriggerSkill` / `IPassiveSkill` 인터페이스. `BattleContext` 정의. |
| `SkillRegistry.cs` | `skillClass` 문자열 → `Type.GetType()` → `Activator.CreateInstance()`. |

## 스킬 타입
| 인터페이스 | 사용처 | 발동 |
|-----------|--------|------|
| `IActiveSkill` | 액티브 DeckCard | 행동권 시 카드 제출 |
| `ITriggerSkill` | 트리거 DeckCard / TriggerToken | 조건 충족 시 자동 |
| `IPassiveSkill` | 장비 장착 효과 | OnEquip / OnUnequip |

## 구현체 위치
스킬 구현체는 `GameApp` 또는 `GameCore/Skills/`(추후)에 작성.
`DeckCard.SkillClass` = 어셈블리 한정 타입명 (`"GameCore.Skills.AttackSkill"` 등).
