using System.Collections;

namespace Faker.Generators;

public class CollectionGenerator : IValueGenerator
{
    private const int MinSize = 2;
    private const int MaxSize = 10;

    public object Generate(Type typeToGenerate, GeneratorContext context)
    {
        if (!CanGenerate(typeToGenerate))
        {
            throw new ArgumentException($"Cannot generate collection of type {typeToGenerate}");
        }

        Type elementType = GetElementType(typeToGenerate);
        int size = context.Random.Next(MinSize, MaxSize + 1);

        if (typeToGenerate.IsArray)
        {
            Array array = Array.CreateInstance(elementType, size);
            for (int i = 0; i < size; i++)
            {
                object element = context.Faker.Create(elementType);
                array.SetValue(element, i);
            }
            return array;
        }

        if (typeToGenerate.IsGenericType)
        {
            Type genericTypeDefinition = typeToGenerate.GetGenericTypeDefinition();
            Type[] genericArguments = typeToGenerate.GetGenericArguments();

            if (genericTypeDefinition == typeof(List<>) ||
                genericTypeDefinition == typeof(IList<>) ||
                genericTypeDefinition == typeof(ICollection<>) ||
                genericTypeDefinition == typeof(IEnumerable<>))
            {
                Type listType = typeof(List<>).MakeGenericType(elementType);
                IList list = (IList)Activator.CreateInstance(listType);
                for (int i = 0; i < size; i++)
                {
                    object element = context.Faker.Create(elementType);
                    list.Add(element);
                }
                return list;
            }
        }

        throw new ArgumentException($"Unsupported collection type: {typeToGenerate}");
    }

    public bool CanGenerate(Type type)
    {
        if (type.IsArray)
        {
            return true;
        }

        if (type.IsGenericType)
        {
            Type genericTypeDefinition = type.GetGenericTypeDefinition();
            return genericTypeDefinition == typeof(List<>) ||
                   genericTypeDefinition == typeof(IList<>) ||
                   genericTypeDefinition == typeof(ICollection<>) ||
                   genericTypeDefinition == typeof(IEnumerable<>);
        }

        return false;
    }

    private Type GetElementType(Type collectionType)
    {
        if (collectionType.IsArray)
        {
            return collectionType.GetElementType();
        }

        if (collectionType.IsGenericType)
        {
            return collectionType.GetGenericArguments()[0];
        }

        throw new ArgumentException($"Cannot determine element type for {collectionType}");
    }
}
