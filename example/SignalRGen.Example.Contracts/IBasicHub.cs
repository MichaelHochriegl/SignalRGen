using SignalRGen.Abstractions;
using SignalRGen.Abstractions.Attributes;

namespace SignalRGen.Example.Contracts;

[HubClient(HubUri = "basic")]
public interface IBasicHub : IBidirectionalHub<IBasicHubServerToClient, IBasicHubClientToServer>
{
    
}

public interface IBasicHubServerToClient
{
    Task SendFromServerToClientMessage(BasicMessage message);
    Task SendFromServerToClientWithList(List<BasicMessage> messages);
    Task SendFromServerToClientPrimitiveTypes(string foo, int bar);
}

public interface IBasicHubClientToServer
{
    Task<BasicReturn> SendFromClientToServerMessage(string message);
    Task<List<BasicReturn>> SendFromClientToServerWithList(List<string> messages);
    Task SendFromClientToServerWithoutReturnType(string message);
    Task<bool> SendFromClientToServerPrimitiveTypes(string foo, int bar);
}

public record BasicMessage(string Message, BasicMessageType Type, DateTimeOffset Timestamp);

public enum BasicMessageType
{
    Info,
    Warning,
    Error
}

public record BasicReturn(BasicMessage Message);