using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Rooms;

namespace sm_json_data_framework.Tests.Models.Rooms
{
    public class RoomEnvironmentTest
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
            RoomEnvironment roomEnvironment = model.Rooms["Spiky Platforms Tunnel"].RoomEnvironments.First();
            Assert.True(roomEnvironment.Heated);
            Assert.Null(roomEnvironment.EntranceNodes);
            Assert.Same(model.Rooms["Spiky Platforms Tunnel"], roomEnvironment.Room);

            RoomEnvironment roomEnvironmentWithEntranceNode = model.Rooms["Volcano Room"].RoomEnvironments.First();
            Assert.False(roomEnvironmentWithEntranceNode.Heated);
            Assert.Equal(1, roomEnvironmentWithEntranceNode.EntranceNodes.Count);
            Assert.Same(model.Rooms["Volcano Room"].Nodes[1], roomEnvironmentWithEntranceNode.EntranceNodes[1]);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            foreach (RoomEnvironment roomEnvironment in model.Rooms["Volcano Room"].RoomEnvironments)
            {
                Assert.True(roomEnvironment.LogicallyRelevant);
            }
        }

        #endregion
    }
}
