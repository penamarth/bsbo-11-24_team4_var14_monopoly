namespace Monopoly;

public class Card
{
    public string Description { get; }
    public EffectType EffectType { get; }
    public int Amount { get; }
    public int TargetPosition { get; }

    public Card(string description, EffectType effectType, int amount = 0, int targetPosition = 0)
    {
        Description = description;
        EffectType = effectType;
        Amount = amount;
        TargetPosition = targetPosition;
        GameTrace.Write(nameof(Card), $"Создана карта {GameTrace.Describe(this)}.");
    }

    public void ApplyEffect(Player player, Game game)
    {
        GameTrace.StartSection("Эффект карты");
        game.Log(nameof(Card), $"Карта {GameTrace.Describe(this)} применяет эффект {EffectType} к игроку {GameTrace.Describe(player)}.");
        switch (EffectType)
        {
            case EffectType.Fine:
                game.ChargePayment(player, null, Amount);
                break;

            case EffectType.Bonus:
                player.Receive(Amount);
                break;

            case EffectType.MoveTo:
                player.MoveTo(TargetPosition);
                // после перемещения обрабатываем новую клетку
                GameTrace.StartSection("Клетка назначения");
                Square square = game.Board.GetSquare(TargetPosition);
                square.OnLand(player, game);
                break;

            case EffectType.GoToJail:
                player.SendToJail();
                break;
        }
    }
}
