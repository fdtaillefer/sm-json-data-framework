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
        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel NewModelForOptions() => StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation
            SuperMetroidModel model = ReusableModel();

            // Expect
            Runway runway = model.Runways["Base Runway - West Ocean Left Door (to Moat)"];
            Assert.Equal("Base Runway - West Ocean Left Door (to Moat)", runway.Name);
            Assert.Equal(23, runway.Length);
            Assert.Equal(1, runway.SteepDownTiles);
            Assert.Equal(6, runway.SteepUpTiles);
            Assert.Equal(1, runway.Strats.Count);
            Assert.Contains("Base", runway.Strats.Keys);
            Assert.True(runway.UsableComingIn);
            Assert.Same(model.Rooms["West Ocean"].Nodes[1], runway.Node);
            Assert.Equal(1, runway.OpenEnds);

            Runway gentleTilesRunway = model.Runways["Base Runway - Lower Mushrooms Left Door (to Elevator)"];
            Assert.Equal(2, gentleTilesRunway.GentleDownTiles);
            Assert.Equal(4, gentleTilesRunway.GentleUpTiles);

            Runway endingUpTilesRunway = model.Runways["Base Runway - Lower Norfair Fireflea Room"];
            Assert.Equal(1, endingUpTilesRunway.EndingUpTiles);

            Runway startingDownTilesRunway = model.Runways["Runway with no Enemies - Purple Farming Room Door (to Purple Shaft)"];
            Assert.Equal(2, startingDownTilesRunway.StartingDownTiles);

            Runway noOpenEndRunway = model.Runways["Base Runway - Construction Zone Top Left Door (to Morph Ball)"];
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
            SuperMetroidModel model = ReusableModel();
            Runway runway = model.Runways["Base Runway - Construction Zone Top Left Door (to Morph Ball)"];
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = runway.Execute(model, inGameState, comingIn: true);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_NoUsableStrat_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Runway runway = model.Runways["Base Runway - Oasis Left Door (to West Sand Hall)"];
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = runway.Execute(model, inGameState, comingIn: true);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_Usable_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Runway runway = model.Runways["Base Runway - Oasis Left Door (to West Sand Hall)"];
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.GRAVITY_SUIT_NAME);

            // When
            ExecutionResult result = runway.Execute(model, inGameState, comingIn: true);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectRunwayUsed(runway.Name, "Base")
                .ExpectItemInvolved(SuperMetroidModel.GRAVITY_SUIT_NAME)
                .AssertRespectedBy(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items[SuperMetroidModel.VARIA_SUIT_NAME])
                )
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Runway freeRunway = model.GetNodeInRoom("Fast Pillars Setup Room", 2).Runways["Base Runway - Fast Pillars Setup Room Bottom Left Door (to Fast Rippers)"];
            Assert.True(freeRunway.LogicallyRelevant);
            Assert.True(freeRunway.LogicallyAlways);
            Assert.True(freeRunway.LogicallyFree);
            Assert.False(freeRunway.LogicallyNever);
            Assert.Equal(2 * (32M / 27M) + 10, freeRunway.LogicalEffectiveRunwayLength);
            Assert.Equal(2 * (32M / 27M) + 10, freeRunway.LogicalEffectiveReversibleRunwayLength);
            Assert.Equal(2 * (32M / 27M) + 10, freeRunway.LogicalEffectiveRunwayLengthNoCharge);

            Runway neverRunway = model.GetNodeInRoom("Oasis", 2).Runways["Base Runway - Oasis Right Door (to East Sand Hall)"];
            Assert.False(neverRunway.LogicallyRelevant);
            Assert.False(neverRunway.LogicallyAlways);
            Assert.False(neverRunway.LogicallyFree);
            Assert.True(neverRunway.LogicallyNever);
            Assert.Equal(12, neverRunway.LogicalEffectiveRunwayLength);
            Assert.Equal(12, neverRunway.LogicalEffectiveReversibleRunwayLength);
            Assert.Equal(12, neverRunway.LogicalEffectiveRunwayLengthNoCharge);

            Runway possibleRunway = model.GetNodeInRoom("Golden Torizo's Room", 2).Runways["Base Runway - Golden Torizo Room Right Door (to Screw Attack)"];
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
