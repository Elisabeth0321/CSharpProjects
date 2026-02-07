using Xunit;

namespace Faker.Tests;

public class CollectionTests
{
    [Fact]
    public void CreateListOfInts_ShouldCreateListWithRandomInts()
    {
        var faker = new Faker();
        List<int> ints = faker.Create<List<int>>();
        
        Assert.NotNull(ints);
        Assert.NotEmpty(ints);
        foreach (int value in ints)
        {
            Assert.True(value >= int.MinValue && value <= int.MaxValue);
        }
    }

    [Fact]
    public void CreateListOfUsers_ShouldCreateListWithUsers()
    {
        var faker = new Faker();
        List<User> users = faker.Create<List<User>>();
        
        Assert.NotNull(users);
        Assert.NotEmpty(users);
        foreach (User user in users)
        {
            Assert.NotNull(user);
            Assert.NotNull(user.Name);
        }
    }

    [Fact]
    public void CreateNestedList_ShouldCreateNestedList()
    {
        var faker = new Faker();
        List<List<User>> lists = faker.Create<List<List<User>>>();
        
        Assert.NotNull(lists);
        Assert.NotEmpty(lists);
        foreach (List<User> list in lists)
        {
            Assert.NotNull(list);
            Assert.NotEmpty(list);
        }
    }

    [Fact]
    public void CreateArray_ShouldCreateArray()
    {
        var faker = new Faker();
        int[] array = faker.Create<int[]>();
        
        Assert.NotNull(array);
        Assert.NotEmpty(array);
    }
}
