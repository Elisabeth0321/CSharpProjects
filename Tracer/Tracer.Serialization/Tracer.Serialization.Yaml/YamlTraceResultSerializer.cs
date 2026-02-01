using Tracer.Core;
using Tracer.Serialization.Abstractions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Tracer.Serialization.Yaml;

public sealed class YamlTraceResultSerializer : ITraceResultSerializer
{
    public string Format => "yaml";

    public void Serialize(TraceResult traceResult, Stream to)
    {
        var dto = DtoMapper.Map(traceResult);

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        using var writer = new StreamWriter(to, leaveOpen: true);
        serializer.Serialize(writer, dto);
        writer.Flush();
    }

    internal static class DtoMapper
    {
        internal static RootDto Map(TraceResult r) =>
            new() { Threads = r.Threads.Select(Map).ToList() };

        private static ThreadDto Map(ThreadTraceResult t) =>
            new()
            {
                Id = t.Id,
                Time = $"{t.TimeMs}ms",
                Methods = t.Methods.Select(Map).ToList()
            };

        private static MethodDto Map(MethodTraceResult m) =>
            new()
            {
                Name = m.Name,
                Class = m.Class,
                Time = $"{m.TimeMs}ms",
                Methods = m.Methods.Select(Map).ToList()
            };

        internal sealed class RootDto { public List<ThreadDto> Threads { get; set; } = new(); }
        internal sealed class ThreadDto { public int Id { get; set; } public string Time { get; set; } = ""; public List<MethodDto> Methods { get; set; } = new(); }
        internal sealed class MethodDto { public string Name { get; set; } = ""; public string Class { get; set; } = ""; public string Time { get; set; } = ""; public List<MethodDto> Methods { get; set; } = new(); }
    }
}