namespace DirectoryScanner.Core;

public class Utils
{
    public static long CalculateSize(Node node)
    {
        if (!node.IsDirectory)
            return node.Size;

        long size = 0;

        foreach (var child in node.Children)
            size += CalculateSize(child);

        node.Size = size;

        return size;
    }
    
    public static void CalculatePercentage(Node node)
    {
        foreach (var child in node.Children)
        {
            if (node.Size > 0)
                child.Percentage = (double)child.Size / node.Size * 100;

            CalculatePercentage(child);
        }
    }
}