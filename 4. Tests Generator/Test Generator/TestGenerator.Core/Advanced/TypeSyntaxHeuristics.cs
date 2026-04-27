using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGenerator.Core.Advanced;

internal static class TypeSyntaxHeuristics
{
    internal static bool IsLikelyInterfaceType(TypeSyntax typeSyntax)
    {
        string name = GetSimpleTypeName(typeSyntax);
        if (string.IsNullOrEmpty(name) || name.Length < 2)
        {
            return false;
        }
        if (name[0] != 'I')
        {
            return false;
        }
        return char.IsUpper(name[1]);
    }

    private static string GetSimpleTypeName(TypeSyntax typeSyntax)
    {
        return typeSyntax switch
        {
            IdentifierNameSyntax id => id.Identifier.ValueText,
            GenericNameSyntax generic => generic.Identifier.ValueText,
            QualifiedNameSyntax qualified => GetSimpleTypeName(qualified.Right),
            AliasQualifiedNameSyntax alias => GetSimpleTypeName(alias.Name),
            NullableTypeSyntax nullable => GetSimpleTypeName(nullable.ElementType),
            _ => string.Empty
        };
    }
}
