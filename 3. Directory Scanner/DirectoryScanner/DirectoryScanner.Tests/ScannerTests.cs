using DirectoryScanner.Core;

namespace DirectoryScanner.Tests;

public class DirectoryScannerTests
{
    public static TheoryData<int> WorkerCornerCaseCounts => new()
    {
        1,
        Math.Max(2, Environment.ProcessorCount * 2),
        100
    };

    [Fact]
    public async Task ScanAsync_ReturnsNestedTree()
    {
        string rootPath = CreateTempDirectory();

        try
        {
            File.WriteAllText(Path.Combine(rootPath, "root.txt"), "12345");
            string subDirectoryPath = Path.Combine(rootPath, "nested");
            Directory.CreateDirectory(subDirectoryPath);
            File.WriteAllText(Path.Combine(subDirectoryPath, "nested.txt"), "1234567890");

            var scanner = new Scanner(4);
            var token = new CancellationTokenSource();
            Node result = await scanner.ScanAsync(rootPath, token.Token);

            Assert.NotNull(result);
            Assert.Equal(2, result.Children.Count);
            Assert.Contains(result.Children, node => node.IsDirectory && node.Name == "nested");
        }
        finally
        {
            Directory.Delete(rootPath, true);
        }
    }

    [Fact]
    public async Task ScanAsync_CalculatesDirectorySize()
    {
        string rootPath = CreateTempDirectory();

        try
        {
            File.WriteAllText(Path.Combine(rootPath, "small.txt"), "1234");
            string subDirectoryPath = Path.Combine(rootPath, "nested");
            Directory.CreateDirectory(subDirectoryPath);
            File.WriteAllText(Path.Combine(subDirectoryPath, "big.txt"), "1234567890");

            var scanner = new Scanner(4);
            var token = new CancellationTokenSource();
            Node result = await scanner.ScanAsync(rootPath, token.Token);
            Node nestedDirectory = Assert.Single(result.Children, child => child.IsDirectory);

            Assert.Equal(14, result.Size);
            Assert.Equal(10, nestedDirectory.Size);
        }
        finally
        {
            Directory.Delete(rootPath, true);
        }
    }

    [Fact]
    public async Task ScanAsync_CalculatesPercentageForChildren()
    {
        string rootPath = CreateTempDirectory();

        try
        {
            File.WriteAllText(Path.Combine(rootPath, "small.txt"), "1234");
            string subDirectoryPath = Path.Combine(rootPath, "nested");
            Directory.CreateDirectory(subDirectoryPath);
            File.WriteAllText(Path.Combine(subDirectoryPath, "big.txt"), "1234567890");

            var scanner = new Scanner(4);
            var token = new CancellationTokenSource();
            Node result = await scanner.ScanAsync(rootPath, token.Token);
            Node nestedDirectory = Assert.Single(result.Children, child => child.IsDirectory);
            Node rootFile = Assert.Single(result.Children, child => !child.IsDirectory);

            Assert.Equal(71.43, nestedDirectory.Percentage, 2);
            Assert.Equal(28.57, rootFile.Percentage, 2);
        }
        finally
        {
            Directory.Delete(rootPath, true);
        }
    }

    [Fact]
    public async Task ScanAsync_WhenCancelled_ReturnsCollectedData()
    {
        var scanner = new Scanner(4);
        var cts = new CancellationTokenSource();
        cts.Cancel();
        string rootPath = CreateTempDirectory();

        try
        {
            Node result = await scanner.ScanAsync(rootPath, cts.Token);
            Assert.NotNull(result);
            Assert.True(result.IsDirectory);
        }
        finally
        {
            Directory.Delete(rootPath, true);
        }
    }

    [Theory]
    [MemberData(nameof(WorkerCornerCaseCounts), MemberType = typeof(DirectoryScannerTests))]
    public async Task ScanAsync_ForWorkerCornerCases_ProducesSameTotalSize(int workerCount)
    {
        string rootPath = CreateTempDirectoryWithNestedFiles();

        try
        {
            var scanner = new Scanner(workerCount);
            var tokenSource = new CancellationTokenSource();
            Node result = await scanner.ScanAsync(rootPath, tokenSource.Token);

            Assert.Equal(15, result.Size);
            Assert.Contains(result.Children, node => node is { IsDirectory: true, Name: "nested" });
            Assert.Contains(result.Children, node => node is { IsDirectory: false, Name: "root.txt" });
        }
        finally
        {
            Directory.Delete(rootPath, true);
        }
    }

    [Fact]
    public async Task ScanAsync_WhenCancelledMidScan_CompletesAndReturnsRootWithKnownName()
    {
        string rootPath = CreateWideDirectory(branchCount: 40);

        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var scanner = new Scanner(4);
            Task<Node> scanTask = scanner.ScanAsync(rootPath, cancellationTokenSource.Token);
            await Task.Delay(30);
            cancellationTokenSource.Cancel();
            Node result = await scanTask;

            Assert.NotNull(result);
            Assert.True(result.IsDirectory);
            Assert.Equal(Path.GetFileName(rootPath), result.Name);
        }
        finally
        {
            Directory.Delete(rootPath, true);
        }
    }

    private static string CreateTempDirectory()
    {
        string temporaryDirectoryPath = Path.Combine(
            Path.GetTempPath(),
            "DirectoryScannerTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectoryPath);
        return temporaryDirectoryPath;
    }

    private static string CreateTempDirectoryWithNestedFiles()
    {
        string rootPath = CreateTempDirectory();
        File.WriteAllText(Path.Combine(rootPath, "root.txt"), "12345");
        string subDirectoryPath = Path.Combine(rootPath, "nested");
        Directory.CreateDirectory(subDirectoryPath);
        File.WriteAllText(Path.Combine(subDirectoryPath, "nested.txt"), "1234567890");
        return rootPath;
    }

    private static string CreateWideDirectory(int branchCount)
    {
        string rootPath = CreateTempDirectory();
        for (int index = 0; index < branchCount; index++)
        {
            string branchPath = Path.Combine(rootPath, "d" + index.ToString());
            Directory.CreateDirectory(branchPath);
            File.WriteAllText(Path.Combine(branchPath, "f.txt"), "x");
        }

        return rootPath;
    }
}