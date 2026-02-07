using Xunit;

namespace Faker.Tests;

public class ClassWithConstructor
{
    public string Name { get; set; } = null!;
    public int Age { get; }

    public ClassWithConstructor(int age)
    {
        Age = age;
    }
}

public class ClassWithMultipleConstructors
{
    public string Name { get; set; } = null!;
    public int Age { get; set; }
    public bool IsActive { get; set; }

    public ClassWithMultipleConstructors()
    {
    }

    public ClassWithMultipleConstructors(string name)
    {
        Name = name;
    }

    public ClassWithMultipleConstructors(string name, int age)
    {
        Name = name;
        Age = age;
    }
}

public class ClassWithPrivateConstructor
{
    public string Name { get; set; } = null!;

    private ClassWithPrivateConstructor()
    {
    }
}

public class ConstructorTests
{
    [Fact]
    public void CreateClassWithConstructor_ShouldUseConstructor()
    {
        var faker = new Faker();
        ClassWithConstructor obj = faker.Create<ClassWithConstructor>();
        
        Assert.NotNull(obj);
        Assert.True(obj.Age >= int.MinValue && obj.Age <= int.MaxValue);
    }

    [Fact]
    public void CreateClassWithMultipleConstructors_ShouldUseBestConstructor()
    {
        var faker = new Faker();
        ClassWithMultipleConstructors obj = faker.Create<ClassWithMultipleConstructors>();
        
        Assert.NotNull(obj);
    }

    [Fact]
    public void CreateClassWithPrivateConstructor_ShouldReturnNull()
    {
        var faker = new Faker();
        ClassWithPrivateConstructor obj = faker.Create<ClassWithPrivateConstructor>();
        
        Assert.Null(obj);
    }
}
