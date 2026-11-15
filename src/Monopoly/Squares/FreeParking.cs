namespace Monopoly;

public class FreeParking : Square
{
    public FreeParking(string name, int position) : base(name, position)
    {
    }

    public override void OnLand(Player player, Game game)
    {
        game.Log(nameof(FreeParking), $"{GameTrace.Describe(player)} отдыхает на клетке {GameTrace.Describe(this)}.");
    }
}
