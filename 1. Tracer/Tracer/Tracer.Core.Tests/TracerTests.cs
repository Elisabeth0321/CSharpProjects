using Xunit;

namespace Tracer.Core.Tests;

public sealed class TracerTests
{
    [Fact]
    public void StartTrace_ShouldRecordMethodInfo()
    {
        var tracer = new Tracer();
        tracer.StartTrace();
        Thread.Sleep(50);
        tracer.StopTrace();

        var result = tracer.GetTraceResult();
        Assert.Single(result.Threads);
        Assert.Single(result.Threads[0].Methods);
        Assert.NotEmpty(result.Threads[0].Methods[0].Name);
        Assert.NotEmpty(result.Threads[0].Methods[0].Class);
        Assert.True(result.Threads[0].Methods[0].TimeMs >= 50);
    }

    [Fact]
    public void NestedMethods_ShouldBeRecordedCorrectly()
    {
        var tracer = new Tracer();
        
        tracer.StartTrace();
        Thread.Sleep(50);
        InnerMethod(tracer);
        Thread.Sleep(50);
        tracer.StopTrace();

        var result = tracer.GetTraceResult();
        Assert.Single(result.Threads);
        Assert.Single(result.Threads[0].Methods);
        
        var outerMethod = result.Threads[0].Methods[0];
        Assert.Equal(nameof(NestedMethods_ShouldBeRecordedCorrectly), outerMethod.Name);
        Assert.Single(outerMethod.Methods);
        
        var innerMethod = outerMethod.Methods[0];
        Assert.Equal(nameof(InnerMethod), innerMethod.Name);
        Assert.True(outerMethod.TimeMs >= innerMethod.TimeMs);
    }

    [Fact]
    public void MultipleThreads_ShouldBeRecordedSeparately()
    {
        var tracer = new Tracer();
        var threads = new List<Thread>();

        for (int i = 0; i < 3; i++)
        {
            int threadIndex = i;
            var thread = new Thread(() =>
            {
                tracer.StartTrace();
                Thread.Sleep(50);
                tracer.StopTrace();
            });
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        var result = tracer.GetTraceResult();
        Assert.Equal(3, result.Threads.Count);
    }

    [Fact]
    public void MultipleRootMethods_ShouldBeRecordedAtSameLevel()
    {
        var tracer = new Tracer();
        
        Method1(tracer);
        Method2(tracer);

        var result = tracer.GetTraceResult();
        Assert.Single(result.Threads);
        Assert.Equal(2, result.Threads[0].Methods.Count);
        Assert.Equal(nameof(Method1), result.Threads[0].Methods[0].Name);
        Assert.Equal(nameof(Method2), result.Threads[0].Methods[1].Name);
    }

    [Fact]
    public void ThreadTime_ShouldBeSumOfRootMethods()
    {
        var tracer = new Tracer();
        
        tracer.StartTrace();
        Thread.Sleep(100);
        tracer.StopTrace();
        
        tracer.StartTrace();
        Thread.Sleep(200);
        tracer.StopTrace();

        var result = tracer.GetTraceResult();
        Assert.Single(result.Threads);
        var threadResult = result.Threads[0];
        Assert.True(threadResult.TimeMs >= 300);
        Assert.Equal(threadResult.Methods[0].TimeMs + threadResult.Methods[1].TimeMs, threadResult.TimeMs);
    }

    private static void InnerMethod(ITracer tracer)
    {
        tracer.StartTrace();
        Thread.Sleep(30);
        tracer.StopTrace();
    }

    private static void Method1(ITracer tracer)
    {
        tracer.StartTrace();
        Thread.Sleep(50);
        tracer.StopTrace();
    }

    private static void Method2(ITracer tracer)
    {
        tracer.StartTrace();
        Thread.Sleep(50);
        tracer.StopTrace();
    }
}

