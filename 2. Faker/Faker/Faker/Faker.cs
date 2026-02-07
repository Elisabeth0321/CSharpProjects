using Faker.Generators;

namespace Faker;

public class Faker : IFaker
{
    private readonly List<IValueGenerator> _generators;
    private readonly Random _random;
    private readonly FakerConfig _config;
    private readonly HashSet<Type> _creatingTypes = new();

    public Faker() : this(null)
    {
    }

    public Faker(FakerConfig config)
    {
        _config = config;
        _random = new Random();
        _generators = new List<IValueGenerator>
        {
            new IntGenerator(),
            new LongGenerator(),
            new DoubleGenerator(),
            new FloatGenerator(),
            new ShortGenerator(),
            new ByteGenerator(),
            new BoolGenerator(),
            new StringGenerator(),
            new DateTimeGenerator(),
            new CollectionGenerator(),
            new ObjectGenerator()
        };
    }

    public T Create<T>()
    {
        return (T)Create(typeof(T));
    }

    public object Create(Type type)
    {
        if (_creatingTypes.Contains(type))
        {
            return GetDefaultValue(type);
        }

        _creatingTypes.Add(type);
        try
        {
            IValueGenerator generator = _generators.FirstOrDefault(g => g.CanGenerate(type));
            if (generator != null)
            {
                GeneratorContext context = new GeneratorContext(_random, this, _config);
                return generator.Generate(type, context);
            }
            return GetDefaultValue(type);
        }
        finally
        {
            _creatingTypes.Remove(type);
        }
    }


    private static object GetDefaultValue(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }
}
