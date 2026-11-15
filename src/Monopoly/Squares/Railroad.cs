namespace Monopoly;

public class Railroad : Property
{
    public const int BaseRent = 25;

    public Railroad(string name, int position, int price) : base(name, position, price)
    {
    }

    // 1 дорога - 25, 2 - 50, 3 - 100, 4 - 200
    public override int CalculateRent(Game game)
    {
        if (Owner == null)
        {
            return 0;
        }

        int count = game.Board.CountRailroadsOf(Owner);
        int rent = BaseRent;

        for (int i = 1; i < count; i++)
        {
            rent *= 2;
        }

        game.Log(nameof(Railroad), $"Аренда на {GameTrace.Describe(this)} — {rent} за {count} ж/д станций владельца.");
        return rent;
    }
}
