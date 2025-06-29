namespace SignalRGen.Example.Contracts;

public interface IExampleHubServerToClient
{
    Task ReceiveExampleCountUpdate(int count);
    Task<string> IllegalReturnType();
    Task<string> GetMessage();     // ❌ Error + Quick Fix available
    Task<int> GetCount();

}