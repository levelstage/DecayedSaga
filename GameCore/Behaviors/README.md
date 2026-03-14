# Behaviors/
씬 무관 범용 실행 파이프라인.

| 파일 | 설명 |
|------|------|
| `Behavior.cs` | 추상 기반. `Execute(BehaviorQueue)` + `OnExecuted` 이벤트. |
| `BehaviorQueue.cs` | LinkedList 기반 데크. `Enqueue`(뒤 삽입) / `PushFront`(앞 삽입). 트리거 인터럽트는 PushFront. |
| `Sequence.cs` | Behavior 컴포지트. Execute 시 자식들을 큐 앞에 일괄 삽입. |

## 실행 흐름
```
Queue.Start() → ProcessNext() → Behavior.Execute() → NotifyExecuted(ProcessNext)
                                                    ↑ 트리거: PushFront 후 재개
```
