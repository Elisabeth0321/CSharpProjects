using Xunit;

namespace Faker.Tests;

public class Foo
{
    public string City { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public class Person
{
    public string Name { get; }

    public Person(string name)
    {
        Name = name;
    }
}

public class CityGenerator : IValueGenerator
{
    public object Generate(Type typeToGenerate, GeneratorContext context)
    {
        return "CustomCity";
    }

    public bool CanGenerate(Type type)
    {
        return type == typeof(string);
    }
}

public class NameGenerator : IValueGenerator
{
    public object Generate(Type typeToGenerate, GeneratorContext context)
    {
        return "CustomName";
    }

    public bool CanGenerate(Type type)
    {
        return type == typeof(string);
    }
}

public class FakerConfigTests
{
    [Fact]
    public void CreateWithConfig_ShouldUseCustomGenerator()
    {
        var config = new FakerConfig();
        config.Add<Foo, string, CityGenerator>(foo => foo.City);
        var faker = new Faker(config);
        
        Foo foo = faker.Create<Foo>();
        
        Assert.NotNull(foo);
        Assert.Equal("CustomCity", foo.City);
    }

    [Fact]
    public void CreateImmutableObjectWithConfig_ShouldUseCustomGeneratorInConstructor()
    {
        var config = new FakerConfig();
        config.Add<Person, string, NameGenerator>(p => p.Name);
        var faker = new Faker(config);
        
        Person person = faker.Create<Person>();
        
        Assert.NotNull(person);
        Assert.Equal("CustomName", person.Name);
    }
}
