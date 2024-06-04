using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.InGameStates;
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
    public class ConnectionNodeTest
    {
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.ModelWithOptions;

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnConnectionNodes()
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
