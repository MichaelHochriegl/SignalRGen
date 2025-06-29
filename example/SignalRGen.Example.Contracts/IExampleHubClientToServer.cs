namespace SignalRGen.Example.Contracts;

public interface IExampleHubClientToServer
{
    Task<string> SendExampleMessage(string myClientMessage);
    
    Task SendWithoutReturnType(string myClientMessage);
}