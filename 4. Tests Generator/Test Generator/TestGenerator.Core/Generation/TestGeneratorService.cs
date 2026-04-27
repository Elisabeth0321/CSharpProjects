using System.Threading.Tasks.Dataflow;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestGenerator.Core.Analysis;
using TestGenerator.Core.Generation;
using TestGenerator.Core.Pipeline;

namespace TestGenerator.Core;

public static class TestGeneratorService
{
    public static async Task GenerateAsync(GeneratorOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ValidateOptions(options);
        Directory.CreateDirectory(options.OutputDirectory);
        TransformBlock<string, ClassFile> loadBlock = CreateLoadBlock(options, cancellationToken);
        TransformManyBlock<ClassFile, WorkItem> extractBlock = CreateExtractBlock(cancellationToken);
        TransformBlock<WorkItem, TestFile> generateBlock = CreateGenerateBlock(options, cancellationToken);
        ActionBlock<TestFile> writeBlock = CreateWriteBlock(options, cancellationToken);
        LinkPipeline(loadBlock, extractBlock, generateBlock, writeBlock);
        await PostInputPathsAndCompleteAsync(loadBlock, options.InputFilePaths, writeBlock, cancellationToken)
            .ConfigureAwait(false);
    }

    private static void ValidateOptions(GeneratorOptions options)
    {
        if (options.InputFilePaths.Count == 0)
        {
            throw new ArgumentException("At least one input file path is required.", nameof(options));
        }
        if (string.IsNullOrWhiteSpace(options.OutputDirectory))
        {
            throw new ArgumentException("Output directory is required.", nameof(options));
        }
        ValidateDegree(nameof(options.MaxConcurrentReads), options.MaxConcurrentReads);
        ValidateDegree(nameof(options.MaxConcurrentGeneration), options.MaxConcurrentGeneration);
        ValidateDegree(nameof(options.MaxConcurrentWrites), options.MaxConcurrentWrites);
    }

    private static ExecutionDataflowBlockOptions CreateParallelismOptions(int maxDegreeOfParallelism, CancellationToken cancellationToken)
    {
        return new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };
    }

    private static TransformBlock<string, ClassFile> CreateLoadBlock(GeneratorOptions options, CancellationToken cancellationToken)
    {
        return new TransformBlock<string, ClassFile>(
            async filePath =>
            {
                string content = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
                return new ClassFile(filePath, content);
            },
            CreateParallelismOptions(options.MaxConcurrentReads, cancellationToken));
    }

    private static TransformManyBlock<ClassFile, WorkItem> CreateExtractBlock(CancellationToken cancellationToken)
    {
        return new TransformManyBlock<ClassFile, WorkItem>(
            loaded =>
            {
                SyntaxTree tree = CSharpSyntaxTree.ParseText(loaded.Content, path: loaded.FilePath);
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot(cancellationToken);
                return ClassExtractor.ExtractWorkItems(loaded.FilePath, root);
            });
    }

    private static TransformBlock<WorkItem, TestFile> CreateGenerateBlock(GeneratorOptions options, CancellationToken cancellationToken)
    {
        return new TransformBlock<WorkItem, TestFile>(
            work => TestCompilationUnitGenerator.Generate(work, options),
            CreateParallelismOptions(options.MaxConcurrentGeneration, cancellationToken));
    }

    private static ActionBlock<TestFile> CreateWriteBlock(GeneratorOptions options, CancellationToken cancellationToken)
    {
        return new ActionBlock<TestFile>(
            async file =>
            {
                string fullPath = Path.Combine(options.OutputDirectory, file.RelativeFileName);
                await File.WriteAllTextAsync(fullPath, file.Content, cancellationToken).ConfigureAwait(false);
            },
            CreateParallelismOptions(options.MaxConcurrentWrites, cancellationToken));
    }

    private static void LinkPipeline(
        TransformBlock<string, ClassFile> loadBlock,
        TransformManyBlock<ClassFile, WorkItem> extractBlock,
        TransformBlock<WorkItem, TestFile> generateBlock,
        ActionBlock<TestFile> writeBlock)
    {
        DataflowLinkOptions linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        loadBlock.LinkTo(extractBlock, linkOptions);
        extractBlock.LinkTo(generateBlock, linkOptions);
        generateBlock.LinkTo(writeBlock, linkOptions);
    }

    private static async Task PostInputPathsAndCompleteAsync(
        TransformBlock<string, ClassFile> loadBlock,
        IReadOnlyList<string> inputFilePaths,
        ActionBlock<TestFile> writeBlock,
        CancellationToken cancellationToken)
    {
        foreach (string path in inputFilePaths)
        {
            bool posted = await loadBlock.SendAsync(path, cancellationToken).ConfigureAwait(false);
            if (!posted)
            {
                break;
            }
        }
        loadBlock.Complete();
        await writeBlock.Completion.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateDegree(string name, int value)
    {
        if (value < 1)
        {
            throw new ArgumentOutOfRangeException(name, value, "Degree must be at least 1.");
        }
    }
}
