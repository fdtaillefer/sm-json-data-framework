using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;

namespace sm_json_data_framework.Tests.Models.Rooms
{
    public class StratTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            Strat strat = Model.Rooms["Early Supers Room"].Links[1].To[2].Strats["Early Supers Mockball"];
            Assert.Equal("Early Supers Mockball", strat.Name);
            Assert.True(strat.Notable);
            Assert.NotNull(strat.Requires);

            Assert.Equal(2, strat.Requires.LogicalElements.Count);
            Assert.NotNull(strat.Requires.LogicalElement<ResetRoom>(0));
            Assert.NotNull(strat.Requires.LogicalElement<TechLogicalElement>(0));

            Assert.Empty(strat.Obstacles);

            Assert.Equal(1, strat.Failures.Count);
            Assert.Contains("Crumble Fall", strat.Failures.Keys);

            Assert.Empty(strat.StratProperties);


            Strat stratWithObstacles = Model.Rooms["Morph Ball Room"].Links[4].To[5].Strats["Base"];
            Assert.False(stratWithObstacles.Notable);
            Assert.Equal(1, stratWithObstacles.Obstacles.Count);
            Assert.Contains("B", stratWithObstacles.Obstacles.Keys);
            Assert.NotNull(stratWithObstacles.Requires);
            Assert.Empty(stratWithObstacles.Requires.LogicalElements);
            Assert.Empty(stratWithObstacles.Failures);

            Strat stratWithProperties = Model.Rooms["Crumble Shaft"].Links[1].To[3].Strats["Space Jump"];
            Assert.Equal(1, stratWithProperties.StratProperties.Count);
            Assert.Contains("spinjump", stratWithProperties.StratProperties);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_PossibleStrat_Succeeds()
        {
            // Given
            Strat strat = Model.Rooms["Morph Ball Room"].Links[6].To[5].Strats["Bomb the Blocks"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(Model.Items["Morph"])
                .ApplyAddItem(Model.Items["Bombs"])
                .ApplyEnterRoom(Model.Rooms["Morph Ball Room"].Nodes[6]);

            // When
            ExecutionResult result = strat.Execute(Model, inGameState);

            // Expect
            Assert.NotNull(result);

            // LastLinkStrat should be updated when moving between nodes, not when executing a strat
            Assert.Null(result.ResultingState.LastLinkStrat);
            Assert.Empty(result.RunwaysUsed);
            Assert.Empty(result.CanLeaveChargedExecuted);
            Assert.Equal(2, result.ItemsInvolved.Count);
            Assert.Contains("Morph", result.ItemsInvolved.Keys);
            Assert.Contains("Bombs", result.ItemsInvolved.Keys);
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
        }

        [Fact]
        public void Execute_StratRequirementsNotMet_Fails()
        {
            // Given
            Strat strat = Model.Rooms["Construction Zone"].Links[3].To[4].Strats["Base"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(Model.Rooms["Construction Zone"].Nodes[3]);

            // When
            ExecutionResult result = strat.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RequirementsMetButObstacleLocalRequirementsNotMet_Fails()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(Model.Rooms["Morph Ball Room"].Nodes[6]);
            Strat strat = Model.Rooms["Morph Ball Room"].Links[6].To[1].Strats["Laugh at Dead Sidehoppers"];

            // When
            ExecutionResult result = strat.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_LocallyIndestructibleObstacleButAlreadyDestroyed_Succeeds()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(Model.Rooms["Morph Ball Room"].Nodes[6])
                .ApplyDestroyObstacle(Model.Rooms["Morph Ball Room"].Obstacles["C"]);
            Strat strat = Model.Rooms["Morph Ball Room"].Links[6].To[1].Strats["Laugh at Dead Sidehoppers"];

            // When
            ExecutionResult result = strat.Execute(Model, inGameState);

            // Expect
            Assert.NotNull(result);

            // LastLinkStrat should be updated when moving between nodes, not when executing a strat
            Assert.Null(result.ResultingState.LastLinkStrat);
            Assert.Empty(result.RunwaysUsed);
            Assert.Empty(result.CanLeaveChargedExecuted);
            Assert.Empty(result.ItemsInvolved);
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
        }

        [Fact]
        public void Execute_ObstacleCanBeBypassedButNotDestroyed_Succeeds()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(Model.Items["Morph"])
                .ApplyAddItem(Model.Items["Ice"])
                .ApplyEnterRoom(Model.Rooms["Post Crocomire Jump Room"].Nodes[5]);
            Strat strat = Model.Rooms["Post Crocomire Jump Room"].Links[5].To[1].Strats["PCJR Frozen Mella Door"];

            // When
            ExecutionResult result = strat.Execute(Model, inGameState);

            // Expect
            Assert.NotNull(result);

            // LastLinkStrat should be updated when moving between nodes, not when executing a strat
            Assert.Null(result.ResultingState.LastLinkStrat);
            Assert.Empty(result.RunwaysUsed);
            Assert.Empty(result.CanLeaveChargedExecuted);
            Assert.Equal(2, result.ItemsInvolved.Count);
            Assert.Contains("Morph", result.ItemsInvolved.Keys);
            Assert.Contains("Ice", result.ItemsInvolved.Keys);
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
        }

        [Fact]
        public void Execute_ObstacleHasUnmetCommonRequirements_Fails()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(Model.Rooms["Climb"].Nodes[2]);
            Strat strat = Model.Rooms["Climb"].Links[2].To[6].Strats["Base"];

            // When
            ExecutionResult result = strat.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_ObstacleHasMetCommonRequirements_Succeeds()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(Model.Items["ScrewAttack"])
                .ApplyEnterRoom(Model.Rooms["Climb"].Nodes[2]);
            Strat strat = Model.Rooms["Climb"].Links[2].To[6].Strats["Base"];


            // When
            ExecutionResult result = strat.Execute(Model, inGameState);

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
        }

        [Fact]
        public void Execute_ConfiguredToTakeMultipleTries_UsesMoreResources()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterStratTries("Ceiling E-Tank Dboost", 3);
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);
            InGameState inGameState = ModelWithOptions.CreateInitialGameState()
                .ApplyAddGameFlag(ModelWithOptions.GameFlags["f_ZebesAwake"])
                .ApplyEnterRoom(ModelWithOptions.Rooms["Blue Brinstar Energy Tank Room"].Nodes[2]);
            Strat strat = ModelWithOptions.Rooms["Blue Brinstar Energy Tank Room"].Links[1].To[3].Strats["Ceiling E-Tank Dboost"];

            // When
            ExecutionResult result = strat.Execute(ModelWithOptions, inGameState);

            // Expect
            Assert.NotNull(result);

            // LastLinkStrat should be updated when moving between nodes, not when executing a strat
            Assert.Null(result.ResultingState.LastLinkStrat);
            Assert.Empty(result.RunwaysUsed);
            Assert.Empty(result.CanLeaveChargedExecuted);
            Assert.Empty(result.ItemsInvolved);
            Assert.Empty(result.ActivatedGameFlags);
            Assert.Empty(result.OpenedLocks);
            Assert.Empty(result.BypassedLocks);
            Assert.Empty(result.DamageReducingItemsInvolved);
            Assert.Empty(result.KilledEnemies);

            // Should have taken 3 Reo hits of damage rather than 1
            Assert.Equal(54, result.ResultingState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(inGameState.ResourceMaximums, result.ResultingState.ResourceMaximums);
            Assert.Equal(inGameState.OpenedLocks.Count, result.ResultingState.OpenedLocks.Count);
            Assert.Equal(inGameState.ActiveGameFlags.Count, result.ResultingState.ActiveGameFlags.Count);
            Assert.Equal(inGameState.TakenItemLocations.Count, result.ResultingState.TakenItemLocations.Count);
            Assert.Same(inGameState.CurrentRoom, result.ResultingState.CurrentRoom);
            Assert.Same(inGameState.CurrentNode, result.ResultingState.CurrentNode);
            Assert.True(result.ResultingState.Inventory.ExceptIn(inGameState.Inventory).Empty);
        }

        [Fact]
        public void Execute_DisabledByConfiguration_Fails()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledStrat("Ceiling E-Tank Dboost");
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);
            InGameState inGameState = ModelWithOptions.CreateInitialGameState()
                .ApplyAddGameFlag(ModelWithOptions.GameFlags["f_ZebesAwake"])
                .ApplyEnterRoom(ModelWithOptions.Rooms["Blue Brinstar Energy Tank Room"].Nodes[2]);
            Strat strat = ModelWithOptions.Rooms["Blue Brinstar Energy Tank Room"].Links[1].To[3].Strats["Ceiling E-Tank Dboost"];

            // When
            ExecutionResult result = strat.Execute(ModelWithOptions, inGameState);

            // Expect
            Assert.Null(result);
        }

        #endregion


        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Grapple")
                .RegisterDisabledStrat("Ceiling E-Tank Dboost");

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Strat impossibleRequirementsStrat = ModelWithOptions.Rooms["Pants Room"].Links[4].To[5].Strats["Base"];
            Assert.False(impossibleRequirementsStrat.LogicallyRelevant);
            Assert.False(impossibleRequirementsStrat.LogicallyAlways);
            Assert.False(impossibleRequirementsStrat.LogicallyFree);
            Assert.True(impossibleRequirementsStrat.LogicallyNever);

            Strat disabledStrat = ModelWithOptions.Rooms["Blue Brinstar Energy Tank Room"].Links[1].To[3].Strats["Ceiling E-Tank Dboost"];
            Assert.False(disabledStrat.LogicallyRelevant);
            Assert.False(disabledStrat.LogicallyAlways);
            Assert.False(disabledStrat.LogicallyFree);
            Assert.True(disabledStrat.LogicallyNever);

            Strat nonFreeStrat = ModelWithOptions.Rooms["Blue Brinstar Energy Tank Room"].Links[1].To[3].Strats["Ceiling E-Tank Speed Jump"];
            Assert.True(nonFreeStrat.LogicallyRelevant);
            Assert.False(nonFreeStrat.LogicallyAlways);
            Assert.False(nonFreeStrat.LogicallyFree);
            Assert.False(nonFreeStrat.LogicallyNever);

            Strat freeStrat = ModelWithOptions.Rooms["Landing Site"].Links[5].To[4].Strats["Base"];
            Assert.True(freeStrat.LogicallyRelevant);
            Assert.True(freeStrat.LogicallyAlways);
            Assert.True(freeStrat.LogicallyFree);
            Assert.False(freeStrat.LogicallyNever);
        }

        #endregion
    }
}
