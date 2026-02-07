namespace Faker.Generators;

public class DoubleGenerator : IValueGenerator
{
    public object Generate(Type typeToGenerate, GeneratorContext context)
    {
        double value = (context.Random.NextDouble() * 2 - 1) * double.MaxValue;
        return value;
    }

    public bool CanGenerate(Type type)
    {
        return type == typeof(double);
    }
}
