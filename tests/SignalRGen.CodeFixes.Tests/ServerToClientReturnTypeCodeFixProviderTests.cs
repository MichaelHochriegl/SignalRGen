using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using SignalRGen.Abstractions;
using SignalRGen.Analyzers;
using SignalRGen.Shared;

namespace SignalRGen.CodeFixes.Tests;

public class ServerToClientReturnTypeCodeFixProviderTests
{
    private class Test : CSharpCodeFixTest<ServerToClientAnalyzer, ServerToClientReturnTypeCodeFixProvider, DefaultVerifier>
    {
        public Test()
        {
            // Add necessary references
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
            TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(IBidirectionalHub<,>).Assembly.Location));
        }
    }

    [Fact]
    public async Task VoidReturnType_ConvertsToTask()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                {|#0:void|} SendMessage(string message);
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        const string fixedSource = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task SendMessage(string message);
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract)
            .WithLocation(0)
            .WithArguments("SendMessage", "IServerToClient", "void");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task VoidReturnType_ConvertsToTask_AddsUsingStatement()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                {|#0:void|} SendMessage(string message);
            }

            public interface IClientToServer
            {
                System.Threading.Tasks.Task SomeMethod();
            }
            """;

        const string fixedSource = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task SendMessage(string message);
            }

            public interface IClientToServer
            {
                System.Threading.Tasks.Task SomeMethod();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract)
            .WithLocation(0)
            .WithArguments("SendMessage", "IServerToClient", "void");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task StringReturnType_ConvertsToTask()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                {|#0:string|} SendMessage();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        const string fixedSource = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task SendMessage();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract)
            .WithLocation(0)
            .WithArguments("SendMessage", "IServerToClient", "string");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task IntReturnType_ConvertsToTask()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                {|#0:int|} GetCount();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        const string fixedSource = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task GetCount();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract)
            .WithLocation(0)
            .WithArguments("GetCount", "IServerToClient", "int");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task TaskOfTReturnType_ConvertsToTask()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task{|#0:<string>|} SendMessage();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        const string fixedSource = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task SendMessage();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract)
            .WithLocation(0)
            .WithArguments("SendMessage", "IServerToClient", "System.Threading.Tasks.Task<string>");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task TaskOfGenericReturnType_ConvertsToTask()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Collections.Generic;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task{|#0:<List<string>>|} GetItems();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        const string fixedSource = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Collections.Generic;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task GetItems();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract)
            .WithLocation(0)
            .WithArguments("GetItems", "IServerToClient", "System.Threading.Tasks.Task<System.Collections.Generic.List<string>>");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task ComplexGenericReturnType_ConvertsToTask()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Collections.Generic;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                {|#0:Dictionary<string, List<int>>|} GetComplexData();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        const string fixedSource = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Collections.Generic;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task GetComplexData();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract)
            .WithLocation(0)
            .WithArguments("GetComplexData", "IServerToClient", "System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task CustomTypeReturnType_ConvertsToTask()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                {|#0:MyCustomType|} SendCustomData();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }

            public record MyCustomType(string Name, int Id);
            """;

        const string fixedSource = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task SendCustomData();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }

            public record MyCustomType(string Name, int Id);
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract)
            .WithLocation(0)
            .WithArguments("SendCustomData", "IServerToClient", "MyCustomType");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task MultipleMethodsWithDifferentReturnTypes()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                {|#0:void|} SendMessage(string message);
                {|#1:string|} GetMessage();
                Task{|#2:<int>|} GetCount();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        const string fixedSource = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task SendMessage(string message);
                Task GetMessage();
                Task GetCount();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        var expected1 = DiagnosticResult.CompilerError(DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract)
            .WithLocation(0)
            .WithArguments("SendMessage", "IServerToClient", "void");
            
        var expected2 = DiagnosticResult.CompilerError(DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract)
            .WithLocation(1)
            .WithArguments("GetMessage", "IServerToClient", "string");
            
        var expected3 = DiagnosticResult.CompilerError(DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract)
            .WithLocation(2)
            .WithArguments("GetCount", "IServerToClient", "System.Threading.Tasks.Task<int>");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected1, expected2, expected3 }
        }.RunAsync();
    }

    [Fact]
    public async Task NoFixForAlreadyCorrectTaskReturnType()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task SendMessage(string message);
                Task NotifyUpdate();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        await new Test
        {
            TestCode = source,
            FixedCode = source, // No changes expected
            ExpectedDiagnostics = { } // No diagnostics expected
        }.RunAsync();
    }

    [Fact]
    public async Task HandlesNullableReturnTypes()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                {|#0:string?|} SendMessage();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        const string fixedSource = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task SendMessage();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract)
            .WithLocation(0)
            .WithArguments("SendMessage", "IServerToClient", "string?");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task WorksWithServerToClientOnlyHub()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IServerToClientHub<IServerToClient>
            {
            }

            public interface IServerToClient
            {
                {|#0:string|} SendMessage();
            }
            """;

        const string fixedSource = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IServerToClientHub<IServerToClient>
            {
            }

            public interface IServerToClient
            {
                Task SendMessage();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract)
            .WithLocation(0)
            .WithArguments("SendMessage", "IServerToClient", "string");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task ComplexGenericReturnType_ConvertsToTask_AddsUsingStatement()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Collections.Generic;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                {|#0:Dictionary<string, List<int>>|} SendComplexData();
            }

            public interface IClientToServer
            {
                System.Threading.Tasks.Task SomeMethod();
            }
            """;

        const string fixedSource = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Collections.Generic;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task SendComplexData();
            }

            public interface IClientToServer
            {
                System.Threading.Tasks.Task SomeMethod();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract)
            .WithLocation(0)
            .WithArguments("SendComplexData", "IServerToClient", "System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task TaskOfComplexGenericReturnType_ConvertsToTask()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Collections.Generic;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task{|#0:<Dictionary<string, List<int>>>|} SendComplexData();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        const string fixedSource = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Collections.Generic;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                Task SendComplexData();
            }

            public interface IClientToServer
            {
                Task SomeMethod();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract)
            .WithLocation(0)
            .WithArguments("SendComplexData", "IServerToClient", "System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>>");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }
}