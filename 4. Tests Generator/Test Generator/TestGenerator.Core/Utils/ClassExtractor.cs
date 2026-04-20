using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestGenerator.Core.Pipeline;

namespace TestGenerator.Core.Analysis;

internal static class ClassExtractor
{
    private const string TestClassSuffix = "Tests";

    internal static IReadOnlyList<WorkItem> ExtractWorkItems(
        string sourceFilePath,
        CompilationUnitSyntax root)
    {
        List<WorkItem> items = new List<WorkItem>();
        foreach (ClassDeclarationSyntax classSyntax in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            if (!IsEligibleTopLevelOrNestedPublicClass(classSyntax))
            {
                continue;
            }
            NamespaceDeclarationSyntax? namespaceSyntax = classSyntax.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            FileScopedNamespaceDeclarationSyntax? fileScoped = classSyntax.Ancestors().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
            items.Add(new WorkItem(sourceFilePath, classSyntax, namespaceSyntax, fileScoped));
        }
        return items;
    }

    internal static string GetTestsNamespace(WorkItem workItem)
    {
        string originalNamespace = GetOriginalNamespace(workItem);
        return $"{originalNamespace}.Tests";
    }

    internal static string GetOriginalNamespace(WorkItem workItem)
    {
        if (workItem.FileScopedNamespaceSyntax is not null)
        {
            return workItem.FileScopedNamespaceSyntax.Name.ToString();
        }
        if (workItem.NamespaceSyntax is not null)
        {
            return workItem.NamespaceSyntax.Name.ToString();
        }
        return "Global";
    }

    internal static string GetTestClassName(ClassDeclarationSyntax classSyntax)
    {
        return $"{classSyntax.Identifier.ValueText}{TestClassSuffix}";
    }

    internal static string GetOutputFileName(ClassDeclarationSyntax classSyntax)
    {
        return $"{GetTestClassName(classSyntax)}.cs";
    }

    private static bool IsEligibleTopLevelOrNestedPublicClass(ClassDeclarationSyntax classSyntax)
    {
        if (!classSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
        {
            return false;
        }
        if (classSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
        {
            return false;
        }
        if (classSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
        {
            return false;
        }
        return true;
    }
}
