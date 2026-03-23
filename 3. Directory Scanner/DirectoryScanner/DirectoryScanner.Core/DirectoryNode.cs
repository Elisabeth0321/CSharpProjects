namespace DirectoryScanner.Core;

public class Node
{
    private readonly List<Node> _children = new();

    private readonly object _childrenLock = new();

    public string Name { get; set; }

    public string FullPath { get; set; }

    public bool IsDirectory { get; set; }

    public long Size { get; set; }

    public double Percentage { get; set; }

    public IReadOnlyList<Node> Children
    {
        get
        {
            lock (_childrenLock)
            {
                return _children.ToArray();
            }
        }
    }

    public void AddChild(Node child)
    {
        ArgumentNullException.ThrowIfNull(child);
        lock (_childrenLock)
        {
            _children.Add(child);
        }
    }
}