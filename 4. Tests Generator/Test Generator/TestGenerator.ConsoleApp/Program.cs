using System.Text;
using TestGenerator.Core;

namespace Test_Generator;

internal static class Program
{
    private static async Task<int> Main()
    {
        ConfigureConsoleEncoding();
        await RunInteractiveAsync().ConfigureAwait(false);
        return 0;
    }

    private static void ConfigureConsoleEncoding()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = new UTF8Encoding(false);
    }

    private static async Task RunInteractiveAsync(CancellationToken cancellationToken = default)
    {
        List<string> inputFiles = new List<string>();
        bool addMore = true;
        while (addMore)
        {
            string path = ReadExistingSourceFilePath();
            inputFiles.Add(path);
            addMore = AskAddAnotherFile();
        }
        string outputDirectory = ReadOutputDirectoryPath();
        (int maxReads, int maxGen, int maxWrite) = ReadPipelineLimits();
        bool useAdvanced = AskUseAdvancedSetup();
        GeneratorOptions options = new GeneratorOptions(
            inputFiles,
            outputDirectory,
            maxReads,
            maxGen,
            maxWrite,
            useAdvanced);
        await TestGeneratorService.GenerateAsync(options, cancellationToken).ConfigureAwait(false);
        Console.WriteLine();
        Console.WriteLine("Генерация завершена успешно.");
    }

    private static string ReadExistingSourceFilePath()
    {
        while (true)
        {
            Console.WriteLine("Введите путь к исходному файлу:");
            string? line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                Console.WriteLine("Путь не может быть пустым.");
                continue;
            }
            string path = NormalizePath(line);
            if (!File.Exists(path))
            {
                Console.WriteLine("Файл не найден. Повторите ввод.");
                continue;
            }
            return Path.GetFullPath(path);
        }
    }

    private static bool AskAddAnotherFile()
    {
        while (true)
        {
            Console.WriteLine("Готово. Желаете добавить ещё 1 файл? (Y/N)");
            string? line = Console.ReadLine();
            if (IsYes(line))
            {
                return true;
            }
            if (IsNo(line))
            {
                return false;
            }
            Console.WriteLine("Введите Y или N.");
        }
    }

    private static string ReadOutputDirectoryPath()
    {
        while (true)
        {
            Console.WriteLine("Введите путь к папке для сохранения результата:");
            string? line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                Console.WriteLine("Путь не может быть пустым.");
                continue;
            }
            return Path.GetFullPath(NormalizePath(line));
        }
    }

    private static bool AskUseAdvancedSetup()
    {
        while (true)
        {
            Console.WriteLine("Включить расширенный режим (Moq, SetUp, шаблоны Arrange / Act / Assert)? (Y/N)");
            string? line = Console.ReadLine();
            if (IsYes(line))
            {
                return true;
            }
            if (IsNo(line))
            {
                return false;
            }
            Console.WriteLine("Введите Y или N.");
        }
    }

    private static (int MaxReads, int MaxGen, int MaxWrite) ReadPipelineLimits()
    {
        while (true)
        {
            Console.WriteLine(
                "Введите через пробел три ограничения: число одновременно загружаемых файлов, " +
                "максимальное число одновременно обрабатываемых задач генерации, " +
                "максимальное число одновременно записываемых файлов:");
            string? line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                Console.WriteLine("Нужно ввести ровно три положительных целых числа через пробел.");
                continue;
            }
            string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                Console.WriteLine("Нужно ровно три числа, разделённые пробелами.");
                continue;
            }
            if (!TryParsePositiveInt(parts[0], out int maxReads)
                || !TryParsePositiveInt(parts[1], out int maxGen)
                || !TryParsePositiveInt(parts[2], out int maxWrite))
            {
                Console.WriteLine("Каждое значение должно быть целым числом не меньше 1.");
                continue;
            }
            return (maxReads, maxGen, maxWrite);
        }
    }

    private static bool TryParsePositiveInt(string text, out int value)
    {
        if (!int.TryParse(text.Trim(), out value) || value < 1)
        {
            value = 0;
            return false;
        }
        return true;
    }

    private static string NormalizePath(string line)
    {
        return line.Trim().Trim('"');
    }

    private static bool IsYes(string? line)
    {
        return string.Equals(line?.Trim(), "Y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(line?.Trim(), "Д", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNo(string? line)
    {
        return string.Equals(line?.Trim(), "N", StringComparison.OrdinalIgnoreCase)
            || string.Equals(line?.Trim(), "Н", StringComparison.OrdinalIgnoreCase);
    }

}
