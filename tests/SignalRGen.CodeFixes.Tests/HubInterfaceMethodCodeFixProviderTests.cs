using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using SignalRGen.Abstractions;
using SignalRGen.Analyzers;
using SignalRGen.Shared;

namespace SignalRGen.CodeFixes.Tests;

public class HubInterfaceMethodCodeFixProviderTests
{
    private class Test : CSharpCodeFixTest<HubContractAnalyzer, HubInterfaceMethodCodeFixProvider, DefaultVerifier>
    {
        public Test()
        {
            // Add necessary references
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
            TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(IBidirectionalHub<,>).Assembly.Location));
        }
    }

    [Fact]
    public async Task BidirectionalHub_MovesMethodToServerInterface()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
                {|#0:Task SendMessage(string message);|}
            }

            public interface IServerToClient
            {
            }

            public interface IClientToServer
            {
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
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0001NoMethodsInHubContractAllowed)
            .WithLocation(0)
            .WithArguments("SendMessage", "ITestHub");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected },
            CodeActionIndex = 0 // Move to server interface
        }.RunAsync();
    }

    [Fact]
    public async Task BidirectionalHub_MovesMethodToClientInterface()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
                {|#0:Task<string> GetMessage();|}
            }

            public interface IServerToClient
            {
            }

            public interface IClientToServer
            {
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
            }

            public interface IClientToServer
            {
                Task<string> GetMessage();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0001NoMethodsInHubContractAllowed)
            .WithLocation(0)
            .WithArguments("GetMessage", "ITestHub");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected },
            CodeActionIndex = 1 // Move to client interface
        }.RunAsync();
    }

    [Fact]
    public async Task ServerToClientOnlyHub_MovesMethodToServerInterface()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IServerToClientHub<IServerToClient>
            {
                {|#0:Task NotifyUpdate(int count);|}
            }

            public interface IServerToClient
            {
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
                Task NotifyUpdate(int count);
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0001NoMethodsInHubContractAllowed)
            .WithLocation(0)
            .WithArguments("NotifyUpdate", "ITestHub");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task ClientToServerOnlyHub_MovesMethodToClientInterface()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IClientToServerHub<IClientToServer>
            {
                {|#0:Task<int> GetCount();|}
            }

            public interface IClientToServer
            {
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
                Task<int> GetCount();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0001NoMethodsInHubContractAllowed)
            .WithLocation(0)
            .WithArguments("GetCount", "ITestHub");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected }
        }.RunAsync();
    }

    [Fact]
    public async Task MovesMethodWithParameters()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
                {|#0:Task SendComplexMessage(string message, int priority, bool urgent);|}
            }

            public interface IServerToClient
            {
            }

            public interface IClientToServer
            {
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
                Task SendComplexMessage(string message, int priority, bool urgent);
            }

            public interface IClientToServer
            {
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0001NoMethodsInHubContractAllowed)
            .WithLocation(0)
            .WithArguments("SendComplexMessage", "ITestHub");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected },
            CodeActionIndex = 0 // Move to server interface
        }.RunAsync();
    }

    [Fact]
    public async Task MovesMethodWithGenericReturnType()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Collections.Generic;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
                {|#0:Task<List<string>> GetMessages();|}
            }

            public interface IServerToClient
            {
            }

            public interface IClientToServer
            {
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
            }

            public interface IClientToServer
            {
                Task<List<string>> GetMessages();
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0001NoMethodsInHubContractAllowed)
            .WithLocation(0)
            .WithArguments("GetMessages", "ITestHub");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected },
            CodeActionIndex = 1 // Move to client interface
        }.RunAsync();
    }

    [Fact]
    public async Task MovesToExistingInterfaceWithMethods()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
                {|#0:Task SendNewMessage(string message);|}
            }

            public interface IServerToClient
            {
                Task SendExistingMessage(string message);
            }

            public interface IClientToServer
            {
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
                Task SendExistingMessage(string message);
                Task SendNewMessage(string message);
            }

            public interface IClientToServer
            {
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0001NoMethodsInHubContractAllowed)
            .WithLocation(0)
            .WithArguments("SendNewMessage", "ITestHub");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected },
            CodeActionIndex = 0 // Move to server interface
        }.RunAsync();
    }

    [Fact(Skip="XML docs are currently not moved properly and instead vanish. See https://github.com/MichaelHochriegl/SignalRGen/issues/81")]
    public async Task HandlesMethodWithDocumentationComments()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
                /// <summary>
                /// Sends a message to all connected clients.
                /// </summary>
                /// <param name="message">The message to send.</param>
                {|#0:Task SendMessage(string message);|}
            }

            public interface IServerToClient
            {
            }

            public interface IClientToServer
            {
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
                /// <summary>
                /// Sends a message to all connected clients.
                /// </summary>
                /// <param name="message">The message to send.</param>
                Task SendMessage(string message);
            }

            public interface IClientToServer
            {
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0001NoMethodsInHubContractAllowed)
            .WithLocation(0)
            .WithArguments("SendMessage", "ITestHub");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected },
            CodeActionIndex = 0 // Move to server interface
        }.RunAsync();
    }

    [Fact]
    public async Task HandlesMethodWithAttributes()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.ComponentModel;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
                {|#0:[Description("A test method")]
                Task SendMessage(string message);|}
            }

            public interface IServerToClient
            {
            }

            public interface IClientToServer
            {
            }
            """;

        const string fixedSource = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.ComponentModel;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
            }

            public interface IServerToClient
            {
                [Description("A test method")]
                Task SendMessage(string message);
            }

            public interface IClientToServer
            {
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0001NoMethodsInHubContractAllowed)
            .WithLocation(0)
            .WithArguments("SendMessage", "ITestHub");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected },
            CodeActionIndex = 0 // Move to server interface
        }.RunAsync();
    }

    [Fact]
    public async Task HandlesCustomParameterTypes()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
                {|#0:Task SendData(CustomData data);|}
            }

            public interface IServerToClient
            {
            }

            public interface IClientToServer
            {
            }

            public record CustomData(string Name, int Value);
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
                Task SendData(CustomData data);
            }

            public interface IClientToServer
            {
            }

            public record CustomData(string Name, int Value);
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0001NoMethodsInHubContractAllowed)
            .WithLocation(0)
            .WithArguments("SendData", "ITestHub");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected },
            CodeActionIndex = 0 // Move to server interface
        }.RunAsync();
    }

    [Fact]
    public async Task NoFixForHubWithoutMethods()
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
            }

            public interface IClientToServer
            {
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
    public async Task HandlesNullableParameters()
    {
        const string source = """
            using SignalRGen.Abstractions;
            using SignalRGen.Abstractions.Attributes;
            using System.Threading.Tasks;

            [HubClient(HubUri = "test")]
            public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
            {
                {|#0:Task<string?> GetOptionalMessage(string? filter);|}
            }

            public interface IServerToClient
            {
            }

            public interface IClientToServer
            {
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
            }

            public interface IClientToServer
            {
                Task<string?> GetOptionalMessage(string? filter);
            }
            """;

        var expected = DiagnosticResult.CompilerError(DiagnosticIds.SRG0001NoMethodsInHubContractAllowed)
            .WithLocation(0)
            .WithArguments("GetOptionalMessage", "ITestHub");

        await new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics = { expected },
            CodeActionIndex = 1 // Move to client interface
        }.RunAsync();
    }
}