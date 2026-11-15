namespace Monopoly;

public class Chance : Square
{
    public Deck Deck { get; }

    public Chance(string name, int position, Deck deck) : base(name, position)
    {
        Deck = deck;
    }

    public override void OnLand(Player player, Game game)
    {
        game.Log(nameof(Chance), $"{GameTrace.Describe(player)} попадает на {GameTrace.Describe(this)}.");
        Card card = Deck.DrawCard();
        Deck.ReturnToBottom(card);
        card.ApplyEffect(player, game);
    }
}
