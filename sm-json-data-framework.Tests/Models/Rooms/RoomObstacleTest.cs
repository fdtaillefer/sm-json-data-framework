using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;

namespace sm_json_data_framework.Tests.Models.Rooms
{
    public class RoomObstacleTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            RoomObstacle roomObstacle = Model.Rooms["Climb"].Obstacles["A"];
            Assert.Equal("A", roomObstacle.Id);
            Assert.Equal("Bottom Bomb Blocks", roomObstacle.Name);
            Assert.Equal(ObstacleTypeEnum.Inanimate, roomObstacle.ObstacleType);
            Assert.NotNull(roomObstacle.Requires);
            Assert.Equal(1, roomObstacle.Requires.LogicalElements.Count);
            Assert.NotNull(roomObstacle.Requires.LogicalElement<Or>(0));
            Assert.Same(Model.Rooms["Climb"], roomObstacle.Room);

            RoomObstacle noRequirementsRoomObstacle = Model.Rooms["Morph Ball Room"].Obstacles["A"];
            Assert.NotNull(noRequirementsRoomObstacle.Requires);
            Assert.Empty(noRequirementsRoomObstacle.Requires.LogicalElements);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_ImpossibleObstacleCommonRequirements_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .RegisterRemovedItem("ScrewAttack")
                .RegisterRemovedItem("Morph")
                .RegisterDisabledGameFlag("f_ZebesSetAblaze");
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ReserveTank"], 4);

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomObstacle obstacle = ModelWithOptions.Rooms["Climb"].Obstacles["A"];
            Assert.True(obstacle.LogicallyRelevant);
            Assert.True(obstacle.LogicallyIndestructible);
            Assert.False(obstacle.LogicallyAlwaysDestructible);
            Assert.False(obstacle.LogicallyDestructibleForFree);
        }

        [Fact]
        public void ApplyLogicalOptions_FreeObstacleCommonRequirements_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["ScrewAttack"])
            )
            .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomObstacle obstacle = ModelWithOptions.Rooms["Climb"].Obstacles["A"];
            Assert.True(obstacle.LogicallyRelevant);
            Assert.False(obstacle.LogicallyIndestructible);
            Assert.True(obstacle.LogicallyAlwaysDestructible);
            Assert.True(obstacle.LogicallyDestructibleForFree);
        }

        #endregion
    }
}
