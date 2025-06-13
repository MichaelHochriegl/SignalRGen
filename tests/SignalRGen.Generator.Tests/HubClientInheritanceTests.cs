namespace SignalRGen.Generator.Tests;

public class HubClientInheritanceTests
{
    [Fact]
    public Task Generates_HubClient_WithOnlyBaseInterfaceMethods_Correctly()
    {
        // A simple case with a client interface that inherits from another interface but doesn't add its own methods
        const string source = """
            using System.Threading.Tasks;
            using SignalRGen.Generator;
            
            namespace SignalRGen.Example.Contracts;
            
            public interface IBaseInterface
            {
                Task ReceiveBaseMessage(string message);
                
                [ClientToServerMethod]
                Task<int> SendBaseMessage(string message);
            }
            
            [HubClient(HubUri = "example")]
            public interface IExampleHubClient : IBaseInterface
            {
            }
            """;
        
        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task Generates_HubClient_WithMixedInterfaceMethods_Correctly()
    {
        // A case with methods from both the client interface and its base
        const string source = """
            using System.Threading.Tasks;
            using SignalRGen.Generator;
            
            namespace SignalRGen.Example.Contracts;
            
            public interface IBaseInterface
            {
                Task ReceiveBaseMessage(string message);
                
                [ClientToServerMethod]
                Task<int> SendBaseMessage(string message);
            }
            
            [HubClient(HubUri = "example")]
            public interface IExampleHubClient : IBaseInterface
            {
                Task ReceiveExampleCountUpdate(int count);
                
                [ClientToServerMethod]
                Task<string> SendExampleMessage(string myClientMessage);
            }
            """;
        
        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task Generates_HubClient_WithMultiLevelInheritance_Correctly()
    {
        // Tests inheritance across multiple levels (IExampleHubClient -> IMidInterface -> IBaseInterface)
        const string source = """
            using System.Threading.Tasks;
            using SignalRGen.Generator;
            
            namespace SignalRGen.Example.Contracts;
            
            public interface IBaseInterface
            {
                Task ReceiveBaseMessage(string message);
            }
            
            public interface IMidInterface : IBaseInterface
            {
                [ClientToServerMethod]
                Task<int> SendMidMessage(int value);
            }
            
            [HubClient(HubUri = "example")]
            public interface IExampleHubClient : IMidInterface
            {
                Task ReceiveExampleCountUpdate(int count);
            }
            """;
        
        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task Generates_HubClient_WithMultipleBaseInterfaces_Correctly()
    {
        // Tests multiple interface inheritance (IExampleHubClient implements both IBaseInterface1 and IBaseInterface2)
        const string source = """
            using System.Threading.Tasks;
            using SignalRGen.Generator;
            
            namespace SignalRGen.Example.Contracts;
            
            public interface IBaseInterface1
            {
                Task ReceiveBase1Message(string message);
            }
            
            public interface IBaseInterface2
            {
                [ClientToServerMethod]
                Task<int> SendBase2Message(int value);
            }
            
            [HubClient(HubUri = "example")]
            public interface IExampleHubClient : IBaseInterface1, IBaseInterface2
            {
                Task ReceiveExampleCountUpdate(int count);
            }
            """;
        
        return TestHelper.Verify(source);
    }
    
    [Fact(Skip = "This is currently producing invalid code. See https://github.com/SignalR/SignalRGen/issues/10")]
    public Task Generates_HubClient_WithMethodsHavingSameNameInDifferentInterfaces_Correctly()
    {
        // Tests how the generator handles method name collisions between interfaces
        // Note: This scenario would typically cause compilation errors in C#, but testing edge cases is valuable
        const string source = """
            using System.Threading.Tasks;
            using SignalRGen.Generator;
            
            namespace SignalRGen.Example.Contracts;
            
            public interface IBaseInterface1
            {
                Task CommonMethod(string param1);
            }
            
            public interface IBaseInterface2
            {
                // Different signature but same name as in IBaseInterface1
                Task<int> CommonMethod(int param1);
            }
            
            [HubClient(HubUri = "example")]
            public interface IExampleHubClient : IBaseInterface1, IBaseInterface2
            {
                // This interface must explicitly implement the conflicting methods
                Task IBaseInterface1.CommonMethod(string param1) => Task.CompletedTask;
                Task<int> IBaseInterface2.CommonMethod(int param1) => Task.FromResult(param1);
                
                // Its own method
                Task ReceiveExampleCountUpdate(int count);
            }
            """;
        
        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task Generates_HubClient_WithAttributeOverrides_Correctly()
    {
        // Tests whether attributes on the derived interface methods correctly override base interface defaults
        const string source = """
            using System.Threading.Tasks;
            using SignalRGen.Generator;
            
            namespace SignalRGen.Example.Contracts;
            
            public interface IBaseInterface
            {
                // This would normally be treated as a server-to-client method (no attribute)
                Task UpdateMessage(string message);
            }
            
            [HubClient(HubUri = "example")]
            public interface IExampleHubClient : IBaseInterface
            {
                // Override the behavior to make it client-to-server
                [ClientToServerMethod]
                new Task UpdateMessage(string message);
                
                Task ReceiveExampleCountUpdate(int count);
            }
            """;
        
        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task Generates_HubClient_WithGenericParameters_Correctly()
    {
        // Tests inheritance with generic parameters in methods
        const string source = """
            using System.Threading.Tasks;
            using System.Collections.Generic;
            using SignalRGen.Generator;
            
            namespace SignalRGen.Example.Contracts;
            
            public interface IBaseInterface
            {
                Task ReceiveCollection<T>(IEnumerable<T> items);
                
                [ClientToServerMethod]
                Task<IEnumerable<T>> RequestItems<T>(string filter);
            }
            
            [HubClient(HubUri = "example")]
            public interface IExampleHubClient : IBaseInterface
            {
                Task ReceiveExampleCountUpdate(int count);
            }
            """;
        
        return TestHelper.Verify(source);
    }
}