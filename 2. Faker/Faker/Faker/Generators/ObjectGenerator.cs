using System.Reflection;

namespace Faker.Generators;

public class ObjectGenerator : IValueGenerator
{
    public object Generate(Type typeToGenerate, GeneratorContext context)
    {
        if (typeToGenerate.IsValueType && !typeToGenerate.IsPrimitive && typeToGenerate != typeof(decimal))
        {
            return GenerateStruct(typeToGenerate, context);
        }

        return GenerateClass(typeToGenerate, context);
    }

    public bool CanGenerate(Type type)
    {
        return !type.IsPrimitive && 
               type != typeof(string) && 
               type != typeof(decimal) &&
               !type.IsEnum &&
               (type.IsClass || (type.IsValueType && !type.IsPrimitive));
    }

    private object GenerateStruct(Type structType, GeneratorContext context)
    {
        ConstructorInfo[] constructors = structType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        
        if (constructors.Length == 0)
        {
            return Activator.CreateInstance(structType);
        }

        ConstructorInfo bestConstructor = GetBestConstructor(constructors);
        object instance = TryCreateInstanceWithConstructor(bestConstructor, structType, context);
        
        if (instance == null)
        {
            instance = Activator.CreateInstance(structType);
        }

        FillFieldsAndProperties(instance, structType, context, bestConstructor);
        return instance;
    }

    private object GenerateClass(Type classType, GeneratorContext context)
    {
        ConstructorInfo[] constructors = classType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        
        if (constructors.Length == 0)
        {
            try
            {
                return Activator.CreateInstance(classType);
            }
            catch (MissingMethodException)
            {
                return null;
            }
        }

        ConstructorInfo bestConstructor = GetBestConstructor(constructors);
        object instance = TryCreateInstanceWithConstructor(bestConstructor, classType, context);
        
        if (instance == null)
        {
            foreach (ConstructorInfo constructor in constructors.OrderByDescending(c => c.GetParameters().Length))
            {
                if (constructor == bestConstructor) continue;
                instance = TryCreateInstanceWithConstructor(constructor, classType, context);
                if (instance != null)
                {
                    FillFieldsAndProperties(instance, classType, context, constructor);
                    return instance;
                }
            }
            return null;
        }

        FillFieldsAndProperties(instance, classType, context, bestConstructor);
        return instance;
    }

    private ConstructorInfo GetBestConstructor(ConstructorInfo[] constructors)
    {
        return constructors.OrderByDescending(c => c.GetParameters().Length).First();
    }

    private object TryCreateInstanceWithConstructor(ConstructorInfo constructor, Type type, GeneratorContext context)
    {
        try
        {
            ParameterInfo[] parameters = constructor.GetParameters();
            object[] parameterValues = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                Type parameterType = parameters[i].ParameterType;
                IValueGenerator customGenerator = context.Config?.GetGenerator(type, parameters[i].Name, parameterType);
                
                if (customGenerator != null)
                {
                    parameterValues[i] = customGenerator.Generate(parameterType, context);
                }
                else
                {
                    parameterValues[i] = context.Faker.Create(parameterType);
                }
            }

            return constructor.Invoke(parameterValues);
        }
        catch
        {
            return null;
        }
    }

    private void FillFieldsAndProperties(object instance, Type type, GeneratorContext context, ConstructorInfo usedConstructor)
    {
        HashSet<string> filledMembers = GetFilledMembers(usedConstructor);
        
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            if (!filledMembers.Contains(field.Name))
            {
                IValueGenerator customGenerator = context.Config?.GetGenerator(type, field.Name, field.FieldType);
                object value = customGenerator != null 
                    ? customGenerator.Generate(field.FieldType, context)
                    : context.Faker.Create(field.FieldType);
                field.SetValue(instance, value);
            }
        }

        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            if (property.CanWrite && property.SetMethod.IsPublic && !filledMembers.Contains(property.Name))
            {
                IValueGenerator customGenerator = context.Config?.GetGenerator(type, property.Name, property.PropertyType);
                object value = customGenerator != null 
                    ? customGenerator.Generate(property.PropertyType, context)
                    : context.Faker.Create(property.PropertyType);
                property.SetValue(instance, value);
            }
        }
    }

    private HashSet<string> GetFilledMembers(ConstructorInfo constructor)
    {
        HashSet<string> filledMembers = new HashSet<string>();
        ParameterInfo[] parameters = constructor.GetParameters();
        
        foreach (ParameterInfo parameter in parameters)
        {
            filledMembers.Add(parameter.Name);
        }

        return filledMembers;
    }
}
