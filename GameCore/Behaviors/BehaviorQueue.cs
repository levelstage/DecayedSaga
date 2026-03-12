namespace GameCore.Behaviors;

public class BehaviorQueue
{
    private readonly LinkedList<Behavior> _queue = new();
    private bool _isProcessing = false;

    public event Action? OnDrained;

    public void Enqueue(Behavior behavior) => _queue.AddLast(behavior);
    public void PushFront(Behavior behavior) => _queue.AddFirst(behavior);

    public void Start()
    {
        if (_isProcessing) return;
        _isProcessing = true;
        ProcessNext();
    }

    public void ProcessNext()
    {
        if (_queue.Count == 0)
        {
            _isProcessing = false;
            OnDrained?.Invoke();
            return;
        }

        var next = _queue.First!.Value;
        _queue.RemoveFirst();
        next.Execute(this);
    }

    public int Count => _queue.Count;
    public bool IsEmpty => _queue.Count == 0;
}
