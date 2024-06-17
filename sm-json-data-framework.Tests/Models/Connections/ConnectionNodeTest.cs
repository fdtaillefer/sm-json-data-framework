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
        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel NewModelForOptions() => StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation
            SuperMetroidModel model = ReusableModel();

            // Expect
            ConnectionNode connectionNode = model.Connections[model.Rooms["Parlor and Alcatraz"].Nodes[7].IdentifyingString].FromNode;

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
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["PowerBomb"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ReserveTank"], 4);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Connection unfollowableconnection = model.Connections[model.GetNodeInRoom("Big Pink", 8).IdentifyingString];
            // Followability is not considered in-scope for logical relevance
            Assert.True(unfollowableconnection.FromNode.LogicallyRelevant);
            Assert.True(unfollowableconnection.ToNode.LogicallyRelevant);

            Connection followableconnection = model.Connections[model.GetNodeInRoom("Landing Site", 1).IdentifyingString];
            Assert.True(followableconnection.FromNode.LogicallyRelevant);
            Assert.True(followableconnection.ToNode.LogicallyRelevant);
        }

        #endregion

    }
}
