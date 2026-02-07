using System.Reflection;
using Tracer.Serialization.Abstractions;

namespace Tracer.Serialization;

public static class PluginLoader
{
    public static IReadOnlyList<ITraceResultSerializer> Load(string dir)
    {
        var list = new List<ITraceResultSerializer>();

        if (!Directory.Exists(dir))
            return list;

        foreach (var file in Directory.GetFiles(dir, "*.dll"))
        {
            var asm = Assembly.Load(File.ReadAllBytes(file));

            foreach (var t in asm.GetTypes())
            {
                if (t.IsAbstract || t.IsInterface)
                    continue;

                if (typeof(ITraceResultSerializer).IsAssignableFrom(t))
                    list.Add((ITraceResultSerializer)Activator.CreateInstance(t)!);
            }
        }

        return list;
    }
}