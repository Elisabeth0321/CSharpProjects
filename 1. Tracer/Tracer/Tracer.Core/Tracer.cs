using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Tracer.Core.Internal;

namespace Tracer.Core;

public sealed class Tracer : ITracer
{
    private readonly ConcurrentDictionary<int, ThreadNode> _threads = new();
    private readonly ThreadLocal<Stack<MethodNode>> _callStack = new(() => new Stack<MethodNode>());

    public void StartTrace()
    {
        var st = new StackTrace(1, false);
        MethodBase? mb = st.GetFrame(0)?.GetMethod();

        var methodName = mb?.Name ?? "Unknown";
        var className = mb?.DeclaringType?.Name ?? "Unknown";

        var node = new MethodNode(methodName, className);
        node.Stopwatch.Start();

        _callStack.Value!.Push(node);
    }

    public void StopTrace()
    {
        var stack = _callStack.Value!;
        var node = stack.Pop();
        node.Stop();

        if (stack.Count > 0)
        {
            stack.Peek().Children.Add(node);
            return;
        }

        int id = Environment.CurrentManagedThreadId;
        var threadNode = _threads.GetOrAdd(id, x => new ThreadNode(x));

        lock (threadNode.Sync)
        {
            threadNode.AddRoot(node);
        }
    }

    public TraceResult GetTraceResult()
    {
        var threads = _threads.Values
            .Select(t =>
            {
                List<MethodNode> roots;
                long time;

                lock (t.Sync)
                {
                    roots = t.Roots.ToList();
                    time = t.TotalRootMs;
                }

                var methods = roots.Select(ToResult).ToList().AsReadOnly();
                return new ThreadTraceResult(t.ThreadId, time, methods);
            })
            .ToList()
            .AsReadOnly();

        return new TraceResult(threads);
    }

    private static MethodTraceResult ToResult(MethodNode n)
    {
        var children = n.Children.Select(ToResult).ToList().AsReadOnly();
        return new MethodTraceResult(n.Name, n.Class, n.ElapsedMs, children);
    }
}
