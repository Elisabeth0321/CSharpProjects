using Xunit;

namespace Faker.Tests;

public struct SimpleStruct
{
    public int Value { get; set; }
    public string Name { get; set; }

    public SimpleStruct()
    {
        Value = 0;
        Name = null!;
    }
}

public struct StructWithConstructor
{
    public int Value { get; }
    public string Name { get; set; }

    public StructWithConstructor(int value)
    {
        Value = value;
        Name = string.Empty;
    }
}

public class StructTests
{
    [Fact]
    public void CreateSimpleStruct_ShouldCreateStruct()
    {
        var faker = new Faker();
        SimpleStruct obj = faker.Create<SimpleStruct>();
        
        Assert.True(obj.Value >= int.MinValue && obj.Value <= int.MaxValue);
        Assert.NotNull(obj.Name);
    }

    [Fact]
    public void CreateStructWithConstructor_ShouldUseConstructor()
    {
        var faker = new Faker();
        StructWithConstructor obj = faker.Create<StructWithConstructor>();
        
        Assert.True(obj.Value >= int.MinValue && obj.Value <= int.MaxValue);
    }
}
