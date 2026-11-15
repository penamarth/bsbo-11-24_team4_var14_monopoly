namespace Monopoly;

public class CommunityChest : Square
{
    public Deck Deck { get; }

    public CommunityChest(string name, int position, Deck deck) : base(name, position)
    {
        Deck = deck;
    }

    public override void OnLand(Player player, Game game)
    {
        game.Log(nameof(CommunityChest), $"{GameTrace.Describe(player)} попадает на {GameTrace.Describe(this)}.");
        Card card = Deck.DrawCard();
        Deck.ReturnToBottom(card);
        card.ApplyEffect(player, game);
    }
}
