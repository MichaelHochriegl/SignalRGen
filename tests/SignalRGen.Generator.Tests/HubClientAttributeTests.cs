namespace SignalRGen.Generator.Tests;

[UsesVerify]
public class HubClientAttributeTests
{
    [Fact]
    public Task Generates_HubClientAttribute_Correctly()
    {
        // Arrange
        var source = @"namespace SignalRGen.Generator;

[System.AttributeUsage(System.AttributeTargets.Interface)]
public sealed class HubClientAttribute : System.Attribute
{
    public string? HubName;
}";

        // Act + Assert
        return TestHelper.Verify(source);
    }
}