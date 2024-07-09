using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.SubObjects
{
    public class CanShineChargeTest
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
            CanShineCharge canShineCharge = model.Rooms["Mickey Mouse Room"].Links[7].To[8].Strats["Speed Through"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.Equal(22, canShineCharge.Length);
            Assert.Equal(0, canShineCharge.GentleUpTiles);
            Assert.Equal(0, canShineCharge.GentleDownTiles);
            Assert.Equal(0, canShineCharge.SteepUpTiles);
            Assert.Equal(0, canShineCharge.SteepDownTiles);
            Assert.Equal(0, canShineCharge.EndingUpTiles);
            Assert.Equal(0, canShineCharge.StartingDownTiles);
            Assert.Equal(0, canShineCharge.OpenEnds);
            Assert.Equal(0, canShineCharge.ShinesparkFrames);
            Assert.False(canShineCharge.MustShinespark);

            CanShineCharge canShineChargeSteepTiles = model.Rooms["Landing Site"].Links[4].To[1].Strats["Shinespark"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.Equal(2, canShineChargeSteepTiles.SteepUpTiles);
            Assert.Equal(1, canShineChargeSteepTiles.SteepDownTiles);
            Assert.Equal(2, canShineChargeSteepTiles.OpenEnds);
            Assert.Equal(125, canShineChargeSteepTiles.ShinesparkFrames);
            Assert.True(canShineChargeSteepTiles.MustShinespark);

            CanShineCharge canShineChargeGentleTiles = model.Rooms["Double Chamber"].Links[2].To[1].Strats["Charge Shinespark"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.Equal(3, canShineChargeGentleTiles.GentleUpTiles);
            Assert.Equal(3, canShineChargeGentleTiles.GentleDownTiles);

            CanShineCharge canShineChargeGentleTilesAlt = model.Rooms["Lower Norfair Spring Ball Maze Room"].Links[1].To[5].Strats["Hotarubi Special"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.Equal(0, canShineChargeGentleTilesAlt.GentleUpTiles);
            Assert.Equal(2, canShineChargeGentleTilesAlt.GentleDownTiles);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_SimpleNoSpark_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanShineCharge canShineCharge= model.Rooms["Parlor and Alcatraz"].Links[8].To[1].Strats["Parlor Quick Charge"].Requires.LogicalElement<CanShineCharge>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME);

            // When
            ExecutionResult result = canShineCharge.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_NoSpeedBooster_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanShineCharge canShineCharge = model.Rooms["Parlor and Alcatraz"].Links[8].To[1].Strats["Parlor Quick Charge"].Requires.LogicalElement<CanShineCharge>(0);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = canShineCharge.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RunwayLogicallyTooShort_Fails()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 30;
            model.ApplyLogicalOptions(logicalOptions);
            CanShineCharge canShineCharge = model.Rooms["Parlor and Alcatraz"].Links[8].To[1].Strats["Parlor Quick Charge"].Requires.LogicalElement<CanShineCharge>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME);

            // When
            ExecutionResult result = canShineCharge.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_ShinesparkTechTurnedOffButNoSparkNeeded_Succeeds()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledTech("canShinespark");
            model.ApplyLogicalOptions(logicalOptions);
            CanShineCharge canShineCharge = model.Rooms["Parlor and Alcatraz"].Links[8].To[1].Strats["Parlor Quick Charge"].Requires.LogicalElement<CanShineCharge>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME);

            // When
            ExecutionResult result = canShineCharge.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_RequiresShinespark_ConsumesEnergy()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanShineCharge canShineCharge = model.Rooms["Landing Site"].Links[7].To[1].Strats["Shinespark"].Requires.LogicalElement<CanShineCharge>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME);

            // When
            ExecutionResult result = canShineCharge.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -40)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_RequiresShinespark_TechTurnedOff_Fails()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledTech("canShinespark");
            model.ApplyLogicalOptions(logicalOptions);
            CanShineCharge canShineCharge = model.Rooms["Landing Site"].Links[7].To[1].Strats["Shinespark"].Requires.LogicalElement<CanShineCharge>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME);

            // When
            ExecutionResult result = canShineCharge.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RequiresShinespark_NotEnoughEnergy_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanShineCharge canShineCharge = model.Rooms["Landing Site"].Links[7].To[1].Strats["Shinespark"].Requires.LogicalElement<CanShineCharge>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 31)
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME);

            // When
            ExecutionResult result = canShineCharge.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RequiresShinespark_HasExcessFrames_NotEnoughEnergy_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanShineCharge canShineCharge = model.Rooms["Landing Site"].Links[4].To[1].Strats["Shinespark"].Requires
                .LogicalElement<CanShineCharge>(0, canShineCharge => canShineCharge.ShinesparkFrames == 125 && canShineCharge.ExcessShinesparkFrames == 33);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 79)
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME);

            // When
            ExecutionResult result = canShineCharge.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RequiresShinespark_HasExcessFrames_BarelyEnoughEnergy_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanShineCharge canShineCharge = model.Rooms["Landing Site"].Links[4].To[1].Strats["Shinespark"].Requires
                .LogicalElement<CanShineCharge>(0, canShineCharge => canShineCharge.ShinesparkFrames == 125 && canShineCharge.ExcessShinesparkFrames == 33);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 78)
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME);

            // When
            ExecutionResult result = canShineCharge.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -92)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_RequiresShinespark_HasExcessFrames_EnoughEnergyForSomeExcessFrames_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanShineCharge canShineCharge = model.Rooms["Landing Site"].Links[4].To[1].Strats["Shinespark"].Requires
                .LogicalElement<CanShineCharge>(0, canShineCharge => canShineCharge.ShinesparkFrames == 125 && canShineCharge.ExcessShinesparkFrames == 33);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 68)
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME);

            // When
            ExecutionResult result = canShineCharge.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -102)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_RequiresShinespark_HasExcessFrames_EnoughEnergyForMoreThanExcessFrames_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanShineCharge canShineCharge = model.Rooms["Landing Site"].Links[4].To[1].Strats["Shinespark"].Requires
                .LogicalElement<CanShineCharge>(0, canShineCharge => canShineCharge.ShinesparkFrames == 125 && canShineCharge.ExcessShinesparkFrames == 33);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME);

            // When
            ExecutionResult result = canShineCharge.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -125)
                .AssertRespectedBy(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_LessPossibleEnergyThanBestCaseDamage_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .RegisterRemovedItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
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
            CanShineCharge canShineCharge = model.Rooms["West Ocean"].Links[13].To[5].Strats["Gravity Suit and Shinespark"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(canShineCharge.LogicallyRelevant);
            Assert.True(canShineCharge.LogicallyNever);
            Assert.False(canShineCharge.LogicallyAlways);
            Assert.False(canShineCharge.LogicallyFree);

            CanShineCharge noSparkCanShineCharge = model.Rooms["Parlor and Alcatraz"].Links[8].To[1].Strats["Parlor Quick Charge"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(noSparkCanShineCharge.LogicallyRelevant);
            Assert.False(noSparkCanShineCharge.LogicallyNever);
            Assert.True(noSparkCanShineCharge.LogicallyAlways);
            Assert.True(noSparkCanShineCharge.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_NormalPossibleEnergy_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .RegisterRemovedItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
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
            CanShineCharge canShineCharge = model.Rooms["West Ocean"].Links[13].To[5].Strats["Gravity Suit and Shinespark"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(canShineCharge.LogicallyRelevant);
            Assert.False(canShineCharge.LogicallyNever);
            Assert.False(canShineCharge.LogicallyAlways);
            Assert.False(canShineCharge.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_WithExcessShinesparkFrames_LessPossibleEnergyThanBestCaseDamage_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            baseResouces.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 120);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
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
            CanShineCharge canShineCharge = model.Rooms["Landing Site"].Links[4].To[1].Strats["Shinespark"].Requires
                .LogicalElement<CanShineCharge>(0, canShineCharge => canShineCharge.ShinesparkFrames == 125 && canShineCharge.ExcessShinesparkFrames == 33);
            Assert.True(canShineCharge.LogicallyRelevant);
            Assert.True(canShineCharge.LogicallyNever);
            Assert.False(canShineCharge.LogicallyAlways);
            Assert.False(canShineCharge.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_WithExcessShinesparkFrames_JustEnoughPossibleEnergyForBestCaseDamage_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            baseResouces.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 121);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
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
            CanShineCharge canShineCharge = model.Rooms["Landing Site"].Links[4].To[1].Strats["Shinespark"].Requires
                .LogicalElement<CanShineCharge>(0, canShineCharge => canShineCharge.ShinesparkFrames == 125 && canShineCharge.ExcessShinesparkFrames == 33);
            Assert.True(canShineCharge.LogicallyRelevant);
            Assert.False(canShineCharge.LogicallyNever);
            Assert.False(canShineCharge.LogicallyAlways);
            Assert.False(canShineCharge.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_BothSuitsFree_SetsLogicalProperties()
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
                        .ApplyAddItem(model.Items[SuperMetroidModel.VARIA_SUIT_NAME])
                        .ApplyAddItem(model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME])
                )
                .BaseResourceMaximums(baseResouces)
                .StartingResources(baseResouces)
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanShineCharge canShineCharge = model.Rooms["West Ocean"].Links[13].To[5].Strats["Gravity Suit and Shinespark"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(canShineCharge.LogicallyRelevant);
            Assert.False(canShineCharge.LogicallyNever);
            Assert.False(canShineCharge.LogicallyAlways);
            Assert.False(canShineCharge.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_NoSpeedBooster_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.SPEED_BOOSTER_NAME);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanShineCharge canShineCharge = model.Rooms["Parlor and Alcatraz"].Links[8].To[1].Strats["Parlor Quick Charge"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(canShineCharge.LogicallyRelevant);
            Assert.True(canShineCharge.LogicallyNever);
            Assert.False(canShineCharge.LogicallyAlways);
            Assert.False(canShineCharge.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_FreeSpeedNoShinespark_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canShinespark");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                )
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanShineCharge noSparkCanShineCharge = model.Rooms["Parlor and Alcatraz"].Links[8].To[1].Strats["Parlor Quick Charge"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(noSparkCanShineCharge.LogicallyRelevant);
            Assert.False(noSparkCanShineCharge.LogicallyNever);
            Assert.True(noSparkCanShineCharge.LogicallyAlways);
            Assert.True(noSparkCanShineCharge.LogicallyFree);

            CanShineCharge canShineCharge = model.Rooms["Landing Site"].Links[7].To[1].Strats["Shinespark"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(canShineCharge.LogicallyRelevant);
            Assert.True(canShineCharge.LogicallyNever);
            Assert.False(canShineCharge.LogicallyAlways);
            Assert.False(canShineCharge.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_NeedsMoreTilesToCharge_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                )
                .Build();
            logicalOptions.ShineChargesWithStutter = false;
            logicalOptions.TilesToShineCharge = 35;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            CanShineCharge canShineCharge = model.Rooms["Parlor and Alcatraz"].Links[8].To[1].Strats["Parlor Quick Charge"].Requires.LogicalElement<CanShineCharge>(0);
            Assert.True(canShineCharge.LogicallyRelevant);
            Assert.True(canShineCharge.LogicallyNever);
            Assert.False(canShineCharge.LogicallyAlways);
            Assert.False(canShineCharge.LogicallyFree);
        }



        #endregion
    }
}
