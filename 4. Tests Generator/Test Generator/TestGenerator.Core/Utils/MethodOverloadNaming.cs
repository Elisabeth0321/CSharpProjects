using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGenerator.Core.Analysis;

internal static class MethodOverloadNaming
{
    internal sealed record PublicMethodInfo(MethodDeclarationSyntax Method, string TestMethodName);

    internal static IReadOnlyList<PublicMethodInfo> BuildPublicMethodInfos(ClassDeclarationSyntax classSyntax)
    {
        List<MethodDeclarationSyntax> methods = classSyntax.Members
            .OfType<MethodDeclarationSyntax>()
            .Where(IsPublicInstanceOrStaticMethod)
            .OrderBy(m => m.SpanStart)
            .ToList();
        List<PublicMethodInfo> result = new List<PublicMethodInfo>();
        ILookup<string, MethodDeclarationSyntax> byName = methods.ToLookup(m => m.Identifier.ValueText);
        foreach (IGrouping<string, MethodDeclarationSyntax> group in byName.OrderBy(g => g.Min(m => m.SpanStart)))
        {
            IOrderedEnumerable<MethodDeclarationSyntax> ordered = group.OrderBy(m => m.SpanStart);
            List<MethodDeclarationSyntax> list = ordered.ToList();
            int index = 0;
            foreach (MethodDeclarationSyntax method in list)
            {
                index++;
                string testName = BuildTestName(group.Key, list.Count, index);
                result.Add(new PublicMethodInfo(method, testName));
            }
        }
        return result;
    }

    private static string BuildTestName(string methodName, int overloadCount, int oneBasedIndex)
    {
        if (overloadCount <= 1)
        {
            return $"{methodName}Test";
        }
        return $"{methodName}{oneBasedIndex}Test";
    }

    private static bool IsPublicInstanceOrStaticMethod(MethodDeclarationSyntax method)
    {
        if (!method.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
        {
            return false;
        }
        if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
        {
            return false;
        }
        return true;
    }
}
