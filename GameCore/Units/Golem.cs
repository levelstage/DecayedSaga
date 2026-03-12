namespace GameCore.Units;

public class Golem : Unit
{
    public Golem() => Name = "이름 없는 인형";

    public int WeaponSlotLevel { get; set; } = 1;
    public int ArmorSlotLevel { get; set; } = 1;
    public int AccessorySlotLevel { get; set; } = 1;

    public int Level => Math.Max(WeaponSlotLevel, Math.Max(ArmorSlotLevel, AccessorySlotLevel));

    public float AccumulatedTurns { get; set; } = 0f;

    public Deck Deck { get; } = new();

    /// <summary>
    /// 3슬롯 변주카드 전부 합쳐 덱을 구성한다.
    /// </summary>
    public void BuildDeck()
    {
        var cards = new List<VariationCard>();
        if (WeaponSlot != null)    cards.AddRange(WeaponSlot.VariationCards);
        if (ArmorSlot != null)     cards.AddRange(ArmorSlot.VariationCards);
        if (AccessorySlot != null) cards.AddRange(AccessorySlot.VariationCards);
        Deck.Build(cards);
    }

    /// <summary>
    /// 장착된 유품에서 체스 기물 타입을 반환한다.
    /// </summary>
    public Movement.ChessPieceType? GetPieceType()
    {
        if (WeaponSlot    is Relic rw) return rw.PieceType;
        if (ArmorSlot     is Relic ra) return ra.PieceType;
        if (AccessorySlot is Relic rc) return rc.PieceType;
        return null;
    }
}
