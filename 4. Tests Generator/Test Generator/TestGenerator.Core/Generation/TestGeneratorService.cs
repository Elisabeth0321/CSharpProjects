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
        Directory.CreateDirectory(options.OutputDirectory);
        ExecutionDataflowBlockOptions loadOptions = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = options.MaxConcurrentReads,
            CancellationToken = cancellationToken
        };
        TransformBlock<string, ClassFile> loadBlock = new TransformBlock<string, ClassFile>(
            async filePath =>
            {
                string content = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
                return new ClassFile(filePath, content);
            },
            loadOptions);
        TransformManyBlock<ClassFile, WorkItem> extractBlock =
            new TransformManyBlock<ClassFile, WorkItem>(
                loaded =>
                {
                    SyntaxTree tree = CSharpSyntaxTree.ParseText(loaded.Content, path: loaded.FilePath);
                    CompilationUnitSyntax root = tree.GetCompilationUnitRoot(cancellationToken);
                    return ClassExtractor.ExtractWorkItems(loaded.FilePath, root);
                });
        ExecutionDataflowBlockOptions generateOptions = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = options.MaxConcurrentGeneration,
            CancellationToken = cancellationToken
        };
        TransformBlock<WorkItem, TestFile> generateBlock =
            new TransformBlock<WorkItem, TestFile>(
                work => TestCompilationUnitGenerator.Generate(work, options),
                generateOptions);
        ExecutionDataflowBlockOptions writeOptions = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = options.MaxConcurrentWrites,
            CancellationToken = cancellationToken
        };
        ActionBlock<TestFile> writeBlock = new ActionBlock<TestFile>(
            async file =>
            {
                string fullPath = Path.Combine(options.OutputDirectory, file.RelativeFileName);
                await File.WriteAllTextAsync(fullPath, file.Content, cancellationToken).ConfigureAwait(false);
            },
            writeOptions);
        loadBlock.LinkTo(extractBlock, new DataflowLinkOptions { PropagateCompletion = true });
        extractBlock.LinkTo(generateBlock, new DataflowLinkOptions { PropagateCompletion = true });
        generateBlock.LinkTo(writeBlock, new DataflowLinkOptions { PropagateCompletion = true });
        foreach (string path in options.InputFilePaths)
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
