namespace Monopoly;

public class Dice
{
    private readonly Random _random;

    public int Value1 { get; protected set; }
    public int Value2 { get; protected set; }

    public Dice() : this(new Random())
    {
    }

    public Dice(Random random)
    {
        _random = random;
        GameTrace.Write(nameof(Dice), "Создан объект для броска двух кубиков (Dice).");
    }

    // virtual, чтобы в тестах можно было подставить кубики с нужными значениями
    public virtual int Roll()
    {
        Value1 = _random.Next(1, 7);
        Value2 = _random.Next(1, 7);
        GameTrace.Write(nameof(Dice), $"Выпало {Value1} и {Value2} на кубиках (Dice).");
        return GetSum();
    }

    public int GetSum()
    {
        int sum = Value1 + Value2;
        GameTrace.Write(nameof(Dice), $"Сумма кубиков (Dice) — {sum}.");
        return sum;
    }

    internal void Reset()
    {
        Value1 = 0;
        Value2 = 0;
        GameTrace.Write(nameof(Dice), "Значения кубиков (Dice) сброшены.");
    }
}
