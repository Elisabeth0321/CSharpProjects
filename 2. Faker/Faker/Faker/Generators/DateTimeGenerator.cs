namespace Faker.Generators;

public class DateTimeGenerator : IValueGenerator
{
    private static readonly DateTime MinDate = new DateTime(1970, 1, 1);
    private static readonly DateTime MaxDate = new DateTime(2100, 12, 31);

    public object Generate(Type typeToGenerate, GeneratorContext context)
    {
        long ticks = context.Random.NextInt64(MinDate.Ticks, MaxDate.Ticks);
        return new DateTime(ticks);
    }

    public bool CanGenerate(Type type)
    {
        return type == typeof(DateTime);
    }
}
