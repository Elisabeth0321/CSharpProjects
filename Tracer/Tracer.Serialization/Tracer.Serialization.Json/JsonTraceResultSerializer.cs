using System.Text.Json;
using Tracer.Core;
using Tracer.Serialization.Abstractions;

namespace Tracer.Serialization.Json;

public sealed class JsonTraceResultSerializer : ITraceResultSerializer
{
    public string Format => "json";

    public void Serialize(TraceResult traceResult, Stream to)
    {
        var dto = DtoMapper.Map(traceResult);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        JsonSerializer.Serialize(to, dto, options);
    }
}

// DTO + маппер
internal static class DtoMapper
{
    internal static RootDto Map(TraceResult r) =>
        new(r.Threads.Select(Map).ToList());

    private static ThreadDto Map(ThreadTraceResult t) =>
        new(t.Id.ToString(), $"{t.TimeMs}ms", t.Methods.Select(Map).ToList());

    private static MethodDto Map(MethodTraceResult m) =>
        new(m.Name, m.Class, $"{m.TimeMs}ms", m.Methods.Select(Map).ToList());

    internal sealed record RootDto(List<ThreadDto> threads);
    internal sealed record ThreadDto(string id, string time, List<MethodDto> methods);
    internal sealed record MethodDto(string name, string @class, string time, List<MethodDto> methods);
}