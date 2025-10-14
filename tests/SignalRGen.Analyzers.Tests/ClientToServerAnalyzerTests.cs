namespace SignalRGen.Analyzers.Tests;

public class ClientToServerAnalyzerTests
{
    [Fact]
    public async Task Interface_NotUsedAsClientToServer_ShouldNotTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IRegularInterface
                                    {
                                        Task<string> DoSomethingAsync();
                                        string GetValue();
                                    }
                                }
                                """;

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithValidTaskReturnTypes_ShouldNotTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IClientToServer
                                    {
                                        Task SendMessage(string message);
                                        Task<string> GetResponseAsync();
                                        Task<int> GetCountAsync();
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithVoidReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IClientToServer
                                    {
                                        {|#0:void|} SendMessage(string message);
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostic = new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
            .WithLocation(0)
            .WithArguments("SendMessage", "IClientToServer", "void");

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithStringReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IClientToServer
                                    {
                                        {|#0:string|} GetMessage();
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostic = new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetMessage", "IClientToServer", "string");

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithIntReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IClientToServer
                                    {
                                        {|#0:int|} GetCount();
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostic = new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetCount", "IClientToServer", "int");

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithBoolReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IClientToServer
                                    {
                                        {|#0:bool|} IsValid();
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostic = new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
            .WithLocation(0)
            .WithArguments("IsValid", "IClientToServer", "bool");

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithCustomObjectReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public class CustomObject
                                    {
                                        public string Name { get; set; }
                                    }

                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IClientToServer
                                    {
                                        {|#0:CustomObject|} GetObject();
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostic = new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetObject", "IClientToServer", "TestNamespace.CustomObject");

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithMultipleInvalidReturnTypes_ShouldTriggerMultipleDiagnostics()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IClientToServer
                                    {
                                        {|#0:string|} GetMessage();
                                        Task SendMessage(string message); // This is fine
                                        {|#1:int|} GetCount();
                                        Task<bool> IsValidAsync(); // This is fine
                                        {|#2:void|} DoSomething();
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostics = new[]
        {
            new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
                .WithLocation(0)
                .WithArguments("GetMessage", "IClientToServer", "string"),
            new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
                .WithLocation(1)
                .WithArguments("GetCount", "IClientToServer", "int"),
            new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
                .WithLocation(2)
                .WithArguments("DoSomething", "IClientToServer", "void")
        };

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_UsingIClientToServerHub_WithInvalidReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IClientToServer
                                    {
                                        {|#0:string|} GetMessage();
                                        Task SendMessage(string message);
                                    }

                                    public interface ITestHub : IClientToServerHub<IClientToServer>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostic = new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetMessage", "IClientToServer", "string");

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_WithInvalidReturnType_ShouldNotTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        string GetMessage(); // This would be invalid for server-to-client but analyzer should not trigger
                                    }

                                    public interface IClientToServer
                                    {
                                        Task SendMessage(string message);
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;

        await test.RunAsync();
    }

    [Fact]
    public async Task Interface_UsedInBothServerAndClientPosition_OnlyClientPosition_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface ISharedInterface
                                    {
                                        {|#0:string|} GetMessage(); // Should trigger diagnostic only when used as client interface
                                    }

                                    public interface ITestHub : IBidirectionalHub<ISharedInterface, ISharedInterface>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostic = new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetMessage", "ISharedInterface", "string");

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithValueTupleReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IClientToServer
                                    {
                                        {|#0:(string, int)|} GetData();
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostic = new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetData", "IClientToServer", "(string, int)");

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithValueTaskReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IClientToServer
                                    {
                                        {|#0:ValueTask|} SendMessageAsync(string message);
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostic = new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
            .WithLocation(0)
            .WithArguments("SendMessageAsync", "IClientToServer", "System.Threading.Tasks.ValueTask");

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithValueTaskGeneric_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IClientToServer
                                    {
                                        {|#0:ValueTask<string>|} GetMessageAsync();
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostic = new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetMessageAsync", "IClientToServer", "System.Threading.Tasks.ValueTask<string>");

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithComplexGenericTaskReturnType_ShouldNotTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;
                                using System.Collections.Generic;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IClientToServer
                                    {
                                        Task<List<string>> GetMessages();
                                        Task<Dictionary<int, string>> GetData();
                                        Task SendMessage(string message);
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithGenericMethods_ShouldNotTriggerDiagnosticForValidReturnTypes()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IClientToServer
                                    {
                                        Task<T> GetGenericValue<T>();
                                        Task SendGenericMessage<T>(T message);
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;

        await test.RunAsync();
    }

    [Theory]
    [InlineData("string")]
    [InlineData("int")]
    [InlineData("bool")]
    [InlineData("object")]
    public async Task ClientToServerInterface_WithSpecificInvalidReturnType_ShouldTriggerDiagnostic(string returnType)
    {
        string testCode = $$"""
                            using SignalRGen.Abstractions;
                            using System.Threading.Tasks;

                            namespace TestNamespace
                            {
                                public interface IServerToClient
                                {
                                    Task ReceiveMessage(string message);
                                }

                                public interface IClientToServer
                                {
                                    {|#0:{{returnType}}|} TestMethod();
                                }

                                public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                {
                                }
                            }
                            """;

        var expectedDiagnostic = new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
            .WithLocation(0)
            .WithArguments("TestMethod", "IClientToServer", returnType);

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithMultipleHubsUsingInterface_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IClientToServer
                                    {
                                        {|#0:string|} GetMessage();
                                    }

                                    public interface ITestHub1 : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }

                                    public interface ITestHub2 : IClientToServerHub<IClientToServer>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostic = new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetMessage", "IClientToServer", "string");

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }
    
    
    [Fact]
    public async Task ClientToServerInterface_WithInheritedInvalidReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IBaseClientToServer<T>
                                    {
                                        Task<T> GetDataAsync(); // Valid
                                        {|#0:T|} GetDataSync(); // Invalid
                                    }

                                    public interface IClientToServer : IBaseClientToServer<string>
                                    {
                                        Task SendMessage(string message); // Valid
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostic = new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetDataSync", "IClientToServer", "T");

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithMultipleLevelsOfInheritance_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IBaseBase
                                    {
                                        {|#0:string|} GetBaseValue(); // Invalid
                                    }

                                    public interface IBase : IBaseBase
                                    {
                                        Task<string> GetValidValue(); // Valid
                                        {|#1:int|} GetIntValue(); // Invalid
                                    }

                                    public interface IClientToServer : IBase
                                    {
                                        Task SendMessage(string message); // Valid
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostics = new[]
        {
            new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
                .WithLocation(0)
                .WithArguments("GetBaseValue", "IClientToServer", "string"),
            new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
                .WithLocation(1)
                .WithArguments("GetIntValue", "IClientToServer", "int")
        };

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithMultipleBaseInterfacesWithInvalidMethods_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IBase1
                                    {
                                        {|#0:string|} GetStringValue(); // Invalid
                                    }

                                    public interface IBase2
                                    {
                                        {|#1:int|} GetIntValue(); // Invalid
                                    }

                                    public interface IClientToServer : IBase1, IBase2
                                    {
                                        Task SendMessage(string message); // Valid
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostics = new[]
        {
            new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
                .WithLocation(0)
                .WithArguments("GetStringValue", "IClientToServer", "string"),
            new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
                .WithLocation(1)
                .WithArguments("GetIntValue", "IClientToServer", "int")
        };

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_EmptyInheritedInterface_WithBaseInvalidMethods_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IBaseClientToServer<T>
                                    {
                                        Task<T> GetDataAsync(); // Valid
                                        {|#0:T|} GetDataSync(); // Invalid
                                        {|#1:void|} ProcessData(T data); // Invalid
                                    }

                                    // Empty interface that only inherits
                                    public interface IClientToServer : IBaseClientToServer<string>
                                    {
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostics = new[]
        {
            new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
                .WithLocation(0)
                .WithArguments("GetDataSync", "IClientToServer", "T"),
            new DiagnosticResult(ClientToServerAnalyzer.ClientToServerReturnTypeRule)
                .WithLocation(1)
                .WithArguments("ProcessData", "IClientToServer", "void")
        };

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithValidInheritedMethods_ShouldNotTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface IBaseClientToServer<T>
                                    {
                                        Task<T> GetDataAsync();
                                        Task ProcessDataAsync(T data);
                                    }

                                    public interface IClientToServer : IBaseClientToServer<string>
                                    {
                                        Task SendMessage(string message);
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var test = TestHelper.CreateAnalyzerTest<ClientToServerAnalyzer>();
        test.TestCode = testCode;

        await test.RunAsync();
    }
}