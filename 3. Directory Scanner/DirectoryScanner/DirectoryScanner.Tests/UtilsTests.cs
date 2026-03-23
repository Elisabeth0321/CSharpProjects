using DirectoryScanner.Core;

namespace DirectoryScanner.Tests;

public class UtilsTests
{
    [Fact]
    public void CalculateSize_ReturnsSumForNestedDirectories()
    {
        var rootNode = new Node
        {
            Name = "root",
            FullPath = "root",
            IsDirectory = true
        };
        var subDirectoryNode = new Node
        {
            Name = "nested",
            FullPath = "root/nested",
            IsDirectory = true
        };
        subDirectoryNode.AddChild(new Node
        {
            Name = "nested.txt",
            FullPath = "root/nested/nested.txt",
            Size = 30,
            IsDirectory = false
        });
        rootNode.AddChild(new Node
        {
            Name = "root.txt",
            FullPath = "root/root.txt",
            Size = 20,
            IsDirectory = false
        });
        rootNode.AddChild(subDirectoryNode);

        long actualSize = Utils.CalculateSize(rootNode);

        Assert.Equal(50, actualSize);
        Assert.Equal(50, rootNode.Size);
        Assert.Equal(30, subDirectoryNode.Size);
    }

    [Fact]
    public void CalculatePercentage_SetsPercentageForChildren()
    {
        var rootNode = new Node
        {
            Name = "root",
            FullPath = "root",
            IsDirectory = true,
            Size = 100
        };
        var fileNode = new Node
        {
            Name = "file.txt",
            FullPath = "root/file.txt",
            IsDirectory = false,
            Size = 25
        };
        var nestedDirectoryNode = new Node
        {
            Name = "nested",
            FullPath = "root/nested",
            IsDirectory = true,
            Size = 75
        };
        nestedDirectoryNode.AddChild(new Node
        {
            Name = "nested.txt",
            FullPath = "root/nested/nested.txt",
            IsDirectory = false,
            Size = 75
        });
        rootNode.AddChild(fileNode);
        rootNode.AddChild(nestedDirectoryNode);

        Utils.CalculatePercentage(rootNode);

        Assert.Equal(25, fileNode.Percentage);
        Assert.Equal(75, nestedDirectoryNode.Percentage);
        Assert.Equal(100, nestedDirectoryNode.Children[0].Percentage);
    }

    [Fact]
    public void CalculatePercentage_WhenParentSizeIsZero_LeavesChildrenPercentageAsZero()
    {
        var rootNode = new Node
        {
            Name = "root",
            FullPath = "root",
            IsDirectory = true,
            Size = 0
        };
        var fileNode = new Node
        {
            Name = "file.txt",
            FullPath = "root/file.txt",
            IsDirectory = false,
            Size = 10
        };
        rootNode.AddChild(fileNode);

        Utils.CalculatePercentage(rootNode);

        Assert.Equal(0, fileNode.Percentage);
    }
}
