using Microsoft.AspNetCore.SignalR;
using SignalRGen.Example.Contracts;

namespace SignalRGen.Example.Server.Api.Hubs;

// This is our Hub on the server side.
// We use the two interfaces we defined in the Contracts project to also make the server side typesafe.
public class ChatHub : Hub<IChatHubServerToClient>, IChatHubClientToServer
{
    // We use the `OnConnectedAsync` method to notify the other clients that a new user joined.
    public override async Task OnConnectedAsync()
    {
        await Clients.Others.UserJoined(Context.UserIdentifier ?? "Anonymous");
        await base.OnConnectedAsync();
    }

    public Task SendMessage(string message)
    {
        var user = Context.UserIdentifier ?? "Anonymous";
        return Clients.Others.MessageReceived(new ChatMessage(user, message, DateTime.UtcNow));
    }

    // We use the `OnDisconnectedAsync` method to notify the other clients that a user left.
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.Others.UserLeft(Context.UserIdentifier ?? "Anonymous");
        await base.OnDisconnectedAsync(exception);
    }
}

public class UsernameUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        // Get username from query parameter
        return connection.GetHttpContext()?.Request.Headers["username"].FirstOrDefault() ?? "Anonymous";
    }
}
