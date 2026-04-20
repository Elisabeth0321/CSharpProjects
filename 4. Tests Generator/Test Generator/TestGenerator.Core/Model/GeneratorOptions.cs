namespace TestGenerator.Core;

public sealed record GeneratorOptions(
    IReadOnlyList<string> InputFilePaths,
    string OutputDirectory,
    int MaxConcurrentReads,
    int MaxConcurrentGeneration,
    int MaxConcurrentWrites,
    bool UseAdvancedDependencySetup);
