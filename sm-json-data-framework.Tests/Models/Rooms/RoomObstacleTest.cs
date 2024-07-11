using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.InGameStates;

namespace sm_json_data_framework.Tests.Models.Rooms
{
    public class RoomObstacleTest
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
            RoomObstacle roomObstacle = model.Rooms["Climb"].Obstacles["A"];
            Assert.Equal("A", roomObstacle.Id);
            Assert.Equal("Bottom Bomb Blocks", roomObstacle.Name);
            Assert.Equal(ObstacleTypeEnum.Inanimate, roomObstacle.ObstacleType);
            Assert.NotNull(roomObstacle.Requires);
            Assert.Equal(1, roomObstacle.Requires.LogicalElements.Count);
            Assert.NotNull(roomObstacle.Requires.LogicalElement<Or>(0));
            Assert.Same(model.Rooms["Climb"], roomObstacle.Room);

            RoomObstacle noRequirementsRoomObstacle = model.Rooms["Morph Ball Room"].Obstacles["A"];
            Assert.NotNull(noRequirementsRoomObstacle.Requires);
            Assert.Empty(noRequirementsRoomObstacle.Requires.LogicalElements);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_ImpossibleObstacleCommonRequirements_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .RegisterRemovedItem("ScrewAttack")
                .RegisterRemovedItem("Morph")
                .RegisterDisabledGameFlag("f_ZebesSetAblaze");
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ReserveTank"], 4);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomObstacle obstacle = model.Rooms["Climb"].Obstacles["A"];
            Assert.True(obstacle.LogicallyRelevant);
            Assert.True(obstacle.LogicallyIndestructible);
            Assert.False(obstacle.LogicallyAlwaysDestructible);
            Assert.False(obstacle.LogicallyDestructibleForFree);
        }

        [Fact]
        public void ApplyLogicalOptions_FreeObstacleCommonRequirements_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items["ScrewAttack"])
            )
            .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomObstacle obstacle = model.Rooms["Climb"].Obstacles["A"];
            Assert.True(obstacle.LogicallyRelevant);
            Assert.False(obstacle.LogicallyIndestructible);
            Assert.True(obstacle.LogicallyAlwaysDestructible);
            Assert.True(obstacle.LogicallyDestructibleForFree);
        }

        #endregion
    }
}
