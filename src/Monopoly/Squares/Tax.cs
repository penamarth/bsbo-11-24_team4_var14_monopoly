namespace Monopoly;

public class Tax : Square
{
    public int Amount { get; }

    public Tax(string name, int position, int amount) : base(name, position)
    {
        Amount = amount;
    }
    // добавили описание 
    // Банка нет, поэтому налог просто выводится из игры (получателя нет)
    public override void OnLand(Player player, Game game)
    {
        game.Log(nameof(Tax), $"На клетке {GameTrace.Describe(this)} игроку {GameTrace.Describe(player)} начислен налог {Amount}.");
        game.ChargePayment(player, null, Amount);
    }
}
