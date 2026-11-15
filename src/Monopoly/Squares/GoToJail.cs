namespace Monopoly;

public class GoToJail : Square
{
    public GoToJail(string name, int position) : base(name, position)
    {
    }

    public override void OnLand(Player player, Game game)
    {
        game.Log(nameof(GoToJail), $"{GameTrace.Describe(player)} попадает на {GameTrace.Describe(this)}.");
        player.SendToJail();
    }
}
