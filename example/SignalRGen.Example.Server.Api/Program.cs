using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRGen.Example.Contracts;
using SignalRGen.Example.Server.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// We add the SignalR services from Microsoft.
builder.Services.AddSignalR();
// We add our custom provider to get the username from the headers.
builder.Services.AddSingleton<IUserIdProvider, UsernameUserIdProvider>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapHub<ExampleHub>($"/{ExampleHubClient.HubUri}");
// To wire up our ChatHub, we have to map it here.
// We use the `HubUri` from the generated client.
// This ensures that both client and server will use the same route.
app.MapHub<ChatHub>($"{ChatHubContractClient.HubUri}");

app.MapGet("/", () => "Hello World!");
app.MapPost("/sendCount",
    async (int count, [FromServices] IHubContext<ExampleHub, IExampleHubServerToClient> hub) =>
    await hub.Clients.All.ReceiveExampleCountUpdate(count));

app.Run();