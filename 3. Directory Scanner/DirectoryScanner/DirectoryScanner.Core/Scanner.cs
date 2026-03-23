using System.Collections.Concurrent;

namespace DirectoryScanner.Core;

public class Scanner
{
    private readonly int _maxThreads;

    public Scanner(int maxThreads = 8)
    {
        _maxThreads = Math.Max(1, maxThreads);
    }

    public async Task<Node> ScanAsync(
        string path,
        CancellationToken token,
        Action<Node>? onScanStructureStarted = null,
        Action<Node, Node>? onChildAttached = null,
        Action<Node>? onMetricsRecalculated = null)
    {
        string rootName = Path.GetFileName(path);
        if (string.IsNullOrEmpty(rootName))
        {
            rootName = path;
        }

        var root = new Node
        {
            Name = rootName,
            FullPath = path,
            IsDirectory = true
        };

        onScanStructureStarted?.Invoke(root);

        var queue = new ConcurrentQueue<Node>();
        var queueSignal = new SemaphoreSlim(0);
        queue.Enqueue(root);
        queueSignal.Release();
        int pendingDirectories = 1;
        var workers = new List<Task>(_maxThreads);

        for (int i = 0; i < _maxThreads; i++)
        {
            workers.Add(Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await queueSignal.WaitAsync(token);
                    }
                    catch (OperationCanceledException)
                    {
                        DrainAbandonedQueue(queue, ref pendingDirectories);
                        return;
                    }

                    if (!queue.TryDequeue(out Node? currentDirectory))
                    {
                        if (Volatile.Read(ref pendingDirectories) == 0)
                        {
                            return;
                        }

                        if (token.IsCancellationRequested)
                        {
                            DrainAbandonedQueue(queue, ref pendingDirectories);
                            return;
                        }

                        continue;
                    }

                    if (token.IsCancellationRequested)
                    {
                        CompleteDirectoryJob(ref pendingDirectories, queueSignal);
                        continue;
                    }

                    ProcessDirectory(
                        currentDirectory,
                        queue,
                        queueSignal,
                        ref pendingDirectories,
                        token,
                        onChildAttached);

                    if (CompleteDirectoryJob(ref pendingDirectories, queueSignal))
                    {
                        return;
                    }
                }
            }, CancellationToken.None));
        }

        try
        {
            await Task.WhenAll(workers);
        }
        finally
        {
            Utils.CalculateSize(root);
            Utils.CalculatePercentage(root);
            onMetricsRecalculated?.Invoke(root);
        }

        return root;
    }
    
    private static void ProcessDirectory(
        Node dir,
        ConcurrentQueue<Node> queue,
        SemaphoreSlim queueSignal,
        ref int pendingDirectories,
        CancellationToken token,
        Action<Node, Node>? onChildAttached)
    {
        var directory = new DirectoryInfo(dir.FullPath);
        FileInfo[] files;
        DirectoryInfo[] directories;

        try
        {
            files = directory.GetFiles();
            directories = directory.GetDirectories();
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }
        catch (DirectoryNotFoundException)
        {
            return;
        }
        catch (IOException)
        {
            return;
        }

        foreach (var file in files)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (file.LinkTarget != null)
            {
                continue;
            }

            var fileNode = new Node
            {
                Name = file.Name,
                FullPath = file.FullName,
                Size = file.Length,
                IsDirectory = false
            };

            dir.AddChild(fileNode);
            onChildAttached?.Invoke(dir, fileNode);
        }

        foreach (var subDir in directories)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (subDir.LinkTarget != null)
            {
                continue;
            }

            var node = new Node
            {
                Name = subDir.Name,
                FullPath = subDir.FullName,
                IsDirectory = true
            };

            dir.AddChild(node);
            onChildAttached?.Invoke(dir, node);
            Interlocked.Increment(ref pendingDirectories);
            queue.Enqueue(node);
            queueSignal.Release();
        }
    }

    private static void DrainAbandonedQueue(ConcurrentQueue<Node> queue, ref int pendingDirectories)
    {
        while (queue.TryDequeue(out _))
        {
            Interlocked.Decrement(ref pendingDirectories);
        }
    }

    private bool CompleteDirectoryJob(ref int pendingDirectories, SemaphoreSlim queueSignal)
    {
        if (Interlocked.Decrement(ref pendingDirectories) == 0)
        {
            for (int releaseIndex = 0; releaseIndex < _maxThreads; releaseIndex++)
            {
                queueSignal.Release();
            }

            return true;
        }

        return false;
    }
}