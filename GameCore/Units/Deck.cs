namespace GameCore.Units;

public class Deck
{
    private readonly List<VariationCard> _drawPile = new();
    private readonly List<VariationCard> _discardPile = new();
    private readonly List<VariationCard> _hand = new();

    public IReadOnlyList<VariationCard> Hand => _hand;
    public int DrawPileCount => _drawPile.Count;
    public int DiscardPileCount => _discardPile.Count;

    public void Build(IEnumerable<VariationCard> cards)
    {
        _drawPile.Clear();
        _discardPile.Clear();
        _hand.Clear();
        _drawPile.AddRange(cards);
        Shuffle(_drawPile);
    }

    public VariationCard? Draw()
    {
        if (_drawPile.Count == 0)
        {
            if (_discardPile.Count == 0) return null;
            _drawPile.AddRange(_discardPile);
            _discardPile.Clear();
            Shuffle(_drawPile);
        }

        var card = _drawPile[^1];
        _drawPile.RemoveAt(_drawPile.Count - 1);
        _hand.Add(card);
        return card;
    }

    public bool Discard(VariationCard card)
    {
        if (!_hand.Remove(card)) return false;
        _discardPile.Add(card);
        return true;
    }

    private static void Shuffle(List<VariationCard> list)
    {
        var rng = Random.Shared;
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
