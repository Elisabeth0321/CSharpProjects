namespace Faker.Generators;

public class ByteGenerator : IValueGenerator
{
    public object Generate(Type typeToGenerate, GeneratorContext context)
    {
        return (byte)context.Random.Next(byte.MinValue, byte.MaxValue + 1);
    }

    public bool CanGenerate(Type type)
    {
        return type == typeof(byte);
    }
}
