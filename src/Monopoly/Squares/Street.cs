namespace Monopoly;

public class Street : Property
{
    public const int MaxHouses = 4;

    public string ColorGroup { get; }
    public override string DisplayName
    {
        get
        {
            string symbol = ColorGroup switch
            {
                "Коричневый" => "🟫",
                "Белый" => "⬜",
                "Фиолетовый" => "🟪",
                "Оранжевый" => "🟧",
                "Красный" => "🟥",
                "Жёлтый" => "🟨",
                "Зелёный" => "🟩",
                "Синий" => "🟦",
                _ => ""
            };
            string prefix = symbol == "" ? "" : $"{symbol} ";
            return $"{prefix}{Name} ({ColorGroup})";
        }
    }
    public int HouseCost { get; }

    // аренда: [без домов, 1 дом, 2 дома, 3 дома, 4 дома, отель]
    public int[] RentLevels { get; }

    public int HouseCount { get; private set; }
    public bool HasHotel { get; private set; }

    public Street(string name, int position, int price, string colorGroup, int houseCost, int[] rentLevels)
        : base(name, position, price)
    {
        if (rentLevels.Length != 6)
        {
            throw new ArgumentException("Нужно 6 значений аренды");
        }

        ColorGroup = colorGroup;
        HouseCost = houseCost;
        RentLevels = rentLevels;
    }

    // Уровень застройки: 0-4 дома, 5 = отель
    public int GetBuildingLevel()
    {
        int level = HasHotel ? MaxHouses + 1 : HouseCount;
        GameTrace.Write(nameof(Street), $"Уровень застройки {GameTrace.Describe(this)} — {level}.");
        return level;
    }

    public override int CalculateRent(Game game)
    {
        int level = HasHotel ? MaxHouses + 1 : HouseCount;
        int rent = RentLevels[level];

        // если у владельца вся цветовая группа - аренда без домов двойная
        if (level == 0 && Owner != null && game.Board.OwnsWholeGroup(Owner, ColorGroup))
        {
            rent *= 2;
        }

        game.Log(nameof(Street), $"Аренда на {GameTrace.Describe(this)} — {rent}. Уровень застройки {level}.");
        return rent;
    }

    public bool CanBuild(Player player, Board board)
    {
        GameTrace.Write(nameof(Street), $"Проверяет возможность строительства игроком {GameTrace.Describe(player)} на {GameTrace.Describe(this)}.");
        if (Owner != player || HasHotel)
        {
            return false;
        }

        if (!board.OwnsWholeGroup(player, ColorGroup))
        {
            return false;
        }

        if (!board.IsBuildingEven(this))
        {
            return false;
        }

        return player.CanAfford(HouseCost);
    }

    public void AddHouse()
    {
        if (HasHotel)
        {
            return;
        }

        if (HouseCount < MaxHouses)
        {
            HouseCount++;
        }
        else
        {
            // пятый дом превращается в отель
            HouseCount = 0;
            HasHotel = true;
        }

        string result = HasHotel ? "построен отель" : $"домов теперь {HouseCount}";
        GameTrace.Write(nameof(Street), $"На {GameTrace.Describe(this)} {result}.");
    }

    public void RemoveHouse()
    {
        if (HasHotel)
        {
            HasHotel = false;
            HouseCount = MaxHouses;
        }
        else if (HouseCount > 0)
        {
            HouseCount--;
        }
    }

    public void ClearBuildings()
    {
        HouseCount = 0;
        HasHotel = false;
        GameTrace.Write(nameof(Street), $"Застройка на {GameTrace.Describe(this)} обнулена.");
    }
}
