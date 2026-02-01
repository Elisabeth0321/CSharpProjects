namespace Tracer.Core;

public sealed class ThreadTraceResult
{
    public int Id { get; }
    public long TimeMs { get; }
    public IReadOnlyList<MethodTraceResult> Methods { get; }

    public ThreadTraceResult(int id, long timeMs, IReadOnlyList<MethodTraceResult> methods)
    {
        Id = id;
        TimeMs = timeMs;
        Methods = methods;
    }
}