namespace GameCore.Map;

public enum NodeType { Battle, Boss, Event, Repair }

public class DungeonNode
{
    public string Id { get; set; } = "";
    public NodeType Type { get; set; }
    public bool IsCleared { get; set; } = false;
    public bool IsVisited { get; set; } = false;
    public List<string> NextNodeIds { get; set; } = new();
    public string DataKey { get; set; } = "";
}
