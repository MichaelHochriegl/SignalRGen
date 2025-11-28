using SignalRGen.Example.Contracts;
using SignalRGen.Testing.Abstractions.Attributes;

[assembly: GenerateFakeForHubClient(typeof(ExampleHubClient))]
[assembly: GenerateFakeForHubClient(typeof(ChatHubContractClient))]