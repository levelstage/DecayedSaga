namespace GameCore.Map;

public class WorldMap
{
    public string StartNodeId { get; set; } = "";
    public string CurrentNodeId { get; set; } = "";

    private readonly Dictionary<string, DungeonNode> _nodes = new();

    public void AddNode(DungeonNode node) => _nodes[node.Id] = node;
    public DungeonNode? GetNode(string id) => _nodes.GetValueOrDefault(id);

    public IEnumerable<DungeonNode> GetAccessibleNodes()
    {
        var current = GetNode(CurrentNodeId);
        if (current == null) yield break;
        foreach (var nextId in current.NextNodeIds)
        {
            var next = GetNode(nextId);
            if (next != null) yield return next;
        }
    }
}
