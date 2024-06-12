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
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;

namespace sm_json_data_framework.Tests.Models.Rooms
{
    public class StratObstacleTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            StratObstacle stratObstacle = Model.Rooms["Wrecked Ship Main Shaft"].Links[7].To[9].Strats["Power Bombs"].Obstacles["B"];
            Assert.Same(Model.Rooms["Wrecked Ship Main Shaft"].Obstacles["B"], stratObstacle.Obstacle);
            Assert.NotNull(stratObstacle.Requires);
            Assert.Equal(1, stratObstacle.Requires.LogicalElements.Count);
            Assert.NotNull(stratObstacle.Requires.LogicalElement<HelperLogicalElement>(0));
            Assert.Null(stratObstacle.Bypass);
            Assert.Equal(1, stratObstacle.AdditionalObstacles.Count);
            Assert.Same(Model.Rooms["Wrecked Ship Main Shaft"].Obstacles["A"], stratObstacle.AdditionalObstacles["A"]);

            StratObstacle stratObstacleWithBypass = Model.Rooms["Post Crocomire Jump Room"].Links[5].To[1].Strats["PCJR Frozen Mella Door"].Obstacles["B"];
            Assert.NotNull(stratObstacleWithBypass.Bypass);
            Assert.Equal(1, stratObstacleWithBypass.Bypass.LogicalElements.Count);
            Assert.NotNull(stratObstacleWithBypass.Bypass.LogicalElement<ItemLogicalElement>(0));
            Assert.Empty(stratObstacleWithBypass.AdditionalObstacles);

            StratObstacle stratObstacleWithNoLocalRequirements = Model.Rooms["Climb"].Links[2].To[6].Strats["Base"].Obstacles["A"];
            Assert.NotNull(stratObstacleWithNoLocalRequirements.Requires);
            Assert.Empty(stratObstacleWithNoLocalRequirements.Requires.LogicalElements);
        }

        #endregion

        #region Tests for DestroyExecution.Execute()

        [Fact]
        public void DestroyExecutionExecute_RequirementsNotMet_Fails()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(Model.Rooms["Morph Ball Room"].Nodes[6]);
            StratObstacle stratObstacle = Model.Rooms["Morph Ball Room"].Links[6].To[1].Strats["Laugh at Dead Sidehoppers"].Obstacles["C"];

            // When
            ExecutionResult result = stratObstacle.DestroyExecution.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void DestroyExecutionExecute_IndestructibleObstacleButAlreadyDestroyed_Fails()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(Model.Rooms["Morph Ball Room"].Nodes[6])
                .ApplyDestroyObstacle(Model.Rooms["Morph Ball Room"].Obstacles["C"]);
            StratObstacle stratObstacle = Model.Rooms["Morph Ball Room"].Links[6].To[1].Strats["Laugh at Dead Sidehoppers"].Obstacles["C"];

            // When
            ExecutionResult result = stratObstacle.DestroyExecution.Execute(Model, inGameState);

            // Expect
            // Whether the obstacle is already destroyed is beyond the scope of executing the destroy requirements
            Assert.Null(result);
        }

        [Fact]
        public void DestroyExecutionExecute_ObstacleHasUnmetCommonRequirements_Fails()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(Model.Rooms["Climb"].Nodes[2]);
            StratObstacle stratObstacle = Model.Rooms["Climb"].Links[2].To[6].Strats["Base"].Obstacles["A"];

            // When
            ExecutionResult result = stratObstacle.DestroyExecution.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void DestroyExecutionExecute_ObstacleHasMetCommonRequirements_Succeeds()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(Model.Items["ScrewAttack"])
                .ApplyEnterRoom(Model.Rooms["Climb"].Nodes[2]);
            StratObstacle stratObstacle = Model.Rooms["Climb"].Links[2].To[6].Strats["Base"].Obstacles["A"];

            // When
            ExecutionResult result = stratObstacle.DestroyExecution.Execute(Model, inGameState);

            // Expect
            Assert.NotNull(result);

            // LastLinkStrat should be updated when moving between nodes, not when executing a strat
            Assert.Null(result.ResultingState.LastLinkStrat);
            Assert.Empty(result.RunwaysUsed);
            Assert.Empty(result.CanLeaveChargedExecuted);
            Assert.Equal(1, result.ItemsInvolved.Count);
            Assert.Contains("ScrewAttack", result.ItemsInvolved.Keys);
            Assert.Empty(result.ActivatedGameFlags);
            Assert.Empty(result.OpenedLocks);
            Assert.Empty(result.BypassedLocks);
            Assert.Empty(result.DamageReducingItemsInvolved);
            Assert.Empty(result.KilledEnemies);

            Assert.Equal(inGameState.Resources, result.ResultingState.Resources);
            Assert.Equal(inGameState.ResourceMaximums, result.ResultingState.ResourceMaximums);
            Assert.Equal(inGameState.OpenedLocks.Count, result.ResultingState.OpenedLocks.Count);
            Assert.Equal(inGameState.ActiveGameFlags.Count, result.ResultingState.ActiveGameFlags.Count);
            Assert.Equal(inGameState.TakenItemLocations.Count, result.ResultingState.TakenItemLocations.Count);
            Assert.Same(inGameState.CurrentRoom, result.ResultingState.CurrentRoom);
            Assert.Same(inGameState.CurrentNode, result.ResultingState.CurrentNode);
            Assert.True(result.ResultingState.Inventory.ExceptIn(inGameState.Inventory).Empty);

            Assert.Contains("A", result.ResultingState.InRoomState.DestroyedObstacleIds);
        }

        [Fact]
        public void DestroyExecutionExecute_DestroyingObstacleDestroysAdditionalObstacle_DestroysBoth()
        {
            // Given
            StratObstacle stratObstacle = Model.Rooms["Wrecked Ship Main Shaft"].Links[7].To[9].Strats["Power Bombs"].Obstacles["B"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(Model.Items["Morph"])
                .ApplyAddItem(Model.Items[SuperMetroidModel.POWER_BOMB_NAME])
                .ApplyRefillResources()
                .ApplyEnterRoom(Model.Rooms["Wrecked Ship Main Shaft"].Nodes[7]);

            // When
            ExecutionResult result = stratObstacle.DestroyExecution.Execute(Model, inGameState);

            // Expect
            Assert.NotNull(result);

            // LastLinkStrat should be updated when moving between nodes, not when executing a strat
            Assert.Null(result.ResultingState.LastLinkStrat);
            Assert.Empty(result.RunwaysUsed);
            Assert.Empty(result.CanLeaveChargedExecuted);
            Assert.Equal(2, result.ItemsInvolved.Count);
            Assert.Contains("Morph", result.ItemsInvolved.Keys);
            Assert.Contains(SuperMetroidModel.POWER_BOMB_NAME, result.ItemsInvolved.Keys);
            Assert.Empty(result.ActivatedGameFlags);
            Assert.Empty(result.OpenedLocks);
            Assert.Empty(result.BypassedLocks);
            Assert.Empty(result.DamageReducingItemsInvolved);
            Assert.Empty(result.KilledEnemies);

            Assert.Equal(4, result.ResultingState.Resources.GetAmount(RechargeableResourceEnum.PowerBomb));
            Assert.Equal(inGameState.ResourceMaximums, result.ResultingState.ResourceMaximums);
            Assert.Equal(inGameState.OpenedLocks.Count, result.ResultingState.OpenedLocks.Count);
            Assert.Equal(inGameState.ActiveGameFlags.Count, result.ResultingState.ActiveGameFlags.Count);
            Assert.Equal(inGameState.TakenItemLocations.Count, result.ResultingState.TakenItemLocations.Count);
            Assert.Same(inGameState.CurrentRoom, result.ResultingState.CurrentRoom);
            Assert.Same(inGameState.CurrentNode, result.ResultingState.CurrentNode);
            Assert.True(result.ResultingState.Inventory.ExceptIn(inGameState.Inventory).Empty);

            Assert.Contains("A", result.ResultingState.InRoomState.DestroyedObstacleIds);
            Assert.Contains("B", result.ResultingState.InRoomState.DestroyedObstacleIds);
        }

        #endregion

        #region Tests for BypassExecution.Execute()

        [Fact]
        public void BypassExecutionExecute_RequirementsNotMet_Fails()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(Model.Rooms["Post Crocomire Jump Room"].Nodes[5]);
            StratObstacle stratObstacle = Model.Rooms["Post Crocomire Jump Room"].Links[5].To[1].Strats["PCJR Frozen Mella Door"].Obstacles["B"];

            // When
            ExecutionResult result = stratObstacle.BypassExecution.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void BypassExecutionExecute_RequirementsMet_Succeeds()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(Model.Items["Morph"])
                .ApplyEnterRoom(Model.Rooms["Post Crocomire Jump Room"].Nodes[5]);
            StratObstacle stratObstacle = Model.Rooms["Post Crocomire Jump Room"].Links[5].To[1].Strats["PCJR Frozen Mella Door"].Obstacles["B"];

            // When
            ExecutionResult result = stratObstacle.BypassExecution.Execute(Model, inGameState);

            // Expect
            Assert.NotNull(result);

            Assert.Null(result.ResultingState.LastLinkStrat);
            Assert.Empty(result.RunwaysUsed);
            Assert.Empty(result.CanLeaveChargedExecuted);
            Assert.Equal(1, result.ItemsInvolved.Count);
            Assert.Contains("Morph", result.ItemsInvolved.Keys);
            Assert.Empty(result.ActivatedGameFlags);
            Assert.Empty(result.OpenedLocks);
            Assert.Empty(result.BypassedLocks);
            Assert.Empty(result.DamageReducingItemsInvolved);
            Assert.Empty(result.KilledEnemies);

            Assert.Equal(inGameState.Resources, result.ResultingState.Resources);
            Assert.Equal(inGameState.ResourceMaximums, result.ResultingState.ResourceMaximums);
            Assert.Equal(inGameState.OpenedLocks.Count, result.ResultingState.OpenedLocks.Count);
            Assert.Equal(inGameState.ActiveGameFlags.Count, result.ResultingState.ActiveGameFlags.Count);
            Assert.Equal(inGameState.TakenItemLocations.Count, result.ResultingState.TakenItemLocations.Count);
            Assert.Same(inGameState.CurrentRoom, result.ResultingState.CurrentRoom);
            Assert.Same(inGameState.CurrentNode, result.ResultingState.CurrentNode);
            Assert.True(result.ResultingState.Inventory.ExceptIn(inGameState.Inventory).Empty);

            Assert.DoesNotContain("B", result.ResultingState.InRoomState.DestroyedObstacleIds);
        }

        // Succeed

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .RegisterRemovedItem("ScrewAttack")
                .RegisterRemovedItem("Bombs")
                .RegisterDisabledGameFlag("f_ZebesSetAblaze");
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ReserveTank"], 4);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Morph"])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            StratObstacle fullyImpossibleStratObstacle = ModelWithOptions.Rooms["Climb"].Links[2].To[6].Strats["Base"].Obstacles["A"];
            Assert.True(fullyImpossibleStratObstacle.LogicallyRelevant);
            Assert.True(fullyImpossibleStratObstacle.LogicallyNever);
            Assert.True(fullyImpossibleStratObstacle.LogicallyNeverFromHere);
            Assert.False(fullyImpossibleStratObstacle.LogicallyAlways);
            Assert.False(fullyImpossibleStratObstacle.LogicallyFree);

            StratObstacle locallyIndestructibleFreeToBypassStratObstacle = ModelWithOptions.Rooms["Post Crocomire Jump Room"].Links[5].To[1].Strats["PCJR Frozen Mella Door"].Obstacles["B"];
            Assert.True(locallyIndestructibleFreeToBypassStratObstacle.LogicallyRelevant);
            Assert.False(locallyIndestructibleFreeToBypassStratObstacle.LogicallyNever);
            Assert.False(locallyIndestructibleFreeToBypassStratObstacle.LogicallyNeverFromHere);
            Assert.True(locallyIndestructibleFreeToBypassStratObstacle.LogicallyAlways);
            Assert.True(locallyIndestructibleFreeToBypassStratObstacle.LogicallyFree);

            StratObstacle locallyImpossibleStratObstacle = ModelWithOptions.Rooms["Post Crocomire Jump Room"].Nodes[2].CanLeaveCharged.First().Strats["Speed Blocks Broken"].Obstacles["B"];
            Assert.True(locallyImpossibleStratObstacle.LogicallyRelevant);
            Assert.False(locallyImpossibleStratObstacle.LogicallyNever);
            Assert.True(locallyImpossibleStratObstacle.LogicallyNeverFromHere);
            Assert.False(locallyImpossibleStratObstacle.LogicallyAlways);
            Assert.False(locallyImpossibleStratObstacle.LogicallyFree);

            StratObstacle freeToDestroyStratObstacle = ModelWithOptions.Rooms["Pink Brinstar Hopper Room"].Links[2].To[1].Strats["Base"].Obstacles["B"];
            Assert.True(freeToDestroyStratObstacle.LogicallyRelevant);
            Assert.False(freeToDestroyStratObstacle.LogicallyNever);
            Assert.False(freeToDestroyStratObstacle.LogicallyNeverFromHere);
            Assert.True(freeToDestroyStratObstacle.LogicallyAlways);
            Assert.True(freeToDestroyStratObstacle.LogicallyFree);
        }

        #endregion
    }
}
