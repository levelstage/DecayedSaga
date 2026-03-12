namespace GameCore.Events;

public class EventBus
{
    private readonly Dictionary<Type, List<Action<GameEvent>>> _handlers = new();

    public void Subscribe<T>(Action<T> handler) where T : GameEvent
    {
        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var list))
            _handlers[type] = list = new List<Action<GameEvent>>();
        list.Add(e => handler((T)e));
    }

    public void Publish(GameEvent e)
    {
        if (_handlers.TryGetValue(e.GetType(), out var list))
            foreach (var handler in list)
                handler(e);
    }
}
