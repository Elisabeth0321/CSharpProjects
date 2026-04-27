using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGenerator.Core.Advanced;

internal sealed class ConstructorPlan
{
    private ConstructorPlan(ClassDeclarationSyntax classSyntax, string subjectFieldName, IReadOnlyList<ConstructorParameterBinding> parameters)
    {
        ClassSyntax = classSyntax;
        SubjectFieldName = subjectFieldName;
        Parameters = parameters;
    }

    internal ClassDeclarationSyntax ClassSyntax { get; }

    internal string SubjectFieldName { get; }

    internal IReadOnlyList<ConstructorParameterBinding> Parameters { get; }

    internal bool RequiresMock => Parameters.Any(p => p.IsInterface);

    internal static ConstructorPlan Create(ClassDeclarationSyntax classSyntax)
    {
        string className = classSyntax.Identifier.ValueText;
        string subjectField = BuildSubjectFieldName(className);
        ConstructorDeclarationSyntax? ctor = classSyntax.Members
            .OfType<ConstructorDeclarationSyntax>()
            .Where(c => c.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
            .OrderByDescending(c => c.ParameterList.Parameters.Count)
            .FirstOrDefault();
        List<ConstructorParameterBinding> bindings = new List<ConstructorParameterBinding>();
        if (ctor is not null)
        {
            foreach (ParameterSyntax parameter in ctor.ParameterList.Parameters)
            {
                TypeSyntax parameterType = parameter.Type ?? SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));
                string parameterName = parameter.Identifier.ValueText;
                string fieldName = $"_{parameterName}";
                bool isInterface = TypeSyntaxHeuristics.IsLikelyInterfaceType(parameterType);
                bindings.Add(new ConstructorParameterBinding(parameterType, parameterName, fieldName, isInterface));
            }
        }
        return new ConstructorPlan(classSyntax, subjectField, bindings);
    }

    internal IEnumerable<MemberDeclarationSyntax> BuildFieldDeclarations()
    {
        foreach (ConstructorParameterBinding binding in Parameters.Where(p => p.IsInterface))
        {
            GenericNameSyntax mockGeneric = SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("Mock"),
                SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(binding.ParameterType)));
            yield return SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(mockGeneric)
                    .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(binding.FieldName))))
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
        }
        IdentifierNameSyntax classIdentifier = SyntaxFactory.IdentifierName(ClassSyntax.Identifier);
        yield return SyntaxFactory.FieldDeclaration(
            SyntaxFactory.VariableDeclaration(classIdentifier)
                .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(SubjectFieldName))))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
    }

    internal MemberDeclarationSyntax BuildInitializationMember()
    {
        BlockSyntax body = BuildInitializationBody();
        return SyntaxFactory.MethodDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
            "SetUp")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(body)
            .WithAttributeLists(
                SyntaxFactory.SingletonList(
                    SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("SetUp"))))));
    }

    private BlockSyntax BuildInitializationBody()
    {
        List<StatementSyntax> bodyStatements = new List<StatementSyntax>();
        foreach (ConstructorParameterBinding binding in Parameters.Where(p => p.IsInterface))
        {
            GenericNameSyntax mockGeneric = SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("Mock"),
                SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(binding.ParameterType)));
            ObjectCreationExpressionSyntax newMock = SyntaxFactory.ObjectCreationExpression(mockGeneric)
                .WithArgumentList(SyntaxFactory.ArgumentList());
            StatementSyntax assignMock = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(binding.FieldName),
                    newMock));
            bodyStatements.Add(assignMock);
        }
        List<ArgumentSyntax> ctorArgs = new List<ArgumentSyntax>();
        foreach (ConstructorParameterBinding binding in Parameters)
        {
            ExpressionSyntax argumentExpression = binding.IsInterface
                ? SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(binding.FieldName),
                    SyntaxFactory.IdentifierName("Object"))
                : DefaultParameterValueSyntaxFactory.CreateDefaultValueForType(binding.ParameterType);
            ctorArgs.Add(SyntaxFactory.Argument(argumentExpression));
        }
        IdentifierNameSyntax typeName = SyntaxFactory.IdentifierName(ClassSyntax.Identifier);
        ObjectCreationExpressionSyntax newSubject = SyntaxFactory.ObjectCreationExpression(typeName)
            .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(ctorArgs)));
        StatementSyntax assignSubject = SyntaxFactory.ExpressionStatement(
            SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(SubjectFieldName),
                newSubject));
        bodyStatements.Add(assignSubject);
        return SyntaxFactory.Block(SyntaxFactory.List(bodyStatements));
    }

    private static string BuildSubjectFieldName(string className)
    {
        if (string.IsNullOrEmpty(className))
        {
            return "_targetUnderTest";
        }
        string camel = char.ToLowerInvariant(className[0]) + className.Substring(1);
        return $"_{camel}UnderTest";
    }
}
