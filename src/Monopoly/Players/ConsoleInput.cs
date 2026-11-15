using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Monopoly;

[ExcludeFromCodeCoverage]
public class ConsoleInput : IInput
{
    public bool EndOfInput { get; private set; }

    public ConsoleInput()
    {
        GameTrace.Write(nameof(ConsoleInput), "Создан объект для ввода с клавиатуры (ConsoleInput).");
    }

    public bool AskYesNo(string question)
    {
        while (true)
        {
            string? line = ReadAnswer(question + " (д/н): ");
            if (line == null)
            {
                return false; // ввод закончился
            }

            string answer = line.Trim().ToLower();

            if (answer == "д" || answer == "y" || answer == "да")
            {
                return true;
            }

            if (answer == "н" || answer == "n" || answer == "нет")
            {
                return false;
            }

            GameTrace.Write(nameof(ConsoleInput), "Введите д или н.");
        }
    }

    public int AskNumber(string question)
    {
        while (true)
        {
            string? answer = ReadAnswer(question);
            if (answer == null)
            {
                return 0; // ввод закончился
            }

            if (int.TryParse(answer, out int number))
            {
                return number;
            }

            GameTrace.Write(nameof(ConsoleInput), "Нужно ввести число.");
        }
    }

    public string AskText(string question)
    {
        return (ReadAnswer(question) ?? "").Trim();
    }

    private string? ReadAnswer(string question, [CallerMemberName] string methodName = "")
    {
        GameTrace.Prompt(nameof(ConsoleInput), question, methodName);
        string? answer = Console.ReadLine();
        EndOfInput = answer == null;
        if (EndOfInput || Console.IsInputRedirected || Console.IsOutputRedirected)
        {
            GameTrace.Separate();
        }

        return answer;
    }
}
