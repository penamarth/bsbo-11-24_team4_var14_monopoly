using System.Runtime.CompilerServices;

namespace Monopoly;

public class Game
{
    public const int StartCash = 1500;
    public const int MinPlayers = 2;
    public const int MaxPlayers = 6;

    private readonly IInput _input;
    private readonly Random? _random;

    public List<Player> Players { get; } = new List<Player>();
    public Board Board { get; private set; }
    public Dice Dice { get; }
    public Deck ChanceDeck { get; private set; }
    public Deck CommunityChestDeck { get; private set; }
    public Auction? Auction { get; private set; }
    public GameStatus Status { get; private set; } = GameStatus.NotStarted;
    public int CurrentPlayerIndex { get; private set; }

    public Game(IInput input, Dice dice, Random? random = null)
    {
        Log(nameof(Game), "Создаётся партия (Game) в состоянии NotStarted.");
        _input = input;
        Dice = dice;
        _random = random;

        (Board, ChanceDeck, CommunityChestDeck) = CreateBoard(random);
    }

    private static (Board Board, Deck ChanceDeck, Deck CommunityChestDeck) CreateBoard(Random? random)
    {
        Deck chanceDeck = Deck.CreateChanceDeck();
        GameTrace.Separate();
        Deck communityChestDeck = Deck.CreateCommunityChestDeck();

        GameTrace.Separate();
        if (random != null)
        {
            chanceDeck.Shuffle(random);
            communityChestDeck.Shuffle(random);
        }

        return (Board.CreateStandard(chanceDeck, communityChestDeck), chanceDeck, communityChestDeck);
    }

    public void StartGame(List<string> playerNames)
    {
        if (playerNames.Count < MinPlayers || playerNames.Count > MaxPlayers)
        {
            throw new ArgumentException($"Игроков должно быть от {MinPlayers} до {MaxPlayers}");
        }

        Log(nameof(Game), $"Начинается новая партия для {playerNames.Count} игроков.");
        if (Status != GameStatus.NotStarted)
        {
            (Board, ChanceDeck, CommunityChestDeck) = CreateBoard(_random);
            Dice.Reset();
            Auction = null;
        }

        Players.Clear();
        foreach (string name in playerNames)
        {
            Players.Add(new Player(name, StartCash));
        }

        // очерёдность ходов определяется случайно (в тестах random не передаём)
        if (_random != null)
        {
            for (int i = Players.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (Players[i], Players[j]) = (Players[j], Players[i]);
            }
        }

        CurrentPlayerIndex = 0;
        Status = GameStatus.InProgress;
        string order = string.Join(", ", Players.Select(GameTrace.Describe));
        Log(nameof(Game), $"Статус InProgress. Порядок ходов — {order}.");
    }

    public Player GetCurrentPlayer()
    {
        Player player = Players[CurrentPlayerIndex];
        Log(nameof(Game), $"Текущий игрок — {GameTrace.Describe(player)}.");
        return player;
    }

    public List<Player> GetActivePlayers()
    {
        List<Player> activePlayers = Players.Where(p => !p.Bankrupt).ToList();
        Log(nameof(Game), $"Игроков (Player) без банкротства — {activePlayers.Count}.");
        return activePlayers;
    }

    public void PlayTurn()
    {
        if (Status != GameStatus.InProgress)
        {
            return;
        }

        GameTrace.StartBlock("ХОД ИГРОКА");
        Player player = GetCurrentPlayer();
        Log(nameof(Game), $"Ход игрока {GameTrace.Describe(player)}. Баланс {player.Cash}.");

        if (player.InJail)
        {
            GameTrace.StartSection("Пропуск хода в тюрьме");
            player.TryLeaveJail();

            GameTrace.StartSection("Итог хода");
            AdvanceToNextPlayer();
            return;
        }

        GameTrace.StartSection("Бросок и перемещение");
        int sum = Dice.Roll();
        player.Move(sum);

        GameTrace.StartSection("Действие клетки");
        Square square = Board.GetSquare(player.Position);
        square.OnLand(player, this);

        GameTrace.StartSection("Итог хода");
        CheckGameOver();
        AdvanceToNextPlayer();
    }

    public bool AskBuyDecision(Player player, Property property)
    {
        Log(nameof(Game), $"Запрашивает решение о покупке у игрока {GameTrace.Describe(player)}.");
        return _input.AskYesNo($"{GameTrace.Describe(player)}, купить {GameTrace.Describe(property)} за {property.Price}?");
    }

    public void OfferPurchase(Player player, Property property)
    {
        GameTrace.StartSection("Покупка собственности");
        Log(nameof(Game), $"Проверяет возможность покупки {GameTrace.Describe(property)} игроком {GameTrace.Describe(player)}.");
        if (player.CanAfford(property.Price) && AskBuyDecision(player, property))
        {
            player.Pay(property.Price);
            property.SetOwner(player);
            Log(nameof(Game), $"{GameTrace.Describe(player)} купил {GameTrace.Describe(property)} за {property.Price}.");
            return;
        }

        Log(nameof(Game), $"{GameTrace.Describe(property)} выставляется на аукцион (Auction).");
        StartAuction(property);
    }

    // Игроки по кругу делают ставки, 0 = пас.
    // Аукцион идёт, пока не останется один лидер
    public Player? StartAuction(Property property)
    {
        GameTrace.StartSection("Аукцион");
        Log(nameof(Game), $"Запускает аукцион (Auction) за {GameTrace.Describe(property)}.");
        var auction = new Auction(property, GetActivePlayers());
        Auction = auction;

        try
        {
            while (!auction.IsFinished())
            {
                foreach (Player bidder in auction.Participants.ToList())
                {
                    if (bidder == auction.HighestBidder)
                    {
                        continue;
                    }

                    GameTrace.Separate();
                    int bid = _input.AskNumber(
                        $"{GameTrace.Describe(bidder)}, ваша ставка за {GameTrace.Describe(property)} (сейчас {auction.CurrentBid}, 0 — пас): ");

                    if (!auction.PlaceBid(bidder, bid))
                    {
                        auction.Pass(bidder);
                    }

                    if (auction.IsFinished())
                    {
                        break;
                    }
                }
            }

            GameTrace.StartSection("Результат аукциона");
            Player? winner = auction.Conclude();

            if (winner != null)
            {
                Log(nameof(Game), $"Аукцион (Auction) выиграл {GameTrace.Describe(winner)} за {auction.CurrentBid}.");
            }
            else
            {
                Log(nameof(Game), $"Ставок не было. {GameTrace.Describe(property)} остаётся свободной.");
            }

            return winner;
        }
        finally
        {
            Auction = null;
        }
    }

    // receiver = null, если деньги никому не идут (налог, штраф)
    public void ChargePayment(Player payer, Player? receiver, int amount)
    {
        GameTrace.StartSection("Обязательный платёж");
        string recipient = receiver == null ? "без получателя" : $"в пользу {GameTrace.Describe(receiver)}";
        Log(nameof(Game), $"Обязательный платёж {amount} от {GameTrace.Describe(payer)} {recipient}.");
        if (payer.CanAfford(amount))
        {
            payer.Pay(amount);
            receiver?.Receive(amount);
            return;
        }

        HandleBankruptcy(payer, receiver, amount);
    }

    public void HandleBankruptcy(Player debtor, Player? creditor, int amount)
    {
        GameTrace.StartSection("Проверка банкротства");
        Log(nameof(Game), $"Проверяет, можно ли погасить долг {amount} игрока {GameTrace.Describe(debtor)} продажей построек.");
        // сначала пробуем продать дома за полцены
        SellAllBuildings(debtor);

        if (debtor.CanAfford(amount))
        {
            Log(nameof(Game), $"После продажи построек {GameTrace.Describe(debtor)} может погасить долг.");
            debtor.Pay(amount);
            creditor?.Receive(amount);
            return;
        }

        GameTrace.StartSection("Распределение имущества");
        Log(nameof(Game), $"{GameTrace.Describe(debtor)} не может погасить долг {amount}. Распределяются оставшиеся активы.");

        if (creditor != null)
        {
            creditor.Receive(debtor.Cash);
            Board.TransferProperties(debtor, creditor);
        }
        else
        {
            Board.ReleaseProperties(debtor);
        }

        debtor.DeclareBankrupt();
        CheckGameOver();
    }

    private void SellAllBuildings(Player player)
    {
        Log(nameof(Game), $"Проверяет постройки игрока {GameTrace.Describe(player)} для продажи за полцены.");
        foreach (Street street in Board.GetPropertiesOf(player).OfType<Street>())
        {
            int level = street.GetBuildingLevel();
            if (level > 0)
            {
                int price = level * street.HouseCost / 2;
                Log(nameof(Game), $"Продаёт постройки на {GameTrace.Describe(street)} за {price}.");
                player.Receive(price);
                street.ClearBuildings();
            }
        }
    }

    public bool BuildHouse(Player player, Street street)
    {
        if (!street.CanBuild(player, Board))
        {
            return false;
        }

        player.Pay(street.HouseCost);
        street.AddHouse();
        string building = street.HasHotel ? "отель" : "дом";
        Log(nameof(Game), $"{GameTrace.Describe(player)} построил {building} на {GameTrace.Describe(street)}.");
        return true;
    }

    public void AdvanceToNextPlayer()
    {
        if (Status != GameStatus.InProgress)
        {
            Log(nameof(Game), $"Статус {Status}. Очередь игроков не меняется.");
            return;
        }

        for (int i = 1; i <= Players.Count; i++)
        {
            int index = (CurrentPlayerIndex + i) % Players.Count;
            if (!Players[index].Bankrupt)
            {
                CurrentPlayerIndex = index;
                Log(nameof(Game), $"Ход переходит к {GameTrace.Describe(Players[index])}.");
                return;
            }
        }
    }

    public bool CheckGameOver()
    {
        if (Status == GameStatus.NotStarted)
        {
            return false;
        }

        int activeCount = GetActivePlayers().Count;
        if (activeCount <= 1)
        {
            Status = GameStatus.Finished;
            Log(nameof(Game), $"Осталось активных игроков — {activeCount}. Статус Finished.");
            return true;
        }

        Log(nameof(Game), $"Осталось активных игроков — {activeCount}. Партия продолжается.");
        return false;
    }

    public Player? GetWinner()
    {
        if (Status != GameStatus.Finished)
        {
            return null;
        }

        Player? winner = GetActivePlayers().FirstOrDefault();
        Log(nameof(Game), winner == null ? "Победителя нет." : $"Победитель — {GameTrace.Describe(winner)}.");
        return winner;
    }

    public void Log(string message)
    {
        Log(nameof(Game), message);
    }

    internal void Log(string className, string message, [CallerMemberName] string methodName = "")
    {
        GameTrace.Write(className, message, methodName);
    }
}
