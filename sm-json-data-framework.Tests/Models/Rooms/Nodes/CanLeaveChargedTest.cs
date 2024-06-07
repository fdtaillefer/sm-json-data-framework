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
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            CanLeaveCharged canLeaveCharged = Model.Rooms["Parlor and Alcatraz"].Nodes[1].CanLeaveCharged.First();
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
            Assert.Same(Model.Rooms["Parlor and Alcatraz"].Nodes[1], canLeaveCharged.Node);


            CanLeaveCharged noSparkCanLeaveCharged = Model.Rooms["Blue Brinstar Boulder Room"].Nodes[2].CanLeaveCharged.First();
            Assert.Equal(50, noSparkCanLeaveCharged.FramesRemaining);
            Assert.Equal(0, noSparkCanLeaveCharged.ShinesparkFrames);
            Assert.False(noSparkCanLeaveCharged.MustShinespark);
            Assert.Equal(0, noSparkCanLeaveCharged.SteepUpTiles);
            Assert.Equal(0, noSparkCanLeaveCharged.SteepDownTiles);


            CanLeaveCharged remoteCanLeaveCharged = Model.Rooms["Early Supers Room"].Nodes[2].CanLeaveCharged.First();
            Assert.NotNull(remoteCanLeaveCharged.InitiateRemotely);
            Assert.True(remoteCanLeaveCharged.IsInitiatedRemotely);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_NoSpeedBooster_Fails()
        {
            // Given
            CanLeaveCharged canLeaveCharged = Model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[9].CanLeaveCharged.First();
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(canLeaveCharged.Node);

            // When
            ExecutionResult result = canLeaveCharged.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_ShinesparkTechDisabled_RequiresShinespark_Fails()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canShinespark");
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);
            CanLeaveCharged canLeaveCharged = ModelWithOptions.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            InGameState inGameState = ModelWithOptions.CreateInitialGameState()
                .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                .ApplyEnterRoom(canLeaveCharged.Node);

            // When
            ExecutionResult result = canLeaveCharged.Execute(ModelWithOptions, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_ShinesparkTechDisabled_DoesntNeedShineSpark_Succeeds()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canShinespark");
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);
            CanLeaveCharged canLeaveCharged = ModelWithOptions.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[9].CanLeaveCharged.First();
            InGameState inGameState = ModelWithOptions.CreateInitialGameState()
                .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.VARIA_SUIT_NAME])
                .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.GRAVITY_SUIT_NAME])
                .ApplyEnterRoom(canLeaveCharged.Node);

            // When
            ExecutionResult result = canLeaveCharged.Execute(ModelWithOptions, inGameState);

            // Expect
            Assert.NotNull(result);

            Assert.Single(result.CanLeaveChargedExecuted);
            Assert.Same(canLeaveCharged, result.CanLeaveChargedExecuted.First().canLeaveChargedUsed);
            Assert.Same(canLeaveCharged.Strats["Base"], result.CanLeaveChargedExecuted.First().stratUsed);

            Assert.Single(result.ItemsInvolved);
            Assert.Same(ModelWithOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME], result.ItemsInvolved[SuperMetroidModel.SPEED_BOOSTER_NAME]);

            Assert.Empty(result.ActivatedGameFlags);
            Assert.Empty(result.BypassedLocks);
            Assert.Empty(result.DamageReducingItemsInvolved);
            Assert.Empty(result.KilledEnemies);
            Assert.Empty(result.OpenedLocks);
            Assert.Empty(result.RunwaysUsed);

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
        public void Execute_TooShortToShineCharge_Fails()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesSavedWithStutter = 0;
            logicalOptions.TilesToShineCharge = 19;
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);
            CanLeaveCharged canLeaveCharged = ModelWithOptions.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[9].CanLeaveCharged.First();
            InGameState inGameState = ModelWithOptions.CreateInitialGameState()
                .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                .ApplyEnterRoom(canLeaveCharged.Node);

            // When
            ExecutionResult result = canLeaveCharged.Execute(ModelWithOptions, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RequiresShinespark_RequiresAndSpendsCorrectEnergy()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);
            CanLeaveCharged canLeaveCharged = ModelWithOptions.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            InGameState inGameState = ModelWithOptions.CreateInitialGameState()
                .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.VARIA_SUIT_NAME])
                .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.GRAVITY_SUIT_NAME])
                .ApplyEnterRoom(canLeaveCharged.Node);

            // When
            inGameState.ApplyConsumeResource(ConsumableResourceEnum.Energy, 51);
            ExecutionResult oneEnergyNotEnoughResult = canLeaveCharged.Execute(ModelWithOptions, inGameState);
            inGameState.ApplyAddResource(RechargeableResourceEnum.RegularEnergy, 1);
            ExecutionResult exactlyEnoughEnergyResult = canLeaveCharged.Execute(ModelWithOptions, inGameState);

            // Expect
            Assert.Null(oneEnergyNotEnoughResult);

            Assert.NotNull(exactlyEnoughEnergyResult);

            Assert.Single(exactlyEnoughEnergyResult.CanLeaveChargedExecuted);
            Assert.Same(canLeaveCharged, exactlyEnoughEnergyResult.CanLeaveChargedExecuted.First().canLeaveChargedUsed);
            Assert.Same(canLeaveCharged.Strats["Base"], exactlyEnoughEnergyResult.CanLeaveChargedExecuted.First().stratUsed);

            Assert.Single(exactlyEnoughEnergyResult.ItemsInvolved);
            Assert.Same(ModelWithOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME], exactlyEnoughEnergyResult.ItemsInvolved[SuperMetroidModel.SPEED_BOOSTER_NAME]);

            Assert.Empty(exactlyEnoughEnergyResult.ActivatedGameFlags);
            Assert.Empty(exactlyEnoughEnergyResult.BypassedLocks);
            Assert.Empty(exactlyEnoughEnergyResult.DamageReducingItemsInvolved);
            Assert.Empty(exactlyEnoughEnergyResult.KilledEnemies);
            Assert.Empty(exactlyEnoughEnergyResult.OpenedLocks);
            Assert.Empty(exactlyEnoughEnergyResult.RunwaysUsed);

            Assert.Equal(29, exactlyEnoughEnergyResult.ResultingState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(inGameState.ResourceMaximums, exactlyEnoughEnergyResult.ResultingState.ResourceMaximums);
            Assert.Equal(inGameState.OpenedLocks.Count, exactlyEnoughEnergyResult.ResultingState.OpenedLocks.Count);
            Assert.Equal(inGameState.ActiveGameFlags.Count, exactlyEnoughEnergyResult.ResultingState.ActiveGameFlags.Count);
            Assert.Equal(inGameState.TakenItemLocations.Count, exactlyEnoughEnergyResult.ResultingState.TakenItemLocations.Count);
            Assert.Same(inGameState.CurrentRoom, exactlyEnoughEnergyResult.ResultingState.CurrentRoom);
            Assert.Same(inGameState.CurrentNode, exactlyEnoughEnergyResult.ResultingState.CurrentNode);
            Assert.True(exactlyEnoughEnergyResult.ResultingState.Inventory.ExceptIn(inGameState.Inventory).Empty);
        }

        [Fact]
        public void Execute_UnableToExecuteStrat_Fails()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);
            CanLeaveCharged canLeaveCharged = ModelWithOptions.Rooms["Blue Brinstar Boulder Room"].Nodes[2].CanLeaveCharged.First();
            InGameState inGameState = ModelWithOptions.CreateInitialGameState()
                .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                .ApplyEnterRoom(canLeaveCharged.Node);

            // When
            ExecutionResult result = canLeaveCharged.Execute(ModelWithOptions, inGameState);

            // Expect
            Assert.Null(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SpeedBoosterPossible_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Morph")
                .RegisterRemovedItem("Gravity")
                .RegisterDisabledTech("canShinespark");
            logicalOptions.TilesSavedWithStutter = 0;
            logicalOptions.TilesToShineCharge = 19;

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged neverByImpossibleRemote = ModelWithOptions.Rooms["Warehouse Kihunter Room"].Nodes[3].CanLeaveCharged.First();
            Assert.False(neverByImpossibleRemote.LogicallyRelevant);
            Assert.False(neverByImpossibleRemote.LogicallyAlways);
            Assert.False(neverByImpossibleRemote.LogicallyFree);
            Assert.True(neverByImpossibleRemote.LogicallyNever);
            Assert.Equal(31.5M, neverByImpossibleRemote.LogicalEffectiveRunwayLength);
            Assert.Equal(0, neverByImpossibleRemote.TilesSavedWithStutter);
            Assert.Equal(19, neverByImpossibleRemote.TilesToShineCharge);
            Assert.False(neverByImpossibleRemote.CanShinespark);

            CanLeaveCharged neverByImpossibleStrat = ModelWithOptions.Rooms["Mt. Everest"].Nodes[3].CanLeaveCharged.First();
            Assert.False(neverByImpossibleStrat.LogicallyRelevant);
            Assert.False(neverByImpossibleStrat.LogicallyAlways);
            Assert.False(neverByImpossibleStrat.LogicallyFree);
            Assert.True(neverByImpossibleStrat.LogicallyNever);
            Assert.Equal(20.5M, neverByImpossibleStrat.LogicalEffectiveRunwayLength);
            Assert.Equal(0, neverByImpossibleStrat.TilesSavedWithStutter);
            Assert.Equal(19, neverByImpossibleStrat.TilesToShineCharge);
            Assert.False(neverByImpossibleStrat.CanShinespark);

            CanLeaveCharged neverByImpossibleShinespark = ModelWithOptions.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            Assert.False(neverByImpossibleShinespark.LogicallyRelevant);
            Assert.False(neverByImpossibleShinespark.LogicallyAlways);
            Assert.False(neverByImpossibleShinespark.LogicallyFree);
            Assert.True(neverByImpossibleShinespark.LogicallyNever);
            Assert.Equal(19M + 2 * 4M / 3M, neverByImpossibleShinespark.LogicalEffectiveRunwayLength);
            Assert.Equal(0, neverByImpossibleShinespark.TilesSavedWithStutter);
            Assert.Equal(19, neverByImpossibleShinespark.TilesToShineCharge);
            Assert.False(neverByImpossibleShinespark.CanShinespark);

            CanLeaveCharged neverByShortRunway = ModelWithOptions.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[9].CanLeaveCharged.First();
            Assert.False(neverByShortRunway.LogicallyRelevant);
            Assert.False(neverByShortRunway.LogicallyAlways);
            Assert.False(neverByShortRunway.LogicallyFree);
            Assert.True(neverByShortRunway.LogicallyNever);
            Assert.Equal(17.5M, neverByShortRunway.LogicalEffectiveRunwayLength);
            Assert.Equal(0, neverByShortRunway.TilesSavedWithStutter);
            Assert.Equal(19, neverByShortRunway.TilesToShineCharge);
            Assert.False(neverByShortRunway.CanShinespark);

            CanLeaveCharged notFreeBecauseSpeedNotFree = ModelWithOptions.Rooms["Morph Ball Room"].Nodes[3].CanLeaveCharged.First();
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
        public void ApplyLogicalOptions_SpeedFree_LessPossibleEnergyThanSparkRequires_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            baseResouces.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 29);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions)
                .StartingInventory(
                    ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                        .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                )
                .BaseResourceMaximums(baseResouces)
                .StartingResources(baseResouces)
                .Build();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(baseResouces)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["PowerBomb"], 10);

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged canLeaveCharged = ModelWithOptions.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
            Assert.False(canLeaveCharged.LogicallyRelevant);
            Assert.True(canLeaveCharged.LogicallyNever);
            Assert.False(canLeaveCharged.LogicallyAlways);
            Assert.False(canLeaveCharged.LogicallyFree);
            Assert.Equal(LogicalOptions.DefaultTilesSavedWithStutter, canLeaveCharged.TilesSavedWithStutter);
            Assert.Equal(LogicalOptions.DefaultTilesToShineCharge, canLeaveCharged.TilesToShineCharge);
            Assert.True(canLeaveCharged.CanShinespark);

            CanLeaveCharged freeCanLeaveCharged = ModelWithOptions.Rooms["Morph Ball Room"].Nodes[3].CanLeaveCharged.First();
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
            LogicalOptions logicalOptions = new LogicalOptions();
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            baseResouces.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 29);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions)
                .StartingInventory(
                    ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                        .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                )
                .BaseResourceMaximums(baseResouces)
                .StartingResources(baseResouces)
                .Build();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(baseResouces)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["PowerBomb"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ReserveTank"], 4);

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanLeaveCharged canLeaveCharged = ModelWithOptions.Rooms["Spore Spawn Farming Room"].Nodes[1].CanLeaveCharged.First();
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
