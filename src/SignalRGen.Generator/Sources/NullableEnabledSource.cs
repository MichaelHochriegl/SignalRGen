using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SignalRGen.Generator.Sources;

internal static class NullableEnabledSource
{
    internal static SyntaxToken GetSyntaxToken(SyntaxKind syntaxKind) =>
        Token(
            TriviaList(
                NullableEnabledTrivia()),
            syntaxKind,
            TriviaList());

    internal static SyntaxTrivia NullableEnabledTrivia()
    {
        return Trivia(
            NullableDirectiveTrivia(
                Token(SyntaxKind.EnableKeyword),
                true));
    }
}