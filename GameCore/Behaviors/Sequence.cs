namespace GameCore.Behaviors;

public class Sequence : Behavior
{
    private readonly List<Behavior> _steps;

    public Sequence(IEnumerable<Behavior> steps) => _steps = new List<Behavior>(steps);
    public Sequence(params Behavior[] steps)      => _steps = new List<Behavior>(steps);

    public override void Execute(BehaviorQueue queue)
    {
        for (int i = _steps.Count - 1; i >= 0; i--)
            queue.PushFront(_steps[i]);

        NotifyExecuted(queue.ProcessNext);
    }
}
