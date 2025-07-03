
namespace SignalRGen.Analyzers.Tests;

public class HubContractAnalyzerTests
{
    [Fact]
    public async Task Interface_WithoutHubInheritance_ShouldNotTriggerDiagnostic()
    {
        const string testCode = """
            using SignalRGen.Abstractions;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public interface IRegularInterface
                {
                    Task DoSomething();
                    string GetValue();
                }
            }
            """;

        var test = TestHelper.CreateAnalyzerTest<HubContractAnalyzer>();
        test.TestCode = testCode;
        
        await test.RunAsync();
    }

    [Fact]
    public async Task Interface_InheritingFromIBidirectionalHub_WithMethod_ShouldTriggerDiagnostic()
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
                }

                public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                {
                    Task {|#0:InvalidMethod|}();
                }
            }
            """;

        var expectedDiagnostic = new DiagnosticResult(HubContractAnalyzer.MethodInHubInterfaceRule)
            .WithLocation(0)
            .WithArguments("InvalidMethod", "ITestHub");

        var test = TestHelper.CreateAnalyzerTest<HubContractAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);
        
        await test.RunAsync();
    }

    [Fact]
    public async Task Interface_InheritingFromIServerToClientHub_WithMethod_ShouldTriggerDiagnostic()
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

                public interface ITestHub : IServerToClientHub<IServerToClient>
                {
                    Task {|#0:SendMessage|}(string message);
                }
            }
            """;

        var expectedDiagnostic = new DiagnosticResult(HubContractAnalyzer.MethodInHubInterfaceRule)
            .WithLocation(0)
            .WithArguments("SendMessage", "ITestHub");

        var test = TestHelper.CreateAnalyzerTest<HubContractAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);
        
        await test.RunAsync();
    }

    [Fact]
    public async Task Interface_InheritingFromIClientToServerHub_WithMethod_ShouldTriggerDiagnostic()
    {
        const string testCode = """
            using SignalRGen.Abstractions;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public interface IClientToServer
                {
                    Task ProcessData(int data);
                }

                public interface ITestHub : IClientToServerHub<IClientToServer>
                {
                    Task {|#0:ProcessData|}(int data);
                }
            }
            """;

        var expectedDiagnostic = new DiagnosticResult(HubContractAnalyzer.MethodInHubInterfaceRule)
            .WithLocation(0)
            .WithArguments("ProcessData", "ITestHub");

        var test = TestHelper.CreateAnalyzerTest<HubContractAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);
        
        await test.RunAsync();
    }

    [Fact]
    public async Task Interface_InheritingFromHubInterface_WithMultipleMethods_ShouldTriggerMultipleDiagnostics()
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
                }

                public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                {
                    Task {|#0:FirstMethod|}();
                    Task {|#1:SecondMethod|}(string param);
                    string {|#2:ThirdMethod|}(int value);
                }
            }
            """;

        var expectedDiagnostics = new[]
        {
            new DiagnosticResult(HubContractAnalyzer.MethodInHubInterfaceRule)
                .WithLocation(0)
                .WithArguments("FirstMethod", "ITestHub"),
            new DiagnosticResult(HubContractAnalyzer.MethodInHubInterfaceRule)
                .WithLocation(1)
                .WithArguments("SecondMethod", "ITestHub"),
            new DiagnosticResult(HubContractAnalyzer.MethodInHubInterfaceRule)
                .WithLocation(2)
                .WithArguments("ThirdMethod", "ITestHub")
        };

        var test = TestHelper.CreateAnalyzerTest<HubContractAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
        
        await test.RunAsync();
    }

    [Fact]
    public async Task Interface_InheritingFromHubInterface_WithEvents_ShouldTriggerDiagnostic()
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
                }

                public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                {
                    event System.Action {|#0:TestEvent|};
                }
            }
            """;

        // Events generate three members: the event itself, add_EventName, and remove_EventName
        var expectedDiagnostics = new[]
        {
            new DiagnosticResult(HubContractAnalyzer.MethodInHubInterfaceRule)
                .WithLocation(0)
                .WithArguments("TestEvent", "ITestHub"),
            new DiagnosticResult(HubContractAnalyzer.MethodInHubInterfaceRule)
                .WithLocation(0)
                .WithArguments("add_TestEvent", "ITestHub"),
            new DiagnosticResult(HubContractAnalyzer.MethodInHubInterfaceRule)
                .WithLocation(0)
                .WithArguments("remove_TestEvent", "ITestHub")
        };

        var test = TestHelper.CreateAnalyzerTest<HubContractAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
        
        await test.RunAsync();
    }

    [Fact]
    public async Task Interface_InheritingFromHubInterface_WithNoMembers_ShouldNotTriggerDiagnostic()
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
                }

                public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                {
                }
            }
            """;

        var test = TestHelper.CreateAnalyzerTest<HubContractAnalyzer>();
        test.TestCode = testCode;
        
        await test.RunAsync();
    }

    [Fact]
    public async Task Interface_InheritingIndirectlyFromHubInterface_WithMethod_ShouldTriggerDiagnostic()
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
                }

                public interface IBaseHub : IBidirectionalHub<IServerToClient, IClientToServer>
                {
                }

                public interface ITestHub : IBaseHub
                {
                    Task {|#0:IndirectMethod|}();
                }
            }
            """;

        var expectedDiagnostic = new DiagnosticResult(HubContractAnalyzer.MethodInHubInterfaceRule)
            .WithLocation(0)
            .WithArguments("IndirectMethod", "ITestHub");

        var test = TestHelper.CreateAnalyzerTest<HubContractAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);
        
        await test.RunAsync();
    }

    [Fact]
    public async Task Interface_InheritingFromMultipleHubInterfaces_WithMethod_ShouldTriggerDiagnostic()
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
                }

                public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>, IServerToClientHub<IServerToClient>
                {
                    Task {|#0:MultipleInheritanceMethod|}();
                }
            }
            """;

        var expectedDiagnostic = new DiagnosticResult(HubContractAnalyzer.MethodInHubInterfaceRule)
            .WithLocation(0)
            .WithArguments("MultipleInheritanceMethod", "ITestHub");

        var test = TestHelper.CreateAnalyzerTest<HubContractAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);
        
        await test.RunAsync();
    }

    [Fact]
    public async Task Interface_WithGenericMethod_ShouldTriggerDiagnostic()
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
                }

                public interface ITestHub : IBidirectionalHub<IServerToClient, IClientToServer>
                {
                    Task<T> {|#0:GenericMethod|}<T>(T value);
                }
            }
            """;

        var expectedDiagnostic = new DiagnosticResult(HubContractAnalyzer.MethodInHubInterfaceRule)
            .WithLocation(0)
            .WithArguments("GenericMethod", "ITestHub");

        var test = TestHelper.CreateAnalyzerTest<HubContractAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);
        
        await test.RunAsync();
    }

    [Theory]
    [InlineData("IBidirectionalHub<IServerToClient, IClientToServer>")]
    [InlineData("IServerToClientHub<IServerToClient>")]
    [InlineData("IClientToServerHub<IClientToServer>")]
    public async Task Interface_InheritingFromSpecificHubInterface_WithMethod_ShouldTriggerDiagnostic(string hubInterfaceName)
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
                    Task SendMessage(string message);
                }

                public interface ITestHub : {{hubInterfaceName}}
                {
                    Task {|#0:TestMethod|}();
                }
            }
            """;

        var expectedDiagnostic = new DiagnosticResult(HubContractAnalyzer.MethodInHubInterfaceRule)
            .WithLocation(0)
            .WithArguments("TestMethod", "ITestHub");

        var test = TestHelper.CreateAnalyzerTest<HubContractAnalyzer>();
        test.TestCode = testCode;
        test.ExpectedDiagnostics.Add(expectedDiagnostic);
        
        await test.RunAsync();
    }
}