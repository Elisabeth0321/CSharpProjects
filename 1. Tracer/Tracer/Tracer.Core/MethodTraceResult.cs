namespace Tracer.Core;

public sealed class MethodTraceResult
{
    public string Name { get; }
    public string Class { get; }
    public long TimeMs { get; }
    public IReadOnlyList<MethodTraceResult> Methods { get; }

    public MethodTraceResult(string name, string @class, long timeMs, IReadOnlyList<MethodTraceResult> methods)
    {
        Name = name;
        Class = @class;
        TimeMs = timeMs;
        Methods = methods;
    }
}