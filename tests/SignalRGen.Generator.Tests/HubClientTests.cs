namespace SignalRGen.Generator.Tests;

[UsesVerify]
public class HubClientTests
{
    [Fact]
    public Task Generates_HubClient_Correctly()
    {
        // Arrange
         const string source = @"
             using System;
             using SignalRGen.Generator.Tests.TestData;
             using SignalRGen.Generator;

             namespace SignalRGen.Clients;

             [HubClient(HubUri = ""examples"")]
             public interface ITestHubClient
             {
                 Task ReceiveCustomTypeUpdate(IEnumerable<CustomTypeDto> customTypes);
                 Task ReceiveFooUpdate(string bar, int bass);
             }";

         return TestHelper.Verify(source);

        
//         const string Source = @"
//             using iCMS.FM.Server.Api.Dtos.Maps;
//
//             namespace iCMS.FM.SignalR.Contracts.MapHub;
//
//             [HubClient]
//             public interface IMapHubClient
//             {
//                 Task ReceiveBlockedNodesUpdate(IEnumerable<BlockedNodeDto> blockedNodes);
//
//                 Task ReceiveReleasedNodesUpdate(IEnumerable<ReleasedNodeDto> releasedBlockedNodes);
//
//                 Task ReceiveBlockedEdgesUpdate(IEnumerable<BlockedEdgeDto> blockedEdges);
//
//                 Task ReceiveReleasedEdgesUpdate(IEnumerable<ReleasedEdgeDto> releasedBlockedEdges);
//
//                 Task ReceiveMapCreated(MapDto createdMap);
//
//                 Task ReceiveMapDeleted(string mapId);
//                 Task Foo(string bar, int awesome);
//             }";
//
//         return TestHelper.Verify(Source);
    }
}