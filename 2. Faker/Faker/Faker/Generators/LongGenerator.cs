namespace Faker.Generators;

public class LongGenerator : IValueGenerator
{
    public object Generate(Type typeToGenerate, GeneratorContext context)
    {
        long value = context.Random.NextInt64(long.MinValue, long.MaxValue);
        return value;
    }

    public bool CanGenerate(Type type)
    {
        return type == typeof(long);
    }
}
