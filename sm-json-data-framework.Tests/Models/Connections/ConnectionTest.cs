using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Models.Connections
{
    public class ConnectionTest
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
            Connection connection = model.Connections[model.Rooms["Parlor and Alcatraz"].Nodes[7].IdentifyingString];

            Assert.Equal(ConnectionTypeEnum.VerticalDoor, connection.ConnectionType);
            Assert.Equal("Parlor and Alcatraz", connection.FromNode.RoomName);
            Assert.Equal("Climb", connection.ToNode.RoomName);
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
            Assert.True(unfollowableconnection.LogicallyRelevant);

            Connection followableconnection = model.Connections[model.GetNodeInRoom("Landing Site", 1).IdentifyingString];
            Assert.True(followableconnection.LogicallyRelevant);
        }

        #endregion
    }
}
