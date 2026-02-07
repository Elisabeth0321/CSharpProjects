using System.Diagnostics;

namespace Tracer.Core.Internal;

internal sealed class MethodNode
{
    public string Name { get; }
    public string Class { get; }
    public Stopwatch Stopwatch { get; } = new();
    public long ElapsedMs { get; private set; }
    public List<MethodNode> Children { get; } = new();

    public MethodNode(string name, string @class)
    {
        Name = name;
        Class = @class;
    }

    public void Stop()
    {
        Stopwatch.Stop();
        ElapsedMs = Stopwatch.ElapsedMilliseconds;
    }
}