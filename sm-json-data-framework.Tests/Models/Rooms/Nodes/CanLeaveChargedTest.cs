using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Requirements;

namespace sm_json_data_framework.Tests.Models.Rooms.Nodes
{
    public class CanLeaveChargedTest
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
            CanLeaveCharged canLeaveCharged = model.Rooms["Parlor and Alcatraz"].Nodes[1].CanLeaveCharged.First();
            Assert.Equal(0, canLeaveCharged.EndingUpTiles);
            Assert.Equal(0, canLeaveCharged.StartingDownTiles);
            Assert.Equal(25, canLeaveCharged.UsedTiles);
            Assert.Equal(1, canLeaveCharged.OpenEnds);
            Assert.Equal(0, canLeaveCharged.GentleUpTiles);
            Assert.Equal(0, canLeaveCharged.GentleDownTiles);
            Assert.Equal(3, canLeaveCharged.SteepUpTiles);
            Assert.Equal(3, canLeaveCharged.SteepDownTiles);

            Assert.Equal(0, canLeaveCharged.FramesRemaining);
            Assert.Equal(40, canLeaveCharged.ShinesparkFrames);
            Assert.True(canLeaveCharged.MustShinespark);
            Assert.Null(canLeaveCharged.InitiateRemotely);
            Assert.False(canLeaveCharged.IsInitiatedRemotely);

            Assert.Equal(1, canLeaveCharged.Strats.Count);
            Assert.Contains("Base", canLeaveCharged.Strats.Keys);
            Assert.Same(model.Rooms["Parlor and Alcatraz"].Nodes[1], canLeaveCharged.Node);


            CanLeaveCharged noSparkCanLeaveCharged = model.Rooms["Blue Brinstar Boulder Room"].Nodes[2].CanLeaveCharged.First();
            Assert.Equal(50, noSparkCanLeaveCharged.FramesRemaining);
            Assert.Equal(0, noSparkCanLeaveCharged.ShinesparkFrames);
            Assert.False(noSparkCanLeaveCharged.MustShinespark);
            Assert.Equal(0, noSparkCanLeaveCharged.SteepUpTiles);
            Assert.Equal(0, noSparkCanLeaveCharged.SteepDownTiles);


            CanLeaveCharged remoteCanLeaveCharged = model.Rooms["Early Supers Room"].Nodes[2].CanLeaveCharged.First();
            Assert.NotNull(remoteCanLeaveCharged.InitiateRemotely);
            Assert.True(remoteCanLeaveCharged.IsInitiatedRemotely);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_NoSpeedBooster_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanLeaveCharged canLeaveCharged = model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[9].CanLeaveCharged.First();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(canLeaveCharged.Node);

            // When
            ExecutionResult result = canLeaveCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_ShinesparkTechDisabled_RequiresShinespark_Fails()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech(SuperMetroidModel.SHINESPARK_TECH_NAME);
            model.ApplyLogicalOptions(logicalOptions);
            CanLeaveCharged canLeaveCharged = model.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom(canLeaveCharged.Node);

            // When
            ExecutionResult result = canLeaveCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_ShinesparkTechDisabled_DoesntNeedShineSpark_Succeeds()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech(SuperMetroidModel.SHINESPARK_TECH_NAME);
            model.ApplyLogicalOptions(logicalOptions);
            CanLeaveCharged canLeaveCharged = model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[9].CanLeaveCharged.First();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .ApplyAddItem(SuperMetroidModel.GRAVITY_SUIT_NAME)
                .ApplyEnterRoom(canLeaveCharged.Node);

            // When
            ExecutionResult result = canLeaveCharged.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectCanLeaveChargedExecuted(canLeaveCharged, "Base")
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_TooShortToShineCharge_Fails()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesSavedWithStutter = 0;
            logicalOptions.TilesToShineCharge = 19;
            model.ApplyLogicalOptions(logicalOptions);
            CanLeaveCharged canLeaveCharged = model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[9].CanLeaveCharged.First();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom(canLeaveCharged.Node);

            // When
            ExecutionResult result = canLeaveCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RequiresShinespark_RequiresAndSpendsCorrectEnergy()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            model.ApplyLogicalOptions(logicalOptions);
            CanLeaveCharged canLeaveCharged = model.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .ApplyAddItem(SuperMetroidModel.GRAVITY_SUIT_NAME)
                .ApplyEnterRoom(canLeaveCharged.Node);

            // When
            inGameState.ApplyConsumeResource(ConsumableResourceEnum.Energy, 51);
            ExecutionResult oneEnergyNotEnoughResult = canLeaveCharged.Execute(model, inGameState);
            inGameState.ApplyAddResource(RechargeableResourceEnum.RegularEnergy, 1);
            ExecutionResult exactlyEnoughEnergyResult = canLeaveCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(oneEnergyNotEnoughResult);

            new ExecutionResultValidator(model, inGameState)
                .ExpectCanLeaveChargedExecuted(canLeaveCharged, "Base")
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                // Starting point is 99-51+1 = 49, expected value 29
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -20)
                .AssertRespectedBy(exactlyEnoughEnergyResult);
        }

        [Fact]
        public void Execute_UnableToExecuteStrat_Fails()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            model.ApplyLogicalOptions(logicalOptions);
            CanLeaveCharged canLeaveCharged = model.Rooms["Blue Brinstar Boulder Room"].Nodes[2].CanLeaveCharged.First();
            InGameState inGameState = model .CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom(canLeaveCharged.Node);

            // When
            ExecutionResult result = canLeaveCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SpeedBoosterPossible_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Morph")
                .RegisterRemovedItem(SuperMetroidModel.GRAVITY_SUIT_NAME)
                .RegisterDisabledTech(SuperMetroidModel.SHINESPARK_TECH_NAME);
            logicalOptions.TilesSavedWithStutter = 0;
            logicalOptions.TilesToShineCharge = 19;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged neverByImpossibleRemote = model.Rooms["Warehouse Kihunter Room"].Nodes[3].CanLeaveCharged.First();
            Assert.False(neverByImpossibleRemote.LogicallyRelevant);
            Assert.False(neverByImpossibleRemote.LogicallyAlways);
            Assert.False(neverByImpossibleRemote.LogicallyFree);
            Assert.True(neverByImpossibleRemote.LogicallyNever);
            Assert.Equal(31.5M, neverByImpossibleRemote.LogicalEffectiveRunwayLength);
            Assert.Equal(0, neverByImpossibleRemote.TilesSavedWithStutter);
            Assert.Equal(19, neverByImpossibleRemote.TilesToShineCharge);
            Assert.False(neverByImpossibleRemote.CanShinespark);

            CanLeaveCharged neverByImpossibleStrat = model.Rooms["Mt. Everest"].Nodes[3].CanLeaveCharged.First();
            Assert.False(neverByImpossibleStrat.LogicallyRelevant);
            Assert.False(neverByImpossibleStrat.LogicallyAlways);
            Assert.False(neverByImpossibleStrat.LogicallyFree);
            Assert.True(neverByImpossibleStrat.LogicallyNever);
            Assert.Equal(20.5M, neverByImpossibleStrat.LogicalEffectiveRunwayLength);
            Assert.Equal(0, neverByImpossibleStrat.TilesSavedWithStutter);
            Assert.Equal(19, neverByImpossibleStrat.TilesToShineCharge);
            Assert.False(neverByImpossibleStrat.CanShinespark);

            CanLeaveCharged neverByImpossibleShinespark = model.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            Assert.False(neverByImpossibleShinespark.LogicallyRelevant);
            Assert.False(neverByImpossibleShinespark.LogicallyAlways);
            Assert.False(neverByImpossibleShinespark.LogicallyFree);
            Assert.True(neverByImpossibleShinespark.LogicallyNever);
            Assert.Equal(19M + 2 * 4M / 3M, neverByImpossibleShinespark.LogicalEffectiveRunwayLength);
            Assert.Equal(0, neverByImpossibleShinespark.TilesSavedWithStutter);
            Assert.Equal(19, neverByImpossibleShinespark.TilesToShineCharge);
            Assert.False(neverByImpossibleShinespark.CanShinespark);

            CanLeaveCharged neverByShortRunway = model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[9].CanLeaveCharged.First();
            Assert.False(neverByShortRunway.LogicallyRelevant);
            Assert.False(neverByShortRunway.LogicallyAlways);
            Assert.False(neverByShortRunway.LogicallyFree);
            Assert.True(neverByShortRunway.LogicallyNever);
            Assert.Equal(17.5M, neverByShortRunway.LogicalEffectiveRunwayLength);
            Assert.Equal(0, neverByShortRunway.TilesSavedWithStutter);
            Assert.Equal(19, neverByShortRunway.TilesToShineCharge);
            Assert.False(neverByShortRunway.CanShinespark);

            CanLeaveCharged notFreeBecauseSpeedNotFree = model.Rooms["Morph Ball Room"].Nodes[3].CanLeaveCharged.First();
            Assert.True(notFreeBecauseSpeedNotFree.LogicallyRelevant);
            Assert.False(notFreeBecauseSpeedNotFree.LogicallyAlways);
            Assert.False(notFreeBecauseSpeedNotFree.LogicallyFree);
            Assert.False(notFreeBecauseSpeedNotFree.LogicallyNever);
            Assert.Equal(30, notFreeBecauseSpeedNotFree.LogicalEffectiveRunwayLength);
            Assert.Equal(0, notFreeBecauseSpeedNotFree.TilesSavedWithStutter);
            Assert.Equal(19, notFreeBecauseSpeedNotFree.TilesToShineCharge);
            Assert.False(notFreeBecauseSpeedNotFree.CanShinespark);
        }

        [Fact]
        public void ApplyLogicalOptions_SpeedBoosterRemoved_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.SPEED_BOOSTER_NAME);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged neverByNoSpeedBooster = model.Rooms["Morph Ball Room"].Nodes[3].CanLeaveCharged.First();
            Assert.False(neverByNoSpeedBooster.LogicallyRelevant);
            Assert.False(neverByNoSpeedBooster.LogicallyAlways);
            Assert.False(neverByNoSpeedBooster.LogicallyFree);
            Assert.True(neverByNoSpeedBooster.LogicallyNever);
            Assert.Equal(30, neverByNoSpeedBooster.LogicalEffectiveRunwayLength);
        }

        [Fact]
        public void ApplyLogicalOptions_SpeedBoosterFree_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                    .ApplyAddItem(model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME])
                )
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged free = model.Rooms["Mt. Everest"].Nodes[3].CanLeaveCharged.First();
            Assert.True(free.LogicallyRelevant);
            Assert.True(free.LogicallyAlways);
            Assert.True(free.LogicallyFree);
            Assert.False(free.LogicallyNever);
            Assert.Equal(20.5M, free.LogicalEffectiveRunwayLength);

            // Expect
            CanLeaveCharged notFreeBecauseShinespark = model.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            Assert.True(notFreeBecauseShinespark.LogicallyRelevant);
            Assert.False(notFreeBecauseShinespark.LogicallyAlways);
            Assert.False(notFreeBecauseShinespark.LogicallyFree);
            Assert.False(notFreeBecauseShinespark.LogicallyNever);
            Assert.Equal(19M + 2 * 4M / 3M, notFreeBecauseShinespark.LogicalEffectiveRunwayLength);

            // Expect
            CanLeaveCharged notFreeBecauseStratNotFree = model.Rooms["Botwoon's Room"].Nodes[1].CanLeaveCharged.First();
            Assert.True(notFreeBecauseStratNotFree.LogicallyRelevant);
            Assert.False(notFreeBecauseStratNotFree.LogicallyAlways);
            Assert.False(notFreeBecauseStratNotFree.LogicallyFree);
            Assert.False(notFreeBecauseStratNotFree.LogicallyNever);
            Assert.Equal(16, notFreeBecauseStratNotFree.LogicalEffectiveRunwayLength);

            // Expect
            CanLeaveCharged notFreeBecauseRemoteNotFree = model.Rooms["Red Brinstar Fireflea Room"].Nodes[1].CanLeaveCharged.First();
            Assert.True(notFreeBecauseRemoteNotFree.LogicallyRelevant);
            Assert.False(notFreeBecauseRemoteNotFree.LogicallyAlways);
            Assert.False(notFreeBecauseRemoteNotFree.LogicallyFree);
            Assert.False(notFreeBecauseRemoteNotFree.LogicallyNever);
            Assert.Equal(13, notFreeBecauseRemoteNotFree.LogicalEffectiveRunwayLength);
        }

        [Fact]
        public void ApplyLogicalOptions_SpeedFree_LessPossibleEnergyThanSparkRequires_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            baseResouces.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 29);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(
                    ItemInventory.CreateVanillaStartingInventory(model)
                        .ApplyAddItem(model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                )
                .BaseResourceMaximums(baseResouces)
                .StartingResources(baseResouces)
                .Build();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(baseResouces)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["PowerBomb"], 10);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged canLeaveCharged = model.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            Assert.False(canLeaveCharged.LogicallyRelevant);
            Assert.True(canLeaveCharged.LogicallyNever);
            Assert.False(canLeaveCharged.LogicallyAlways);
            Assert.False(canLeaveCharged.LogicallyFree);
            Assert.Equal(LogicalOptions.DefaultTilesSavedWithStutter, canLeaveCharged.TilesSavedWithStutter);
            Assert.Equal(LogicalOptions.DefaultTilesToShineCharge, canLeaveCharged.TilesToShineCharge);
            Assert.True(canLeaveCharged.CanShinespark);

            CanLeaveCharged freeCanLeaveCharged = model.Rooms["Morph Ball Room"].Nodes[3].CanLeaveCharged.First();
            Assert.True(freeCanLeaveCharged.LogicallyRelevant);
            Assert.False(freeCanLeaveCharged.LogicallyNever);
            Assert.True(freeCanLeaveCharged.LogicallyAlways);
            Assert.True(freeCanLeaveCharged.LogicallyFree);
            Assert.Equal(LogicalOptions.DefaultTilesSavedWithStutter, freeCanLeaveCharged.TilesSavedWithStutter);
            Assert.Equal(LogicalOptions.DefaultTilesToShineCharge, freeCanLeaveCharged.TilesToShineCharge);
            Assert.True(freeCanLeaveCharged.CanShinespark);
        }

        [Fact]
        public void ApplyLogicalOptions_SpeedFree_NormalPossibleEnergy_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            baseResouces.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 29);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(
                    ItemInventory.CreateVanillaStartingInventory(model)
                        .ApplyAddItem(model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                )
                .BaseResourceMaximums(baseResouces)
                .StartingResources(baseResouces)
                .Build();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(baseResouces)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["PowerBomb"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ReserveTank"], 4);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged canLeaveCharged = model.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            Assert.True(canLeaveCharged.LogicallyRelevant);
            Assert.False(canLeaveCharged.LogicallyNever);
            Assert.False(canLeaveCharged.LogicallyAlways);
            Assert.False(canLeaveCharged.LogicallyFree);
            Assert.Equal(LogicalOptions.DefaultTilesSavedWithStutter, canLeaveCharged.TilesSavedWithStutter);
            Assert.Equal(LogicalOptions.DefaultTilesToShineCharge, canLeaveCharged.TilesToShineCharge);
            Assert.True(canLeaveCharged.CanShinespark);
        }

        #endregion
    }
}
