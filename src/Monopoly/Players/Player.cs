namespace Monopoly;

public class Player
{
    public const int BoardSize = Board.Size;
    public const int GoReward = 200;
    public const int JailPosition = 10;
    public const int JailTurnsToLeave = 3;

    public string Name { get; }
    public int Cash { get; private set; }
    public int Position { get; private set; }
    public bool InJail { get; private set; }
    public int JailTurns { get; private set; }
    public bool Bankrupt { get; private set; }

    public Player(string name, int cash)
    {
        Name = name;
        Cash = cash;
        GameTrace.Write(nameof(Player), $"Создан игрок {GameTrace.Describe(this)}. Баланс {Cash}, позиция {Position}.");
    }

    // Двигаем игрока вперёд. Возвращает true, если он прошёл через Старт
    public bool Move(int steps)
    {
        int previousPosition = Position;
        int newPosition = Position + steps;
        bool passedGo = newPosition >= BoardSize;

        if (passedGo)
        {
            Cash += GoReward;
        }

        Position = newPosition % BoardSize;
        GameTrace.Write(nameof(Player),
            $"{GameTrace.Describe(this)} проходит {steps} клеток, с позиции {previousPosition} на {Position}.");
        if (passedGo)
        {
            GameTrace.Write(nameof(Player), $"{GameTrace.Describe(this)} получает {GoReward} за «Старт». Баланс {Cash}.");
        }
        return passedGo;
    }

    // Переносим игрока на конкретную клетку (для карточек).
    // Если клетка "позади", значит игрок прошёл круг и получает деньги за Старт
    public bool MoveTo(int position)
    {
        int previousPosition = Position;
        bool passedGo = position < previousPosition;

        if (passedGo)
        {
            Cash += GoReward;
        }

        Position = position;
        GameTrace.Write(nameof(Player), $"{GameTrace.Describe(this)} перемещается с позиции {previousPosition} на {Position}.");
        if (passedGo)
        {
            GameTrace.Write(nameof(Player), $"{GameTrace.Describe(this)} получает {GoReward} за «Старт». Баланс {Cash}.");
        }
        return passedGo;
    }

    public void Pay(int amount)
    {
        Cash -= amount;
        GameTrace.Write(nameof(Player), $"У игрока {GameTrace.Describe(this)} списано {amount}. Баланс {Cash}.");
    }

    public void Receive(int amount)
    {
        Cash += amount;
        GameTrace.Write(nameof(Player), $"Игроку {GameTrace.Describe(this)} зачислено {amount}. Баланс {Cash}.");
    }

    public bool CanAfford(int amount)
    {
        bool canAfford = Cash >= amount;
        string result = canAfford ? "достаточно" : "недостаточно";
        GameTrace.Write(nameof(Player), $"Баланс игрока {GameTrace.Describe(this)} — {Cash}, для оплаты {amount} средств {result}.");
        return canAfford;
    }

    public void SendToJail()
    {
        Position = JailPosition;
        InJail = true;
        JailTurns = 0;
        GameTrace.Write(nameof(Player), $"{GameTrace.Describe(this)} отправлен в «Тюрьму» (Jail, №{Position}).");
    }

    // Вызывается каждый ход, пока игрок в тюрьме.
    // После 3-го пропущенного хода игрок выходит
    public bool TryLeaveJail()
    {
        if (!InJail)
        {
            return true;
        }

        JailTurns++;

        if (JailTurns >= JailTurnsToLeave)
        {
            InJail = false;
            JailTurns = 0;
            GameTrace.Write(nameof(Player), $"{GameTrace.Describe(this)} пропускает третий ход и сможет бросить кубики в следующий.");
            return true;
        }

        GameTrace.Write(nameof(Player), $"{GameTrace.Describe(this)} остаётся в тюрьме. Пропущено ходов {JailTurns} из {JailTurnsToLeave}.");
        return false;
    }

    public void DeclareBankrupt()
    {
        Bankrupt = true;
        Cash = 0;
        InJail = false;
        JailTurns = 0;
        GameTrace.Write(nameof(Player), $"{GameTrace.Describe(this)} объявлен банкротом. Баланс обнулён.");
    }

    public override string ToString()
    {
        return $"{Name} (деньги: {Cash}, клетка: {Position})";
    }
}
