namespace GameCore.Units;

public class Golem : Unit
{
    public Golem() => Name = "이름 없는 인형";

    public int WeaponSlotLevel    { get; set; } = 1;
    public int ArmorSlotLevel     { get; set; } = 1;
    public int AccessorySlotLevel { get; set; } = 1;

    public int Level => Math.Max(WeaponSlotLevel, Math.Max(ArmorSlotLevel, AccessorySlotLevel));

    public float AccumulatedTurns { get; set; } = 0f;

    public Deck Deck { get; } = new();

    /// <summary>3슬롯 덱카드 전부 합쳐 덱을 구성한다.</summary>
    public void BuildDeck()
    {
        var cards = new List<DeckCard>();
        if (WeaponSlot    != null) cards.AddRange(WeaponSlot.DeckCards);
        if (ArmorSlot     != null) cards.AddRange(ArmorSlot.DeckCards);
        if (AccessorySlot != null) cards.AddRange(AccessorySlot.DeckCards);
        Deck.Build(cards);
    }
}
