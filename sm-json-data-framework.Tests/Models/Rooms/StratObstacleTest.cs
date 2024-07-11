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
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.InGameStates;

namespace sm_json_data_framework.Tests.Models.Rooms
{
    public class StratObstacleTest
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
            StratObstacle stratObstacle = model.Rooms["Wrecked Ship Main Shaft"].Links[7].To[9].Strats["Power Bombs"].Obstacles["B"];
            Assert.Same(model.Rooms["Wrecked Ship Main Shaft"].Obstacles["B"], stratObstacle.Obstacle);
            Assert.NotNull(stratObstacle.Requires);
            Assert.Equal(1, stratObstacle.Requires.LogicalElements.Count);
            Assert.NotNull(stratObstacle.Requires.LogicalElement<HelperLogicalElement>(0));
            Assert.Null(stratObstacle.Bypass);
            Assert.Equal(1, stratObstacle.AdditionalObstacles.Count);
            Assert.Same(model.Rooms["Wrecked Ship Main Shaft"].Obstacles["A"], stratObstacle.AdditionalObstacles["A"]);

            StratObstacle stratObstacleWithBypass = model.Rooms["Post Crocomire Jump Room"].Links[5].To[1].Strats["PCJR Frozen Mella Door"].Obstacles["B"];
            Assert.NotNull(stratObstacleWithBypass.Bypass);
            Assert.Equal(1, stratObstacleWithBypass.Bypass.LogicalElements.Count);
            Assert.NotNull(stratObstacleWithBypass.Bypass.LogicalElement<ItemLogicalElement>(0));
            Assert.Empty(stratObstacleWithBypass.AdditionalObstacles);

            StratObstacle stratObstacleWithNoLocalRequirements = model.Rooms["Climb"].Links[2].To[6].Strats["Base"].Obstacles["A"];
            Assert.NotNull(stratObstacleWithNoLocalRequirements.Requires);
            Assert.Empty(stratObstacleWithNoLocalRequirements.Requires.LogicalElements);
        }

        #endregion

        #region Tests for DestroyExecution.Execute()

        [Fact]
        public void DestroyExecutionExecute_RequirementsNotMet_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Morph Ball Room", 6);
            StratObstacle stratObstacle = model.Rooms["Morph Ball Room"].Links[6].To[1].Strats["Laugh at Dead Sidehoppers"].Obstacles["C"];

            // When
            ExecutionResult result = stratObstacle.DestroyExecution.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void DestroyExecutionExecute_IndestructibleObstacleButAlreadyDestroyed_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Morph Ball Room", 6)
                .ApplyDestroyObstacle("C");
            StratObstacle stratObstacle = model.Rooms["Morph Ball Room"].Links[6].To[1].Strats["Laugh at Dead Sidehoppers"].Obstacles["C"];

            // When
            ExecutionResult result = stratObstacle.DestroyExecution.Execute(model, inGameState);

            // Expect
            // Whether the obstacle is already destroyed is beyond the scope of executing the destroy requirements
            Assert.Null(result);
        }

        [Fact]
        public void DestroyExecutionExecute_ObstacleHasUnmetCommonRequirements_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Climb", 2);
            StratObstacle stratObstacle = model.Rooms["Climb"].Links[2].To[6].Strats["Base"].Obstacles["A"];

            // When
            ExecutionResult result = stratObstacle.DestroyExecution.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void DestroyExecutionExecute_ObstacleHasMetCommonRequirements_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("ScrewAttack")
                .ApplyEnterRoom("Climb", 2);
            StratObstacle stratObstacle = model.Rooms["Climb"].Links[2].To[6].Strats["Base"].Obstacles["A"];

            // When
            ExecutionResult result = stratObstacle.DestroyExecution.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved("ScrewAttack")
                .ExpectDestroyedObstacle("A")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void DestroyExecutionExecute_DestroyingObstacleThatHasAdditionalObstacle_DestroysBoth()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StratObstacle stratObstacle = model.Rooms["Wrecked Ship Main Shaft"].Links[7].To[9].Strats["Power Bombs"].Obstacles["B"];
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Morph")
                .ApplyAddItem(SuperMetroidModel.POWER_BOMB_NAME)
                .ApplyRefillResources()
                .ApplyEnterRoom("Wrecked Ship Main Shaft", 7);

            // When
            ExecutionResult result = stratObstacle.DestroyExecution.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved("Morph")
                .ExpectItemInvolved(SuperMetroidModel.POWER_BOMB_NAME)
                .ExpectDestroyedObstacle("A")
                .ExpectDestroyedObstacle("B")
                .ExpectResourceVariation(RechargeableResourceEnum.PowerBomb, -1)
                .AssertRespectedBy(result);
        }

        #endregion

        #region Tests for BypassExecution.Execute()

        [Fact]
        public void BypassExecutionExecute_RequirementsNotMet_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Post Crocomire Jump Room", 5);
            StratObstacle stratObstacle = model.Rooms["Post Crocomire Jump Room"].Links[5].To[1].Strats["PCJR Frozen Mella Door"].Obstacles["B"];

            // When
            ExecutionResult result = stratObstacle.BypassExecution.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void BypassExecutionExecute_RequirementsMet_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Morph")
                .ApplyEnterRoom("Post Crocomire Jump Room", 5);
            StratObstacle stratObstacle = model.Rooms["Post Crocomire Jump Room"].Links[5].To[1].Strats["PCJR Frozen Mella Door"].Obstacles["B"];

            // When
            ExecutionResult result = stratObstacle.BypassExecution.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved("Morph")
                .AssertRespectedBy(result);
        }

        // Succeed

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .RegisterRemovedItem("ScrewAttack")
                .RegisterRemovedItem("Bombs")
                .RegisterDisabledGameFlag("f_ZebesSetAblaze");
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ReserveTank"], 4);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items["Morph"])
                )
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            StratObstacle fullyImpossibleStratObstacle = model.Rooms["Climb"].Links[2].To[6].Strats["Base"].Obstacles["A"];
            Assert.True(fullyImpossibleStratObstacle.LogicallyRelevant);
            Assert.True(fullyImpossibleStratObstacle.LogicallyNever);
            Assert.True(fullyImpossibleStratObstacle.LogicallyNeverFromHere);
            Assert.False(fullyImpossibleStratObstacle.LogicallyAlways);
            Assert.False(fullyImpossibleStratObstacle.LogicallyFree);

            StratObstacle locallyIndestructibleFreeToBypassStratObstacle = model.Rooms["Post Crocomire Jump Room"].Links[5].To[1].Strats["PCJR Frozen Mella Door"].Obstacles["B"];
            Assert.True(locallyIndestructibleFreeToBypassStratObstacle.LogicallyRelevant);
            Assert.False(locallyIndestructibleFreeToBypassStratObstacle.LogicallyNever);
            Assert.False(locallyIndestructibleFreeToBypassStratObstacle.LogicallyNeverFromHere);
            Assert.True(locallyIndestructibleFreeToBypassStratObstacle.LogicallyAlways);
            Assert.True(locallyIndestructibleFreeToBypassStratObstacle.LogicallyFree);

            StratObstacle locallyImpossibleStratObstacle = model.Rooms["Post Crocomire Jump Room"].Nodes[2].CanLeaveCharged.First().Strats["Speed Blocks Broken"].Obstacles["B"];
            Assert.True(locallyImpossibleStratObstacle.LogicallyRelevant);
            Assert.False(locallyImpossibleStratObstacle.LogicallyNever);
            Assert.True(locallyImpossibleStratObstacle.LogicallyNeverFromHere);
            Assert.False(locallyImpossibleStratObstacle.LogicallyAlways);
            Assert.False(locallyImpossibleStratObstacle.LogicallyFree);

            StratObstacle freeToDestroyStratObstacle = model.Rooms["Pink Brinstar Hopper Room"].Links[2].To[1].Strats["Base"].Obstacles["B"];
            Assert.True(freeToDestroyStratObstacle.LogicallyRelevant);
            Assert.False(freeToDestroyStratObstacle.LogicallyNever);
            Assert.False(freeToDestroyStratObstacle.LogicallyNeverFromHere);
            Assert.True(freeToDestroyStratObstacle.LogicallyAlways);
            Assert.True(freeToDestroyStratObstacle.LogicallyFree);
        }

        #endregion
    }
}
