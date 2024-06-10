using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements;

namespace sm_json_data_framework.Tests.Models.Rooms.Nodes
{
    public class RunwayTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            Runway runway = Model.Runways["Base Runway - West Ocean Left Door (to Moat)"];
            Assert.Equal("Base Runway - West Ocean Left Door (to Moat)", runway.Name);
            Assert.Equal(23, runway.Length);
            Assert.Equal(1, runway.SteepDownTiles);
            Assert.Equal(6, runway.SteepUpTiles);
            Assert.Equal(1, runway.Strats.Count);
            Assert.Contains("Base", runway.Strats.Keys);
            Assert.True(runway.UsableComingIn);
            Assert.Same(Model.Rooms["West Ocean"].Nodes[1], runway.Node);
            Assert.Equal(1, runway.OpenEnds);

            Runway gentleTilesRunway = Model.Runways["Base Runway - Lower Mushrooms Left Door (to Elevator)"];
            Assert.Equal(2, gentleTilesRunway.GentleDownTiles);
            Assert.Equal(4, gentleTilesRunway.GentleUpTiles);

            Runway endingUpTilesRunway = Model.Runways["Base Runway - Lower Norfair Fireflea Room"];
            Assert.Equal(1, endingUpTilesRunway.EndingUpTiles);

            Runway startingDownTilesRunway = Model.Runways["Runway with no Enemies - Purple Farming Room Door (to Purple Shaft)"];
            Assert.Equal(2, startingDownTilesRunway.StartingDownTiles);

            Runway noOpenEndRunway = Model.Runways["Base Runway - Construction Zone Top Left Door (to Morph Ball)"];
            Assert.Equal(2, noOpenEndRunway.Length);
            Assert.Equal(0, noOpenEndRunway.GentleUpTiles);
            Assert.Equal(0, noOpenEndRunway.GentleDownTiles);
            Assert.Equal(0, noOpenEndRunway.SteepUpTiles);
            Assert.Equal(0, noOpenEndRunway.SteepDownTiles);
            Assert.Equal(0, noOpenEndRunway.StartingDownTiles);
            Assert.Equal(0, noOpenEndRunway.EndingUpTiles);
            Assert.Equal(0, noOpenEndRunway.OpenEnds);
            Assert.False(noOpenEndRunway.UsableComingIn);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_ComingIn_NotUsableComingIn_Fails()
        {
            // Given
            Runway runway = Model.Runways["Base Runway - Construction Zone Top Left Door (to Morph Ball)"];
            InGameState inGameState = Model.CreateInitialGameState();

            // When
            ExecutionResult result = runway.Execute(Model, inGameState, comingIn: true);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_NoUsableStrat_Fails()
        {
            // Given
            Runway runway = Model.Runways["Base Runway - Oasis Left Door (to West Sand Hall)"];
            InGameState inGameState = Model.CreateInitialGameState();

            // When
            ExecutionResult result = runway.Execute(Model, inGameState, comingIn: true);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_Usable_Succeeds()
        {
            // Given
            Runway runway = Model.Runways["Base Runway - Oasis Left Door (to West Sand Hall)"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(Model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME]);

            // When
            ExecutionResult result = runway.Execute(Model, inGameState, comingIn: true);

            // Expect
            Assert.NotNull(result);

            Assert.Single(result.RunwaysUsed);
            Assert.Same(runway, result.RunwaysUsed[runway.Name].runwayUsed);
            Assert.Same(runway.Strats["Base"], result.RunwaysUsed[runway.Name].stratUsed);

            Assert.Empty(result.CanLeaveChargedExecuted);
            Assert.Single(result.ItemsInvolved);
            Assert.Same(Model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME], result.ItemsInvolved[SuperMetroidModel.GRAVITY_SUIT_NAME]);

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

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.VARIA_SUIT_NAME])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Runway freeRunway = ModelWithOptions.GetNodeInRoom("Fast Pillars Setup Room", 2).Runways["Base Runway - Fast Pillars Setup Room Bottom Left Door (to Fast Rippers)"];
            Assert.True(freeRunway.LogicallyRelevant);
            Assert.True(freeRunway.LogicallyAlways);
            Assert.True(freeRunway.LogicallyFree);
            Assert.False(freeRunway.LogicallyNever);
            Assert.Equal(2 * (32M / 27M) + 10, freeRunway.LogicalEffectiveRunwayLength);
            Assert.Equal(2 * (32M / 27M) + 10, freeRunway.LogicalEffectiveReversibleRunwayLength);
            Assert.Equal(2 * (32M / 27M) + 10, freeRunway.LogicalEffectiveRunwayLengthNoCharge);

            Runway neverRunway = ModelWithOptions.GetNodeInRoom("Oasis", 2).Runways["Base Runway - Oasis Right Door (to East Sand Hall)"];
            Assert.False(neverRunway.LogicallyRelevant);
            Assert.False(neverRunway.LogicallyAlways);
            Assert.False(neverRunway.LogicallyFree);
            Assert.True(neverRunway.LogicallyNever);
            Assert.Equal(12, neverRunway.LogicalEffectiveRunwayLength);
            Assert.Equal(12, neverRunway.LogicalEffectiveReversibleRunwayLength);
            Assert.Equal(12, neverRunway.LogicalEffectiveRunwayLengthNoCharge);

            Runway possibleRunway = ModelWithOptions.GetNodeInRoom("Golden Torizo's Room", 2).Runways["Base Runway - Golden Torizo Room Right Door (to Screw Attack)"];
            Assert.True(possibleRunway.LogicallyRelevant);
            Assert.False(possibleRunway.LogicallyAlways);
            Assert.False(possibleRunway.LogicallyFree);
            Assert.False(possibleRunway.LogicallyNever);
            Assert.Equal(28, possibleRunway.LogicalEffectiveRunwayLength);
            Assert.Equal(28, possibleRunway.LogicalEffectiveReversibleRunwayLength);
            Assert.Equal(28, possibleRunway.LogicalEffectiveRunwayLengthNoCharge);
        }

        #endregion
    }
}
