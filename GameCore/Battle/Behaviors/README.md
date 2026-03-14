# Battle/Behaviors/
BehaviorQueue 안에서 실행되는 전투 전용 Behavior.

| 파일 | 설명 |
|------|------|
| `SkillBehavior.cs` | `IActiveSkill`을 큐에서 실행. |
| `TriggerSkillBehavior.cs` | `ITriggerSkill`을 큐에서 실행. 트리거 카드·TriggerToken 발동 시 사용. PushFront로 삽입됨. |
| `MoveBehavior.cs` | 지정 목적지로 유닛을 이동. |
