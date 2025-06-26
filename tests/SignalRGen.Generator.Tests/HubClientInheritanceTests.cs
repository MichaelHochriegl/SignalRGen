namespace SignalRGen.Generator.Tests;

public class HubClientInheritanceTests
{
    [Fact]
    public Task Generates_HubClient_WithOnlyBaseInterfaceMethods_Correctly()
    {
        // A simple case with a client interface that inherits from another interface but doesn't add its own methods
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Example.Contracts;

                              public interface IBaseServerToClientMethods
                              {
                                  Task ReceiveBaseMessage(string message);
                              }

                              public interface IBaseClientToServerMethods
                              {
                                  Task<int> SendBaseMessage(string message);
                              }

                              [HubClient(HubUri = "example")]
                              public interface IExampleHub : IBidirectionalHub<IBaseServerToClientMethods, IBaseClientToServerMethods>
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
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Example.Contracts;

                              public interface IBaseServerToClientMethods
                              {
                                  Task ReceiveBaseMessage(string message);
                              }

                              public interface IBaseClientToServerMethods
                              {
                                  Task<int> SendBaseMessage(string message);
                              }

                              public interface IExampleServerToClientMethods : IBaseServerToClientMethods
                              {
                                  Task ReceiveExampleCountUpdate(int count);
                              }

                              public interface IExampleClientToServerMethods : IBaseClientToServerMethods
                              {
                                  Task<string> SendExampleMessage(string myClientMessage);
                              }

                              [HubClient(HubUri = "example")]
                              public interface IExampleHub : IBidirectionalHub<IExampleServerToClientMethods, IExampleClientToServerMethods>
                              {
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
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Example.Contracts;

                              public interface IBaseServerToClientMethods
                              {
                                  Task ReceiveBaseMessage(string message);
                              }

                              public interface IMidServerToClientMethods : IBaseServerToClientMethods
                              {
                                  // No additional server-to-client methods
                              }

                              public interface IMidClientToServerMethods
                              {
                                  Task<int> SendMidMessage(int value);
                              }

                              public interface IExampleServerToClientMethods : IMidServerToClientMethods
                              {
                                  Task ReceiveExampleCountUpdate(int count);
                              }

                              [HubClient(HubUri = "example")]
                              public interface IExampleHub : IBidirectionalHub<IExampleServerToClientMethods, IMidClientToServerMethods>
                              {
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
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Example.Contracts;

                              public interface IBaseInterface1
                              {
                                  Task ReceiveBase1Message(string message);
                              }

                              public interface IBaseInterface2
                              {
                                  Task<int> SendBase2Message(int value);
                              }

                              public interface ICombinedServerToClientMethods : IBaseInterface1
                              {
                                  Task ReceiveExampleCountUpdate(int count);
                              }

                              [HubClient(HubUri = "example")]
                              public interface IExampleHub : IBidirectionalHub<ICombinedServerToClientMethods, IBaseInterface2>
                              {
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
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Example.Contracts;

                              public interface IServerToClientMethods1
                              {
                                  Task CommonMethod(string param1);
                              }

                              public interface IClientToServerMethods1
                              {
                                  // Different signature but same name as in IServerToClientMethods1
                                  Task<int> CommonMethod(int param1);
                              }

                              public interface ICombinedServerToClientMethods : IServerToClientMethods1
                              {
                                  Task ReceiveExampleCountUpdate(int count);
                              }

                              [HubClient(HubUri = "example")]
                              public interface IExampleHub : IBidirectionalHub<ICombinedServerToClientMethods, IClientToServerMethods1>
                              {
                              }
                              """;

        
        return TestHelper.Verify(source);
    }
    
    [Fact(Skip = "This is currently not exposing both, as the same-signature-check will sort it out.")]
    public Task Generates_HubClient_WithSameMethodSignatureInServerAndClientExposesBoth_Correctly()
    {
        // Tests whether attributes on the derived interface methods correctly override base interface defaults
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Example.Contracts;

                              public interface IServerToClientMethods
                              {
                                  Task UpdateMessage(string message);
                                  Task ReceiveExampleCountUpdate(int count);
                              }

                              public interface IClientToServerMethods
                              {
                                  // Same method name but now it's client-to-server
                                  Task UpdateMessage(string message);
                              }

                              [HubClient(HubUri = "example")]
                              public interface IExampleHub : IBidirectionalHub<IServerToClientMethods, IClientToServerMethods>
                              {
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
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Example.Contracts;

                              public interface IBaseServerToClientMethods
                              {
                                  Task ReceiveCollection<T>(IEnumerable<T> items);
                              }

                              public interface IBaseClientToServerMethods
                              {
                                  Task<IEnumerable<T>> RequestItems<T>(string filter);
                              }

                              public interface IExampleServerToClientMethods : IBaseServerToClientMethods
                              {
                                  Task ReceiveExampleCountUpdate(int count);
                              }

                              [HubClient(HubUri = "example")]
                              public interface IExampleHub : IBidirectionalHub<IExampleServerToClientMethods, IBaseClientToServerMethods>
                              {
                              }
                              """;

        
        return TestHelper.Verify(source);
    }
}