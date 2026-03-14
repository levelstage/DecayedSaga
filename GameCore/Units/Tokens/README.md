# Units/Tokens/

| 파일 | 설명 |
|------|------|
| `Token.cs` | 기본 클래스. `string Id` + `int Count`. 새 토큰 종류는 JSON에서 임의 Id로 정의 가능. |
| `TriggerToken.cs` | `Token` 서브클래스. TriggerCondition + SkillClass 추가. 발동 시 PublicCard에서 제거(소모). |
| `WellKnownTokens.cs` | MVP 토큰 Id 상수. |

## WellKnownTokens
| Id | 슬롯 | 효과 |
|----|------|------|
| `durability` | 전체 | 0이 되면 브레이크 |
| `enhancement` | 무기 | 수만큼 공격력 +1 |
| `guard` | 장갑 | 수만큼 피해 -1 |
