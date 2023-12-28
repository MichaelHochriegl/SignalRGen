using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.SignalR;
using SignalRGen.Example.Contracts;
using SignalRGen.Example.Server.Api.Hubs;
using SignalRGen.Generator;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapHub<ExampleHub>($"/{ExampleHubClient.HubUri}");
app.MapGet("/", () => "Hello World!");
app.MapPost("/sendCount", async (int count ,[FromServices]IHubContext<ExampleHub, IExampleHubClient> hub) => await hub.Clients.All.ReceiveExampleCountUpdate(count));

app.Run();