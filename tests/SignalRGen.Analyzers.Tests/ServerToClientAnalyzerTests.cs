namespace SignalRGen.Analyzers.Tests;

public class ServerToClientAnalyzerTests
{
    [Fact]
    public async Task Interface_NotUsedAsServerToClient_ShouldNotTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IRegularInterface
                                    {
                                        Task<string> DoSomethingAsync();
                                        Task<int> GetValueAsync();
                                    }
                                }
                                """;

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_WithTaskReturnType_ShouldNotTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message);
                                        Task ReceiveNotification(int id);
                                        Task UpdateCount(int count);
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

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_WithGenericTaskReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task{|#0:<string>|} GetMessage();
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

        var expectedDiagnostic = new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetMessage", "IServerToClient", "System.Threading.Tasks.Task<string>");

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_WithMultipleGenericTaskMethods_ShouldTriggerMultipleDiagnostics()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task ReceiveMessage(string message); // This is fine
                                        Task{|#0:<string>|} GetMessage();
                                        Task{|#1:<int>|} GetCount();
                                        Task UpdateData(int value); // This is fine
                                        Task{|#2:<bool>|} IsValid();
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

        var expectedDiagnostics = new[]
        {
            new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
                .WithLocation(0)
                .WithArguments("GetMessage", "IServerToClient", "System.Threading.Tasks.Task<string>"),
            new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
                .WithLocation(1)
                .WithArguments("GetCount", "IServerToClient", "System.Threading.Tasks.Task<int>"),
            new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
                .WithLocation(2)
                .WithArguments("IsValid", "IServerToClient", "System.Threading.Tasks.Task<bool>")
        };

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_UsingIServerToClientHub_WithGenericTask_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task{|#0:<string>|} GetMessage();
                                        Task ReceiveMessage(string message);
                                    }

                                    public interface ITestHub : IServerToClientHub<IServerToClient>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostic = new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetMessage", "IServerToClient", "System.Threading.Tasks.Task<string>");

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ClientToServerInterface_WithGenericTaskReturnType_ShouldNotTriggerDiagnostic()
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
                                        Task<string> SendMessageAsync(string message); // This should be allowed for client-to-server
                                        Task<int> GetCountAsync();
                                    }

                                    public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                                    {
                                    }
                                }
                                """;

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;

        await test.RunAsync();
    }

    [Fact]
    public async Task Interface_UsedInBothServerAndClientPosition_OnlyServerPosition_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface ISharedInterface
                                    {
                                        Task{|#0:<string>|} GetMessage(); // Should trigger diagnostic only when used as server interface
                                    }

                                    public interface ITestHub : IBidirectionalHub<ISharedInterface, ISharedInterface>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostic = new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetMessage", "ISharedInterface", "System.Threading.Tasks.Task<string>");

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_WithComplexGenericTaskReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;
                                using System.Collections.Generic;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task{|#0:<List<string>>|} GetMessages();
                                        Task{|#1:<Dictionary<int, string>>|} GetData();
                                        Task ReceiveMessage(string message);
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

        var expectedDiagnostics = new[]
        {
            new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
                .WithLocation(0)
                .WithArguments("GetMessages", "IServerToClient",
                    "System.Threading.Tasks.Task<System.Collections.Generic.List<string>>"),
            new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
                .WithLocation(1)
                .WithArguments("GetData", "IServerToClient",
                    "System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<int, string>>")
        };

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_WithGenericMethods_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task{|#0:<T>|} GetGenericValue<T>();
                                        Task ReceiveMessage(string message);
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

        var expectedDiagnostic = new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetGenericValue", "IServerToClient", "System.Threading.Tasks.Task<T>");

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Theory]
    [InlineData("string")]
    [InlineData("int")]
    [InlineData("bool")]
    [InlineData("object")]
    public async Task ServerToClientInterface_WithSpecificGenericTaskType_ShouldTriggerDiagnostic(string typeArgument)
    {
        string testCode = $$"""
                            using SignalRGen.Abstractions;
                            using System.Threading.Tasks;

                            namespace TestNamespace
                            {
                                public interface IServerToClient
                                {
                                    Task{|#0:<{{typeArgument}}>|} TestMethod();
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

        var expectedDiagnostic = new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
            .WithLocation(0)
            .WithArguments("TestMethod", "IServerToClient", $"System.Threading.Tasks.Task<{typeArgument}>");

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_WithMultipleHubsUsingInterface_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        Task{|#0:<string>|} GetMessage();
                                    }

                                    public interface IClientToServer1
                                    {
                                        Task SendMessage(string message);
                                    }

                                    public interface IClientToServer2
                                    {
                                        Task SendData(int data);
                                    }

                                    public interface ITestHub1 : IBidirectionalHub<IServerToClient, IClientToServer1>
                                    {
                                    }

                                    public interface ITestHub2 : IServerToClientHub<IServerToClient>
                                    {
                                    }
                                }
                                """;

        var expectedDiagnostic = new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetMessage", "IServerToClient", "System.Threading.Tasks.Task<string>");

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }


    [Fact]
    public async Task ServerToClientInterface_WithVoidReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        {|#0:void|} SendMessage(string message);
                                        Task ReceiveMessage(string message); // This is fine
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

        var expectedDiagnostic = new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
            .WithLocation(0)
            .WithArguments("SendMessage", "IServerToClient", "void");

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_WithStringReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        {|#0:string|} GetMessage();
                                        Task ReceiveMessage(string message); // This is fine
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

        var expectedDiagnostic = new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetMessage", "IServerToClient", "string");

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_WithIntReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        {|#0:int|} GetCount();
                                        Task ReceiveMessage(string message); // This is fine
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

        var expectedDiagnostic = new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetCount", "IServerToClient", "int");

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_WithBoolReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        {|#0:bool|} IsValid();
                                        Task ReceiveMessage(string message); // This is fine
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

        var expectedDiagnostic = new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
            .WithLocation(0)
            .WithArguments("IsValid", "IServerToClient", "bool");

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_WithCustomObjectReturnType_ShouldTriggerDiagnostic()
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
                                        {|#0:CustomObject|} GetObject();
                                        Task ReceiveMessage(string message); // This is fine
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

        var expectedDiagnostic = new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetObject", "IServerToClient", "TestNamespace.CustomObject");

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_WithMixedInvalidReturnTypes_ShouldTriggerMultipleDiagnostics()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        {|#0:string|} GetMessage();
                                        Task ReceiveMessage(string message); // This is fine
                                        {|#1:int|} GetCount();
                                        Task{|#2:<bool>|} IsValid();
                                        {|#3:void|} DoSomething();
                                        Task ProcessData(string data); // This is fine
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

        var expectedDiagnostics = new[]
        {
            new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
                .WithLocation(0)
                .WithArguments("GetMessage", "IServerToClient", "string"),
            new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
                .WithLocation(1)
                .WithArguments("GetCount", "IServerToClient", "int"),
            new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
                .WithLocation(2)
                .WithArguments("IsValid", "IServerToClient", "System.Threading.Tasks.Task<bool>"),
            new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
                .WithLocation(3)
                .WithArguments("DoSomething", "IServerToClient", "void")
        };

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_WithValueTupleReturnType_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        {|#0:(string, int)|} GetData();
                                        Task ReceiveMessage(string message); // This is fine
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

        var expectedDiagnostic = new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetData", "IServerToClient", "(string, int)");

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_WithAsyncMethodWithoutTask_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        {|#0:ValueTask|} SendMessageAsync(string message);
                                        Task ReceiveMessage(string message); // This is fine
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

        var expectedDiagnostic = new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
            .WithLocation(0)
            .WithArguments("SendMessageAsync", "IServerToClient", "System.Threading.Tasks.ValueTask");

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }

    [Fact]
    public async Task ServerToClientInterface_WithValueTaskGeneric_ShouldTriggerDiagnostic()
    {
        const string testCode = """
                                using SignalRGen.Abstractions;
                                using System.Threading.Tasks;

                                namespace TestNamespace
                                {
                                    public interface IServerToClient
                                    {
                                        {|#0:ValueTask<string>|} GetMessageAsync();
                                        Task ReceiveMessage(string message); // This is fine
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

        var expectedDiagnostic = new DiagnosticResult(ServerToClientAnalyzer.ServerToClientReturnTypeRule)
            .WithLocation(0)
            .WithArguments("GetMessageAsync", "IServerToClient", "System.Threading.Tasks.ValueTask<string>");

        var test = TestHelper.CreateAnalyzerTest<ServerToClientAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);

        await test.RunAsync();
    }
}