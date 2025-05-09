using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace SignalRGen.Generator.Sources;

internal static class ServerToClientMethodAttributeSource
{
    internal static SourceText GetSource()
    {
        var template =
            $$"""
            {{AutoGeneratedHintSource.AutoGeneratedHintTemplate}}
            
            using System;

            #nullable enable

            namespace SignalRGen.Generator;

            [AttributeUsage(AttributeTargets.Method)]
            public sealed class ServerToClientMethodAttribute : Attribute
            {
            }
            """;
        
        return SourceText.From(template, Encoding.UTF8);
    }
}