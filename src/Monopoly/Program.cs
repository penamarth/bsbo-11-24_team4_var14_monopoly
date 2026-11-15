using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Monopoly;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        GameTrace.StartBlock("МОНОПОЛИЯ — ИНИЦИАЛИЗАЦИЯ ИГРЫ", first: true);
        var input = new ConsoleInput();

        int count = input.AskNumber($"Количество игроков ({Game.MinPlayers}-{Game.MaxPlayers}): ");
        while (!input.EndOfInput && (count < Game.MinPlayers || count > Game.MaxPlayers))
        {
            count = input.AskNumber($"Нужно от {Game.MinPlayers} до {Game.MaxPlayers}: ");
        }

        var names = new List<string>();
        for (int i = 1; i <= count && !input.EndOfInput; i++)
        {
            string name = input.AskText($"Имя игрока {i}: ");
            names.Add(name == "" ? $"Игрок {i}" : name);
        }

        if (input.EndOfInput)
        {
            GameTrace.StartBlock("ЗАВЕРШЕНИЕ ИГРЫ");
            GameTrace.Write(nameof(Program), "Ввод завершён. Партия не началась.");
            return;
        }

        GameTrace.StartSection("Подготовка поля и колод");
        var game = new Game(input, new Dice(), new Random());
        GameTrace.StartSection("Игроки и порядок ходов");
        game.StartGame(names);

        while (game.Status == GameStatus.InProgress)
        {
            GameTrace.StartBlock("МЕНЮ ХОДА");
            Player player = game.GetCurrentPlayer();
            GameTrace.Write(nameof(Program), $"Ходит {Describe(game, player)}.");
            GameTrace.Write(nameof(Program), $"""
                Доступные действия
                    1 — бросить кубики   → {nameof(Game)}.{nameof(Game.PlayTurn)}()
                    2 — построить дом    → {nameof(Program)}.{nameof(BuildMenu)}()
                    3 — мои владения     → {nameof(Program)}.{nameof(ShowProperties)}()
                    4 — все игроки       → {nameof(Program)}.{nameof(ShowPlayers)}()
                    0 — выход            → {nameof(Program)}.{nameof(Main)}() (завершение)
                """);

            int choice = input.AskNumber("> ");
            switch (choice)
            {
                case 1:
                    game.PlayTurn();
                    break;
                case 2:
                    BuildMenu(game, player, input);
                    break;
                case 3:
                    ShowProperties(game, player);
                    break;
                case 4:
                    ShowPlayers(game);
                    break;
                case 0:
                    GameTrace.StartBlock("ЗАВЕРШЕНИЕ ИГРЫ");
                    GameTrace.Write(nameof(Program), "Игра прервана.");
                    return;
                default:
                    GameTrace.Write(nameof(Program), "Нет такого пункта.");
                    break;
            }
        }

        GameTrace.StartBlock("ЗАВЕРШЕНИЕ ИГРЫ");
        Player? winner = game.GetWinner();
        GameTrace.Write(nameof(Program), winner == null
            ? "Игра окончена без победителя."
            : $"Игра окончена! Победитель — {GameTrace.Describe(winner)}.");
    }

    private static void BuildMenu(Game game, Player player, IInput input)
    {
        GameTrace.StartBlock("СТРОИТЕЛЬСТВО");
        GameTrace.Write(nameof(Program), $"Подбирает улицы, на которых может строить {GameTrace.Describe(player)}.");
        List<Street> streets = game.Board.GetPropertiesOf(player)
            .OfType<Street>()
            .Where(s => s.CanBuild(player, game.Board))
            .ToList();

        if (streets.Count == 0)
        {
            GameTrace.Write(nameof(Program), "Строить пока негде. Проверьте владение группой, баланс и равномерность застройки.");
            return;
        }

        GameTrace.StartSection("Доступные улицы");
        foreach (Street street in streets)
        {
            GameTrace.Write(nameof(Program),
                $"{GameTrace.Describe(street)} — домов {street.HouseCount}, цена дома {street.HouseCost}.");
        }

        GameTrace.Separate();
        int position = input.AskNumber("Номер клетки: ");
        Street? chosen = streets.FirstOrDefault(s => s.Position == position);

        if (chosen == null || !game.BuildHouse(player, chosen))
        {
            GameTrace.Write(nameof(Program), "Построить не получилось.");
        }
    }

    private static void ShowProperties(Game game, Player player)
    {
        GameTrace.StartBlock("МОИ ВЛАДЕНИЯ");
        GameTrace.Write(nameof(Program), $"Владения игрока {GameTrace.Describe(player)}.");
        List<Property> properties = game.Board.GetPropertiesOf(player);

        if (properties.Count == 0)
        {
            GameTrace.Write(nameof(Program), "У вас пока ничего нет.");
            return;
        }

        GameTrace.Separate();
        foreach (Property property in properties)
        {
            string extra = "";
            if (property is Street street)
            {
                extra = street.HasHotel ? ", отель" : $", домов: {street.HouseCount}";
            }
            GameTrace.Write(nameof(Program), $"{GameTrace.Describe(property)}{extra}");
        }
    }

    private static string Describe(Game game, Player player)
    {
        Square square = game.Board.GetSquare(player.Position);
        return $"{GameTrace.Describe(player)} — баланс {player.Cash}, клетка {GameTrace.Describe(square)}";
    }

    private static void ShowPlayers(Game game)
    {
        GameTrace.StartBlock("ВСЕ ИГРОКИ");
        foreach (Player p in game.Players)
        {
            string status = p.Bankrupt ? ", банкрот" : p.InJail ? ", в тюрьме" : "";
            GameTrace.Write(nameof(Program), $"{Describe(game, p)}{status}.");
        }
    }
}
