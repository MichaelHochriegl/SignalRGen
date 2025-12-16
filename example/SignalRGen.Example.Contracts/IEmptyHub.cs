using SignalRGen.Abstractions;
using SignalRGen.Abstractions.Attributes;

namespace SignalRGen.Example.Contracts;

[HubClient(HubUri = "empty")]
public interface IEmptyHub : IBidirectionalHub<IEmptyServerToClient, IEmptyClientToServer>
{
    
}

public interface IEmptyServerToClient
{
    
}

public interface IEmptyClientToServer
{
    
}