namespace Faker;

public interface IValueGenerator
{
    object Generate(Type typeToGenerate, GeneratorContext context);
    bool CanGenerate(Type type);
}
