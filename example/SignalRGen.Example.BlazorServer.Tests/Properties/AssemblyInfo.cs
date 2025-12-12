using SignalRGen.Example.Contracts;
using SignalRGen.Testing.Abstractions.Attributes;

// With these attributes we tell SignalRGen.Testing for which HubClients we want a fake generated.
[assembly: GenerateFakeForHubClient(typeof(ExampleHubClient))]
[assembly: GenerateFakeForHubClient(typeof(ChatHubContractClient))]
[assembly: GenerateFakeForHubClient(typeof(BasicHubClient))]