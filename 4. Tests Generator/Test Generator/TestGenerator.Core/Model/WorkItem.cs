using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGenerator.Core.Pipeline;

internal sealed record WorkItem(
    string SourceFilePath,
    ClassDeclarationSyntax ClassSyntax,
    NamespaceDeclarationSyntax? NamespaceSyntax,
    FileScopedNamespaceDeclarationSyntax? FileScopedNamespaceSyntax);
