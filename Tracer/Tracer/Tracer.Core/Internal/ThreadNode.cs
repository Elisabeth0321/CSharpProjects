namespace Tracer.Core.Internal;

internal sealed class ThreadNode
{
    public int ThreadId { get; }
    public long TotalRootMs { get; private set; }
    public List<MethodNode> Roots { get; } = new();
    public object Sync { get; } = new();

    public ThreadNode(int threadId)
    {
        ThreadId = threadId;
    }

    public void AddRoot(MethodNode node)
    {
        Roots.Add(node);
        TotalRootMs += node.ElapsedMs;
    }
}