namespace Faker.Generators;

public class FloatGenerator : IValueGenerator
{
    public object Generate(Type typeToGenerate, GeneratorContext context)
    {
        float value = (float)((context.Random.NextDouble() * 2 - 1) * float.MaxValue);
        return value;
    }

    public bool CanGenerate(Type type)
    {
        return type == typeof(float);
    }
}
