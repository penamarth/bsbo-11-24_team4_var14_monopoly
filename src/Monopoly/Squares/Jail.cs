namespace Monopoly;

// Если просто попал на эту клетку - игрок "посещает" тюрьму
public class Jail : Square
{
    public Jail(string name, int position) : base(name, position)
    {
    }

    public override void OnLand(Player player, Game game)
    {
        game.Log(nameof(Jail), $"{GameTrace.Describe(player)} посещает клетку {GameTrace.Describe(this)}.");
    }
}
