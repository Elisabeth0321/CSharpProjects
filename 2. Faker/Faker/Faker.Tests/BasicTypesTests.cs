using Xunit;

namespace Faker.Tests;

public class BasicTypesTests
{
    [Fact]
    public void CreateInt_ShouldReturnRandomInt()
    {
        var faker = new Faker();
        int value = faker.Create<int>();
        Assert.True(value >= int.MinValue && value <= int.MaxValue);
    }

    [Fact]
    public void CreateLong_ShouldReturnRandomLong()
    {
        var faker = new Faker();
        long value = faker.Create<long>();
        Assert.True(value >= long.MinValue && value <= long.MaxValue);
    }

    [Fact]
    public void CreateDouble_ShouldReturnRandomDouble()
    {
        var faker = new Faker();
        double value = faker.Create<double>();
        Assert.True(value >= double.MinValue && value <= double.MaxValue);
    }

    [Fact]
    public void CreateFloat_ShouldReturnRandomFloat()
    {
        var faker = new Faker();
        float value = faker.Create<float>();
        Assert.True(value >= float.MinValue && value <= float.MaxValue);
    }

    [Fact]
    public void CreateString_ShouldReturnNonEmptyString()
    {
        var faker = new Faker();
        string value = faker.Create<string>();
        Assert.NotNull(value);
        Assert.NotEmpty(value);
    }

    [Fact]
    public void CreateBool_ShouldReturnBool()
    {
        var faker = new Faker();
        bool value = faker.Create<bool>();
        Assert.True(value == true || value == false);
    }

    [Fact]
    public void CreateDateTime_ShouldReturnValidDateTime()
    {
        var faker = new Faker();
        DateTime value = faker.Create<DateTime>();
        Assert.True(value >= new DateTime(1970, 1, 1));
        Assert.True(value <= new DateTime(2100, 12, 31));
    }
}
