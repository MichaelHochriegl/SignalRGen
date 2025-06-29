namespace SignalRGen.Example.Contracts;

public interface IExampleHubServerToClient
{
    Task ReceiveExampleCountUpdate(int count);
}