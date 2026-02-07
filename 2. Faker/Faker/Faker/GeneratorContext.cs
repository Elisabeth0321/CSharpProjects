namespace Faker;

public class GeneratorContext
{
    public Random Random { get; }
    public IFaker Faker { get; }
    public FakerConfig Config { get; }

    public GeneratorContext(Random random, IFaker faker, FakerConfig config = null)
    {
        Random = random;
        Faker = faker;
        Config = config;
    }
}
