using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGenerator.Core.Advanced;

internal sealed record ConstructorParameterBinding(
    TypeSyntax ParameterType,
    string ParameterName,
    string FieldName,
    bool IsInterface);
