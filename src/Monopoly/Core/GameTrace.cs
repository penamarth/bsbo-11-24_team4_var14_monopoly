using System.Runtime.CompilerServices;

namespace Monopoly;

internal static class GameTrace
{
    public static void StartBlock(string title, bool first = false)
    {
        if (!first)
        {
            Separate();
            Separate();
        }

        Console.WriteLine($"=== {title} ===");
    }

    public static void StartSection(string title)
    {
        Separate();
        Console.WriteLine($"  -- {title} --");
    }

    public static void Separate()
    {
        Console.WriteLine();
    }

    public static void Write(string className, string message, [CallerMemberName] string methodName = "")
    {
        Console.WriteLine(Format(className, methodName, message));
    }

    public static void Prompt(string className, string message, [CallerMemberName] string methodName = "")
    {
        Console.Write(Format(className, methodName, message));
    }

    private static string Format(string className, string methodName, string message)
    {
        string operation = methodName == ".ctor" ? $"new {className}()" : $"{className}.{methodName}()";
        return $"    [{operation}] {message}";
    }

    public static string Describe(Player player) => $"{player.Name} ({player.GetType().Name})";

    public static string Describe(Square square) =>
        $"«{square.DisplayName}» ({square.GetType().Name}, №{square.Position})";

    public static string Describe(Deck deck) => $"«{deck.Name}» ({deck.GetType().Name})";

    public static string Describe(Card card) => $"«{card.Description}» ({card.GetType().Name})";
}
