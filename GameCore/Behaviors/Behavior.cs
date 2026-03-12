namespace GameCore.Behaviors;

public abstract class Behavior
{
    public event Action<Behavior, Action>? OnExecuted;

    public abstract void Execute(BehaviorQueue queue);

    protected void NotifyExecuted(Action continuation)
        => OnExecuted?.Invoke(this, continuation);
}
