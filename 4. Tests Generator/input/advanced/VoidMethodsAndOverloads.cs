namespace AdvancedSamples;

public interface ILogger
{
}

public class CalculatorClass
{
    private readonly ILogger _logger;

    public CalculatorClass(ILogger logger)
    {
        _logger = logger;
    }

    public void Reset()
    {
    }

    public static int Add(int a, int b)
    {
        return a + b;
    }

    public double Scale(double value)
    {
        return value;
    }

    public double Scale(int value)
    {
        return value;
    }
}
