// ReSharper disable InconsistentNaming
namespace SignalRGen.Analyzers;

public static class DiagnosticIds
{
    public const string SRG0001NoMethodsInHubContractAllowed = "SRG0001";
    public const string SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract = "SRG0002";
    public const string SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract = "SRG0003";
}