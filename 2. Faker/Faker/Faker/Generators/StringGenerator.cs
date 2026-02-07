namespace Faker.Generators;

public class StringGenerator : IValueGenerator
{
    private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int MinLength = 5;
    private const int MaxLength = 20;

    public object Generate(Type typeToGenerate, GeneratorContext context)
    {
        int length = context.Random.Next(MinLength, MaxLength + 1);
        char[] chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = Characters[context.Random.Next(Characters.Length)];
        }
        return new string(chars);
    }

    public bool CanGenerate(Type type)
    {
        return type == typeof(string);
    }
}
