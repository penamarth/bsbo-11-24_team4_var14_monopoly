namespace Monopoly;

// Общая логика для улиц, железных дорог и коммуналки.
// Подклассы меняют только расчёт аренды
public abstract class Property : Square
{
    public int Price { get; }
    public Player? Owner { get; private set; }

    protected Property(string name, int position, int price) : base(name, position)
    {
        Price = price;
    }

    public bool IsFree()
    {
        return Owner == null;
    }

    public Player? GetOwner()
    {
        return Owner;
    }

    public void SetOwner(Player? player)
    {
        Owner = player;
        string message = player == null
            ? $"{GameTrace.Describe(this)} теперь свободна."
            : $"Новый владелец {GameTrace.Describe(this)} — {GameTrace.Describe(player)}.";
        GameTrace.Write(nameof(Property), message);
    }

    public override void OnLand(Player player, Game game)
    {
        game.Log(nameof(Property), $"{GameTrace.Describe(player)} попадает на {GameTrace.Describe(this)}.");
        if (Owner == null)
        {
            game.OfferPurchase(player, this);
            return;
        }

        if (Owner == player)
        {
            game.Log(nameof(Property), "Это ваша собственность.");
            return;
        }

        int rent = CalculateRent(game);
        game.Log(nameof(Property), $"Аренда для {GameTrace.Describe(player)} в пользу {GameTrace.Describe(Owner)} — {rent}.");
        game.ChargePayment(player, Owner, rent);
    }

    public abstract int CalculateRent(Game game);
}
