using System.Linq.Expressions;

namespace Faker;

public class FakerConfig
{
    private readonly Dictionary<Type, Dictionary<string, IValueGenerator>> _generators = new();

    public void Add<TClass, TProperty, TGenerator>(Expression<Func<TClass, TProperty>> expression) 
        where TGenerator : IValueGenerator, new()
    {
        if (expression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Expression must be a member access expression");
        }

        string memberName = memberExpression.Member.Name;
        Type classType = typeof(TClass);

        if (!_generators.ContainsKey(classType))
        {
            _generators[classType] = new Dictionary<string, IValueGenerator>();
        }

        TGenerator generator = new TGenerator();
        _generators[classType][memberName] = generator;
    }

    public IValueGenerator? GetGenerator(Type classType, string memberName, Type memberType)
    {
        if (!_generators.TryGetValue(classType, out var classGenerators))
            return null;

        if (classGenerators.TryGetValue(memberName, out var generator) && generator.CanGenerate(memberType))
            return generator;

        var key = classGenerators.Keys.FirstOrDefault(k =>
            string.Equals(k, memberName, StringComparison.OrdinalIgnoreCase));
        if (key != null && classGenerators.TryGetValue(key, out generator) && generator.CanGenerate(memberType))
            return generator;

        return null;
    }
}
