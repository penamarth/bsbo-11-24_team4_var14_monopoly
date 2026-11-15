namespace Monopoly;

public class Auction
{
    private bool _settled;
    private bool HasEnded => Participants.Count == 0 || (Participants.Count == 1 && HighestBidder != null);

    public Property Property { get; }
    public List<Player> Participants { get; }
    public int CurrentBid { get; private set; }
    public Player? HighestBidder { get; private set; }

    public Auction(Property property, List<Player> participants)
    {
        Property = property;
        // копируем список, чтобы Pass не удалял игроков из самой игры
        Participants = new List<Player>(participants);
        string bidders = string.Join(", ", Participants.Select(GameTrace.Describe));
        GameTrace.Write(nameof(Auction), $"Создан аукцион (Auction) за {GameTrace.Describe(Property)}. Участники — {bidders}. Ставка 0.");
    }

    public bool PlaceBid(Player player, int amount)
    {
        if (_settled || HasEnded)
        {
            GameTrace.Write(nameof(Auction), "Торги завершены. Ставка отклонена.");
            return false;
        }

        if (!Participants.Contains(player))
        {
            GameTrace.Write(nameof(Auction), $"{GameTrace.Describe(player)} не участвует в торгах. Ставка отклонена.");
            return false;
        }

        if (amount <= CurrentBid)
        {
            string message = amount == 0
                ? $"{GameTrace.Describe(player)} пасует."
                : $"Ставка {amount} от {GameTrace.Describe(player)} должна превышать {CurrentBid}.";
            GameTrace.Write(nameof(Auction), message);
            return false;
        }

        if (!player.CanAfford(amount))
        {
            GameTrace.Write(nameof(Auction), $"Ставка {amount} от {GameTrace.Describe(player)} отклонена из-за нехватки денег.");
            return false;
        }

        CurrentBid = amount;
        HighestBidder = player;
        GameTrace.Write(nameof(Auction), $"Ставка {amount} принята. Новый лидер — {GameTrace.Describe(player)}.");
        return true;
    }

    public void Pass(Player player)
    {
        if (!_settled && player != HighestBidder && Participants.Remove(player))
        {
            GameTrace.Write(nameof(Auction), $"{GameTrace.Describe(player)} выбывает из текущих торгов.");
        }
    }

    // Аукцион закончен, когда остался только лидер (или никого)
    public bool IsFinished()
    {
        bool finished = _settled || HasEnded;
        string status = finished ? "торги завершены" : "торги продолжаются";
        GameTrace.Write(nameof(Auction), $"Участников осталось {Participants.Count}, {status}.");
        return finished;
    }

    // Победитель платит ставку и становится владельцем
    public Player? Conclude()
    {
        if (_settled)
        {
            return HighestBidder;
        }

        if (!HasEnded)
        {
            throw new InvalidOperationException("Торги ещё не завершены.");
        }

        if (HighestBidder != null)
        {
            GameTrace.Write(nameof(Auction), $"Завершает сделку с победителем {GameTrace.Describe(HighestBidder)} по ставке {CurrentBid}.");
            HighestBidder.Pay(CurrentBid);
            Property.SetOwner(HighestBidder);
        }
        else
        {
            GameTrace.Write(nameof(Auction), "Аукцион (Auction) завершён без победителя.");
        }

        _settled = true;
        return HighestBidder;
    }
}
