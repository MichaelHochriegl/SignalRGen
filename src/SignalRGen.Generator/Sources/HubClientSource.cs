using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SignalRGen.Generator.Sources;

internal static class HubClientSource
{
    public static SourceText GetSourceText(
        HubClientToGenerate hubClientToGenerate)
    {
        var memberList =
            new List<MemberDeclarationSyntax>()
            {
                CreateCtor(hubClientToGenerate)
            };
        foreach (var clientMethod in hubClientToGenerate.Methods)
        {
            memberList.Add(CreateEventHandler(clientMethod));
            memberList.Add(CreateClientHandlerMethod(clientMethod));
        }

        memberList.Add(CreateRegisterHubMethods(hubClientToGenerate));

        return CompilationUnit()
            .WithUsings(
                List<UsingDirectiveSyntax>(hubClientToGenerate.Usings.Append(
                    UsingDirective(
                        QualifiedName(
                            QualifiedName(
                                QualifiedName(
                                    IdentifierName("Microsoft"),
                                    IdentifierName("AspNetCore")),
                                IdentifierName("SignalR")),
                            IdentifierName("Client")))).ToArray()))
            .WithMembers(
                SingletonList<MemberDeclarationSyntax>(
                    FileScopedNamespaceDeclaration(
                            QualifiedName(
                                QualifiedName(
                                    QualifiedName(
                                        IdentifierName("iCMS"),
                                        IdentifierName("FM")),
                                    IdentifierName("SignalR")),
                                IdentifierName("Contracts")))
                        .WithMembers(
                            SingletonList<MemberDeclarationSyntax>(
                                ClassDeclaration(hubClientToGenerate.HubName)
                                    .WithModifiers(
                                        TokenList(
                                            Token(SyntaxKind
                                                .PublicKeyword)))
                                    .WithBaseList(
                                        BaseList(
                                            SingletonSeparatedList<
                                                BaseTypeSyntax>(
                                                SimpleBaseType(
                                                    IdentifierName(
                                                        "HubClient")))))
                                    .WithMembers(
                                        List<MemberDeclarationSyntax>(
                                            memberList))))))
            .NormalizeWhitespace().GetText(Encoding.UTF8);
    }

    private static FieldDeclarationSyntax CreateEventHandler(
        MethodDeclarationSyntax clientMethod)
    {
        var parameterTypes =
            clientMethod.ParameterList.Parameters.Select(p =>
                p.Type.ToString());
        var funcGenerics = new SyntaxNodeOrTokenList();
        foreach (var parameterType in parameterTypes)
        {
            funcGenerics = funcGenerics.Add(IdentifierName(parameterType));
            funcGenerics = funcGenerics.Add(Token(SyntaxKind.CommaToken));
        }

        funcGenerics = funcGenerics.Add(IdentifierName("Task"));

        return FieldDeclaration(
                VariableDeclaration(
                        GenericName(
                                Identifier("Func"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SeparatedList<TypeSyntax>(funcGenerics))))
                    .WithVariables(
                        SingletonSeparatedList<
                            VariableDeclaratorSyntax>(
                            VariableDeclarator(
                                    Identifier(
                                        $"On{clientMethod.Identifier.ToString()}"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        LiteralExpression(
                                            SyntaxKind
                                                .DefaultLiteralExpression,
                                            Token(SyntaxKind
                                                .DefaultKeyword)))))))
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword)));
    }

    private static MethodDeclarationSyntax CreateClientHandlerMethod(
        MethodDeclarationSyntax clientMethod)
    {
        var handlerInvokation = InvocationExpression(
            MemberAccessExpression(
                SyntaxKind
                    .SimpleMemberAccessExpression,
                IdentifierName($"On{clientMethod.Identifier.ToString()}"),
                IdentifierName(
                    "Invoke")));

        var eventHandlerArguments =
            clientMethod.ParameterList.Parameters.Select(p => p.Identifier)
                .ToList();

        var argumentList = new SyntaxNodeOrTokenList();
        if (eventHandlerArguments.Any())
        {
            argumentList.Add(
                Argument(IdentifierName(eventHandlerArguments.First())));
            foreach (var argument in eventHandlerArguments.Skip(1))
            {
                argumentList.Add(Token(SyntaxKind.CommaToken));
                argumentList.Add(Argument(IdentifierName(argument)));
            }

            if (argumentList.Count > 1)
            {
                handlerInvokation = handlerInvokation.WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(argumentList)));
            }
            else
            {
                handlerInvokation = handlerInvokation.WithArgumentList(
                    ArgumentList(SingletonSeparatedList<ArgumentSyntax>(
                        Argument(
                            IdentifierName(eventHandlerArguments
                                .First())))));
            }
        }

        return MethodDeclaration(
                IdentifierName(
                    "Task"),
                Identifier($"{clientMethod.Identifier.ToString()}Handler"))
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind
                        .PrivateKeyword)))
            .WithParameterList(clientMethod.ParameterList)
            .WithBody(
                Block(
                    SingletonList<
                        StatementSyntax>(
                        ReturnStatement(handlerInvokation))));
    }

    private static ConstructorDeclarationSyntax CreateCtor(
        HubClientToGenerate hubClient)
    {
        return ConstructorDeclaration(
                Identifier(
                    hubClient.HubName))
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind
                        .PublicKeyword)))
            .WithParameterList(
                ParameterList(
                    SingletonSeparatedList
                        <ParameterSyntax>(
                            Parameter(
                                    Identifier(
                                        "hubConnection"))
                                .WithType(
                                    IdentifierName(
                                        "HubConnection")))))
            .WithInitializer(
                ConstructorInitializer(
                    SyntaxKind
                        .BaseConstructorInitializer,
                    ArgumentList(
                        SingletonSeparatedList
                            <ArgumentSyntax>(
                                Argument(
                                    IdentifierName(
                                        "hubConnection"))))))
            .WithBody(
                Block());
    }

    private static MethodDeclarationSyntax CreateRegisterHubMethods(
        HubClientToGenerate hubClientToGenerate)
    {
        var methodRegistration = new SyntaxList<ExpressionStatementSyntax>();
        foreach (var clientMethod in hubClientToGenerate.Methods)
        {
            var method = CreateOnMethod(clientMethod);
            methodRegistration = methodRegistration.Add(method);
        }

        return MethodDeclaration(
                PredefinedType(
                    Token(SyntaxKind
                        .VoidKeyword)),
                Identifier(
                    "RegisterHubMethods"))
            .WithModifiers(
                TokenList(
                    new[]
                    {
                        Token(
                            SyntaxKind
                                .ProtectedKeyword),
                        Token(
                            SyntaxKind
                                .OverrideKeyword)
                    }))
            .WithBody(
                Block(methodRegistration));
    }

    private static ExpressionStatementSyntax CreateOnMethod(
        MethodDeclarationSyntax clientMethod)
    {
        TypeArgumentListSyntax onGenericList;
        var parameterTypes =
            clientMethod.ParameterList.Parameters.Select(p =>
                p.Type).ToList();
        if (parameterTypes.Count > 1)
        {
            var argumentList = new SyntaxNodeOrTokenList();
            argumentList = argumentList.Add(parameterTypes.First());
            foreach (var type in parameterTypes.Skip(1))
            {
                argumentList = argumentList.Add(Token(SyntaxKind.CommaToken));
                argumentList = argumentList.Add(type);
            }

            onGenericList =
                TypeArgumentList(SeparatedList<TypeSyntax>(argumentList));
        }
        else
        {
            onGenericList = TypeArgumentList(
                SingletonSeparatedList<TypeSyntax>(parameterTypes.First()));
        }

        return ExpressionStatement(
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind
                            .SimpleMemberAccessExpression,
                        IdentifierName(
                            "HubConnection"),
                        GenericName(
                                Identifier(
                                    "On"))
                            .WithTypeArgumentList(onGenericList)))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList
                            <ArgumentSyntax>(
                                new
                                    SyntaxNodeOrToken
                                    []
                                    {
                                        Argument(
                                            LiteralExpression(
                                                SyntaxKind
                                                    .StringLiteralExpression,
                                                Literal(
                                                    clientMethod.Identifier
                                                        .ToString()))),
                                        Token(
                                            SyntaxKind
                                                .CommaToken),
                                        Argument(
                                            IdentifierName(
                                                $"{clientMethod.Identifier.ToString()}Handler"))
                                    }))));
    }
}