namespace Faker;

public interface IFaker
{
    T Create<T>();
    object Create(Type type);
}
