using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using SignalRGen.Abstractions;
using SignalRGen.Analyzers;
using SignalRGen.Shared;

namespace SignalRGen.CodeFixes.Tests;

public class ClientToServerReturnTypeCodeFixProviderTests
{
    private class Test : CSharpCodeFixTest<ClientToServerAnalyzer, ClientToServerReturnTypeCodeFixProvider, DefaultVerifier>
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                {|#0:void|} SendMessage(string message);
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                Task SendMessage(string message);
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract)
            .WithLocation(0)
            .WithArguments("SendMessage", "IClientToServer", "void");

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
                System.Threading.Tasks.Task SomeMethod();
            }

            public interface IClientToServer
            {
                {|#0:void|} SendMessage(string message);
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
                System.Threading.Tasks.Task SomeMethod();
            }

            public interface IClientToServer
            {
                Task SendMessage(string message);
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract)
            .WithLocation(0)
            .WithArguments("SendMessage", "IClientToServer", "void");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task StringReturnType_ConvertsToTaskOfString()
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                {|#0:string|} GetMessage();
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                Task<string> GetMessage();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract)
            .WithLocation(0)
            .WithArguments("GetMessage", "IClientToServer", "string");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task StringReturnType_ConvertsToTaskOfString_AddsUsingStatement()
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
                System.Threading.Tasks.Task SomeMethod();
            }

            public interface IClientToServer
            {
                {|#0:string|} GetMessage();
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
                System.Threading.Tasks.Task SomeMethod();
            }

            public interface IClientToServer
            {
                Task<string> GetMessage();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract)
            .WithLocation(0)
            .WithArguments("GetMessage", "IClientToServer", "string");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task IntReturnType_ConvertsToTaskOfInt()
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                {|#0:int|} GetCount();
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                Task<int> GetCount();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract)
            .WithLocation(0)
            .WithArguments("GetCount", "IClientToServer", "int");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task GenericReturnType_ConvertsToTaskOfGeneric()
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                {|#0:List<string>|} GetItems();
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                Task<List<string>> GetItems();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract)
            .WithLocation(0)
            .WithArguments("GetItems", "IClientToServer", "System.Collections.Generic.List<string>");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task CustomTypeReturnType_ConvertsToTaskOfCustomType()
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                {|#0:MyCustomType|} GetCustomData();
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                Task<MyCustomType> GetCustomData();
            }

            public record MyCustomType(string Name, int Id);
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract)
            .WithLocation(0)
            .WithArguments("GetCustomData", "IClientToServer", "MyCustomType");

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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                {|#0:void|} SendMessage(string message);
                {|#1:string|} GetMessage();
                {|#2:int|} GetCount();
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                Task SendMessage(string message);
                Task<string> GetMessage();
                Task<int> GetCount();
            }
            """;

        var expected1 = DiagnosticResult.CompilerError(DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract)
            .WithLocation(0)
            .WithArguments("SendMessage", "IClientToServer", "void");
            
        var expected2 = DiagnosticResult.CompilerError(DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract)
            .WithLocation(1)
            .WithArguments("GetMessage", "IClientToServer", "string");
            
        var expected3 = DiagnosticResult.CompilerError(DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract)
            .WithLocation(2)
            .WithArguments("GetCount", "IClientToServer", "int");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected1, expected2, expected3 },
            NumberOfFixAllIterations = 2 // Need multiple iterations to fix all issues
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                Task SendMessage(string message);
                Task<string> GetMessage();
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                {|#0:string?|} GetMessage();
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                Task<string?> GetMessage();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract)
            .WithLocation(0)
            .WithArguments("GetMessage", "IClientToServer", "string?");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task WorksWithClientToServerOnlyHub()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IClientToServerHub<IClientToServer>
            {
            }

            public interface IClientToServer
            {
                {|#0:string|} GetMessage();
            }
            """;

        const string fixedSource = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IClientToServerHub<IClientToServer>
            {
            }

            public interface IClientToServer
            {
                Task<string> GetMessage();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract)
            .WithLocation(0)
            .WithArguments("GetMessage", "IClientToServer", "string");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task HandlesComplexGenericTypes()
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                {|#0:Dictionary<string, List<int>>|} GetComplexData();
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
                Task SomeMethod();
            }

            public interface IClientToServer
            {
                Task<Dictionary<string, List<int>>> GetComplexData();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract)
            .WithLocation(0)
            .WithArguments("GetComplexData", "IClientToServer", "System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task HandlesComplexGenericTypes_AddsUsingStatement()
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
                System.Threading.Tasks.Task SomeMethod();
            }

            public interface IClientToServer
            {
                {|#0:Dictionary<string, List<int>>|} GetComplexData();
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
                System.Threading.Tasks.Task SomeMethod();
            }

            public interface IClientToServer
            {
                Task<Dictionary<string, List<int>>> GetComplexData();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract)
            .WithLocation(0)
            .WithArguments("GetComplexData", "IClientToServer", "System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }
}