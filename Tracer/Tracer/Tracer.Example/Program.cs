using CoreTracer = Tracer.Core.Tracer;
using Tracer.Core;
using Tracer.Serialization;

var tracer = new CoreTracer();

var t1 = new Thread(() =>
{
    var foo = new Foo(tracer);
    foo.MyMethod();
});

var t2 = new Thread(() =>
{
    var bar = new Bar(tracer);
    bar.InnerMethod();
    
    tracer.StartTrace();
    Thread.Sleep(50);
    tracer.StopTrace();
});

t1.Start();
t2.Start();

t1.Join();
t2.Join();

var result = tracer.GetTraceResult();

var plugins = PluginLoader.Load("plugins");
Directory.CreateDirectory("out");

int i = 1;
foreach (var s in plugins)
{
    using var fs = File.Create($"out/result{i}.{s.Format}");
    s.Serialize(result, fs);
    i++;
}

public class Foo
{
    private readonly Bar _bar;
    private readonly ITracer _tracer;

    public Foo(ITracer tracer)
    {
        _tracer = tracer;
        _bar = new Bar(tracer);
    }

    public void MyMethod()
    {
        _tracer.StartTrace();
        Thread.Sleep(100);
        _bar.InnerMethod();
        Thread.Sleep(50);
        _tracer.StopTrace();
    }
}

public class Bar
{
    private readonly ITracer _tracer;

    public Bar(ITracer tracer)
    {
        _tracer = tracer;
    }

    public void InnerMethod()
    {
        _tracer.StartTrace();
        Thread.Sleep(75);
        _tracer.StopTrace();
    }
}