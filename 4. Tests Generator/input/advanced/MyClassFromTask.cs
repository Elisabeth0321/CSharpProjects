namespace MyCode;

public interface IDependency
{
}

public class Foo
{
}

public class ClassWithDependency
{
    private readonly IDependency _dependency;

    public ClassWithDependency(IDependency dependency)
    {
        _dependency = dependency;
    }

    public int MyMethod(int number, string s, Foo foo)
    {
        return number;
    }
}
