using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Models.Connections
{
    public class ConnectionNodeTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            ConnectionNode connectionNode = Model.Connections[Model.Rooms["Parlor and Alcatraz"].Nodes[7].IdentifyingString].FromNode;

            Assert.Equal("Crateria", connectionNode.Area);
            Assert.Equal("Central", connectionNode.Subarea);
            Assert.Equal(10, connectionNode.Roomid);
            Assert.Equal("Parlor and Alcatraz", connectionNode.RoomName);
            Assert.Equal(7, connectionNode.Nodeid);
            Assert.Equal("Parlor Bottom Door (to Climb)", connectionNode.NodeName);
            Assert.Equal(ConnectionNodePositionEnum.Top, connectionNode.Position);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["PowerBomb"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ReserveTank"], 4);

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Connection unfollowableconnection = ModelWithOptions.Connections[ModelWithOptions.GetNodeInRoom("Big Pink", 8).IdentifyingString];
            // Followability is not considered in-scope for logical relevance
            Assert.True(unfollowableconnection.FromNode.LogicallyRelevant);
            Assert.True(unfollowableconnection.ToNode.LogicallyRelevant);

            Connection followableconnection = ModelWithOptions.Connections[ModelWithOptions.GetNodeInRoom("Landing Site", 1).IdentifyingString];
            Assert.True(followableconnection.FromNode.LogicallyRelevant);
            Assert.True(followableconnection.ToNode.LogicallyRelevant);
        }

        #endregion

    }
}
