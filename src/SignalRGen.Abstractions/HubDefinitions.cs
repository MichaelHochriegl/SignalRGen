namespace SignalRGen.Abstractions;

/// <summary>
/// Represents a bidirectional hub interface that provides a server-to-client and client-to-server contract.
/// </summary>
/// <typeparam name="TServer">The interface defining the server-to-client communication contract.</typeparam>
/// <typeparam name="TClient">The interface defining the client-to-server communication contract.</typeparam>
public interface IBidirectionalHub<TServer, TClient>
    where TServer : class
    where TClient : class
{
}

/// <summary>
/// Represents a one-way hub interface that defines the server-to-client communication contract.
/// </summary>
/// <typeparam name="TServer">The interface defining the server-to-client communication contract.</typeparam>
public interface IServerToClientHub<TServer>
    where TServer : class
{
}

/// <summary>
/// Represents a one-way hub interface that defines the client-to-server communication contract.
/// </summary>
/// <typeparam name="TClient">The interface defining the client-to-server communication contract.</typeparam>
public interface IClientToServerHub<TClient>
    where TClient : class
{
}