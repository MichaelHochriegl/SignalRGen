using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SignalRGen.Generator.Sources;

internal static class HubClientAttributeSource
{
    internal static SourceText GetSource() => CompilationUnit()
        .WithMembers(
            SingletonList<MemberDeclarationSyntax>(
                FileScopedNamespaceDeclaration(
                        QualifiedName(
                            IdentifierName("SignalRGen"),
                            IdentifierName("Generator")))
                    .WithMembers(
                        SingletonList<MemberDeclarationSyntax>(
                            ClassDeclaration("HubClientAttribute")
                                .WithAttributeLists(
                                    SingletonList<AttributeListSyntax>(
                                        AttributeList(
                                            SingletonSeparatedList<AttributeSyntax>(
                                                Attribute(
                                                        QualifiedName(
                                                            IdentifierName("System"),
                                                            IdentifierName("AttributeUsage")))
                                                    .WithArgumentList(
                                                        AttributeArgumentList(
                                                            SingletonSeparatedList<AttributeArgumentSyntax>(
                                                                AttributeArgument(
                                                                    MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            IdentifierName("System"),
                                                                            IdentifierName("AttributeTargets")),
                                                                        IdentifierName("Interface"))))))))))
                                .WithModifiers(
                                    TokenList(
                                        new[]
                                        {
                                            Token(SyntaxKind.PublicKeyword),
                                            Token(SyntaxKind.SealedKeyword)
                                        }))
                                .WithBaseList(
                                    BaseList(
                                        SingletonSeparatedList<BaseTypeSyntax>(
                                            SimpleBaseType(
                                                QualifiedName(
                                                    IdentifierName("System"),
                                                    IdentifierName("Attribute"))))))
                                .WithMembers(
                                    SingletonList<MemberDeclarationSyntax>(
                                        FieldDeclaration(
                                                VariableDeclaration(
                                                        NullableType(
                                                            PredefinedType(
                                                                Token(SyntaxKind.StringKeyword))))
                                                    .WithVariables(
                                                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                                                            VariableDeclarator(
                                                                Identifier("HubName")))))
                                            .WithModifiers(
                                                TokenList(
                                                    Token(SyntaxKind.PublicKeyword)))))))))
        .NormalizeWhitespace()
        .GetText(Encoding.UTF8);
}