using Xunit;

namespace Faker.Tests;

public class User
{
    public string Name { get; set; } = null!;
    public int Age { get; set; }
}

public class UserClassTests
{
    [Fact]
    public void CreateUser_ShouldCreateUserWithFilledProperties()
    {
        var faker = new Faker();
        var user = faker.Create<User>();
        
        Assert.NotNull(user);
        Assert.NotNull(user.Name);
        Assert.NotEmpty(user.Name);
        Assert.True(user.Age >= int.MinValue && user.Age <= int.MaxValue);
    }

    [Fact]
    public void CreateMultipleUsers_ShouldCreateDifferentUsers()
    {
        var faker = new Faker();
        var user1 = faker.Create<User>();
        var user2 = faker.Create<User>();
        
        Assert.NotNull(user1);
        Assert.NotNull(user2);
    }
}
