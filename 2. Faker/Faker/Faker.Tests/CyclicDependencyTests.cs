using Xunit;

namespace Faker.Tests;

public class A
{
    public B B { get; set; } = null!;
}

public class B
{
    public C C { get; set; } = null!;
}

public class C
{
    public A? A { get; set; }
}

public class CyclicDependencyTests
{
    [Fact]
    public void CreateClassWithCyclicDependency_ShouldHandleCyclicDependency()
    {
        var faker = new Faker();
        A obj = faker.Create<A>();
        
        Assert.NotNull(obj);
        Assert.NotNull(obj.B);
        Assert.NotNull(obj.B.C);
        Assert.Null(obj.B.C.A);
    }
}
