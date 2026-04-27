using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGenerator.Core.Advanced;

internal static class DefaultParameterValueSyntaxFactory
{
    internal static ExpressionSyntax CreateDefaultValueForType(TypeSyntax typeSyntax)
    {
        TypeSyntax unwrapped = UnwrapNullable(typeSyntax);
        if (unwrapped is PredefinedTypeSyntax predefined)
        {
            SyntaxKind kind = predefined.Keyword.Kind();
            if (kind == SyntaxKind.StringKeyword)
            {
                return SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal(string.Empty));
            }
            return kind switch
            {
                SyntaxKind.BoolKeyword => SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression),
                SyntaxKind.ByteKeyword or SyntaxKind.SByteKeyword or SyntaxKind.ShortKeyword or SyntaxKind.UShortKeyword
                    or SyntaxKind.IntKeyword or SyntaxKind.UIntKeyword or SyntaxKind.LongKeyword or SyntaxKind.ULongKeyword
                    or SyntaxKind.FloatKeyword or SyntaxKind.DoubleKeyword => SyntaxFactory.LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    SyntaxFactory.Literal(0)),
                SyntaxKind.DecimalKeyword => SyntaxFactory.LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    SyntaxFactory.Literal(0m)),
                SyntaxKind.CharKeyword => SyntaxFactory.LiteralExpression(
                    SyntaxKind.CharacterLiteralExpression,
                    SyntaxFactory.Literal('\0')),
                _ => SyntaxFactory.DefaultExpression(unwrapped)
            };
        }
        return SyntaxFactory.DefaultExpression(unwrapped);
    }

    private static TypeSyntax UnwrapNullable(TypeSyntax typeSyntax)
    {
        if (typeSyntax is NullableTypeSyntax nullable)
        {
            return nullable.ElementType;
        }
        return typeSyntax;
    }
}
