namespace Tracer.Core;

public sealed class TraceResult
{
    public IReadOnlyList<ThreadTraceResult> Threads { get; }

    public TraceResult(IReadOnlyList<ThreadTraceResult> threads)
    {
        Threads = threads;
    }
}