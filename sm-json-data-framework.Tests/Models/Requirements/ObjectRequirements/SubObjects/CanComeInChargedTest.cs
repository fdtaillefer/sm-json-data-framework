using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Tests.TestTools;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Rooms;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.SubObjects
{
    public class CanComeInChargedTest
    {
        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel NewModelForOptions() => StaticTestObjects.UnfinalizedModel.Finalize();
        private static UnfinalizedSuperMetroidModel UnfinalizedModelForModification() => new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation
            SuperMetroidModel model = ReusableModel();

            // Expect
            CanComeInCharged canComeInCharged = model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Links[10].To[9].Strats["Shinespark"]
                .Requires.LogicalElement<CanComeInCharged>(0);
            Assert.Same(model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[10], canComeInCharged.FromNode);
            Assert.Empty(canComeInCharged.InRoomPath);
            Assert.Equal(30, canComeInCharged.FramesRemaining);
            Assert.Equal(75, canComeInCharged.ShinesparkFrames);
            Assert.True(canComeInCharged.MustShinespark);

            CanComeInCharged canComeInChargedWithInRoomPath = model.Rooms["Parlor and Alcatraz"].Links[8].To[1].Strats["Parlor Shinespark"]
                .Requires.LogicalElement<CanComeInCharged>(0);
            Assert.Equal(2, canComeInChargedWithInRoomPath.InRoomPath.Count);
            Assert.Equal(4, canComeInChargedWithInRoomPath.InRoomPath[0]);
            Assert.Equal(8, canComeInChargedWithInRoomPath.InRoomPath[1]);

            CanComeInCharged canComeInChargedNoSpark = model.Rooms["Parlor and Alcatraz"].Links[1].To[8].Strats["SpeedBooster"]
                .Requires.LogicalElement<CanComeInCharged>(0);
            Assert.Equal(0, canComeInChargedNoSpark.ShinesparkFrames);
            Assert.False(canComeInChargedNoSpark.MustShinespark);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_NoCanLeaveChargedOrGoodRunway_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanComeInCharged canComeInCharged = model.Rooms["Pants Room"].Links[4].To[2].Strats["Pants Room Shinespark"]
                .Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.FromNode.Id == 1);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom("East Sand Hall", 2)
                .ApplyEnterRoom("Pants Room", 1)
                .ApplyVisitNode(4, "Suitless");

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_CanlLeaveChargedAvailableButRequirementsNotMet_Fails()
        {
            // The vanilla model currently contains no case where a CanLeaveCharged with non-free requirements
            // is needed to fulfill a CanComeInCharged, so we have to add a requirement manually...

            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = UnfinalizedModelForModification();
            UnfinalizedItemLogicalElement extraRequirement = new UnfinalizedItemLogicalElement(unfinalizedModel.Items["Morph"]);
            unfinalizedModel.Rooms["Crocomire's Room"].Nodes[1].CanLeaveCharged.First().Strats["Base"].Requires.LogicalElements.Add(extraRequirement);
            SuperMetroidModel model = new SuperMetroidModel(unfinalizedModel);

            CanComeInCharged canComeInCharged = model.Rooms["Post Crocomire Farming Room"].Links[5].To[1].Strats["Shinespark"]
                .Requires.LogicalElement<Or>(0)
                .LogicalRequirements.LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.FromNode.Id == 4);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom("Crocomire's Room", 1)
                .ApplyEnterRoom("Post Crocomire Farming Room", 4)
                .ApplyVisitNode(5, "Base");

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_CanLeaveChargedAvailableRequirementsMet_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanComeInCharged canComeInCharged = model.Rooms["Post Crocomire Farming Room"].Links[5].To[1].Strats["Shinespark"]
                .Requires.LogicalElement<Or>(0)
                .LogicalRequirements.LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.FromNode.Id == 4);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom("Crocomire's Room", 1)
                .ApplyEnterRoom("Post Crocomire Farming Room", 4)
                .ApplyVisitNode(5, "Base");

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectCanLeaveChargedExecuted(model.Rooms["Crocomire's Room"].Nodes[1].CanLeaveCharged.First(), "Base")
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -45)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_CanLeaveChargedWithNotEnoughFramesRemaining_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanComeInCharged canComeInCharged = model.Rooms["Botwoon Energy Tank Room"].Links[1].To[4].Strats["Suitless Spark"]
                .Obstacles["A"].Requires.LogicalElement<CanComeInCharged>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyAddItem(SuperMetroidModel.GRAVITY_SUIT_NAME)
                .ApplyEnterRoom("Botwoon's Room", 2)
                .ApplyEnterRoom("Botwoon Energy Tank Room", 1);

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_CanLeaveChargedTooShortForLogic_Fails()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 60;
            model.ApplyLogicalOptions(logicalOptions);
            CanComeInCharged canComeInCharged = model.Rooms["Post Crocomire Farming Room"].Links[5].To[1].Strats["Shinespark"]
                .Requires.LogicalElement<Or>(0)
                .LogicalRequirements.LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.FromNode.Id == 4);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom("Crocomire's Room", 1)
                .ApplyEnterRoom("Post Crocomire Farming Room", 4)
                .ApplyVisitNode(5, "Base");

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_CanLeaveChargedButNotEnoughEnergyForIt_Fails()
        {
            // The vanilla model currently contains no case where a CanComeInChaarged can only be fulfilled by a non-remote CanLeaveCharged with a shinespark
            // (much less one that costs less energy than the CanComeInCharged), so we have to add a CanLeaveCharged manually...

            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = UnfinalizedModelForModification();
            UnfinalizedCanLeaveCharged newCanLeaveCharged = new UnfinalizedCanLeaveCharged();
            newCanLeaveCharged.ShinesparkFrames = 100;
            newCanLeaveCharged.Node = unfinalizedModel.Rooms["Electric Death Room"].Nodes[1];
            newCanLeaveCharged.UsedTiles = 20;
            newCanLeaveCharged.FramesRemaining = 0;
            UnfinalizedStrat newStrat = new UnfinalizedStrat();
            newStrat.Name = "Base";
            newStrat.Requires = new UnfinalizedLogicalRequirements();
            newCanLeaveCharged.Strats.Add("Base", newStrat);
            unfinalizedModel.Rooms["Electric Death Room"].Nodes[1].CanLeaveCharged.Add(newCanLeaveCharged);

            SuperMetroidModel model = unfinalizedModel.Finalize();
            CanComeInCharged canComeInCharged = model.Rooms["Wrecked Ship Energy Tank Room"].Links[1].To[2].Strats["Shinespark"]
                .Requires.LogicalElement<CanComeInCharged>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 71)
                .ApplyEnterRoom("Electric Death Room", 1)
                .ApplyEnterRoom("Wrecked Ship Energy Tank Room", 1);

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_MissingSpeedBooster_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanComeInCharged canComeInCharged = model.Rooms["Parlor and Alcatraz"].Links[1].To[8].Strats["SpeedBooster"]
                .Requires.LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.FromNode.Id == 1);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Terminator Room", 2)
                .ApplyEnterRoom("Parlor and Alcatraz", 1);

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_CanLeaveChargedRequiresShinespark_ShinesparkTechDisabled_Fails()
        {
            // The vanilla model currently contains no case where a CanComeInChaarged can only be fulfilled by a non-remote CanLeaveCharged with a shinespark,
            // so we have to add a CanLeaveCharged manually...

            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = UnfinalizedModelForModification();
            UnfinalizedCanLeaveCharged newCanLeaveCharged = new UnfinalizedCanLeaveCharged();
            newCanLeaveCharged.ShinesparkFrames = 100;
            newCanLeaveCharged.Node = unfinalizedModel.Rooms["Electric Death Room"].Nodes[1];
            newCanLeaveCharged.UsedTiles = 20;
            newCanLeaveCharged.FramesRemaining = 0;
            UnfinalizedStrat newStrat = new UnfinalizedStrat();
            newStrat.Name = "Base";
            newStrat.Requires = new UnfinalizedLogicalRequirements();
            newCanLeaveCharged.Strats.Add("Base", newStrat);
            unfinalizedModel.Rooms["Electric Death Room"].Nodes[1].CanLeaveCharged.Add(newCanLeaveCharged);
            // While we're at it, remove shinespark frames from the CanComeInCharged to really home in on the test case
            unfinalizedModel.Rooms["Wrecked Ship Energy Tank Room"].Links[1].To[2].Strats["Shinespark"]
                .Requires.LogicalElement<UnfinalizedCanComeInCharged>(0).ShinesparkFrames = 0;

            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canShinespark");
            SuperMetroidModel model = unfinalizedModel.Finalize(logicalOptions);
            CanComeInCharged canComeInCharged = model.Rooms["Wrecked Ship Energy Tank Room"].Links[1].To[2].Strats["Shinespark"]
                .Requires.LogicalElement<CanComeInCharged>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources()
                .ApplyEnterRoom("Electric Death Room", 1)
                .ApplyEnterRoom("Wrecked Ship Energy Tank Room", 1);

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RequiresShinespark_ShinesparkTechDisabled_Fails()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canShinespark");
            model.ApplyLogicalOptions(logicalOptions);
            CanComeInCharged canComeInCharged = model.Rooms["Post Crocomire Farming Room"].Links[5].To[1].Strats["Shinespark"]
                .Requires.LogicalElement<Or>(0)
                .LogicalRequirements.LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.FromNode.Id == 4);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom("Crocomire's Room", 1)
                .ApplyEnterRoom("Post Crocomire Farming Room", 4)
                .ApplyVisitNode(5, "Base");

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RequiresShinespark_NotEnoughEnergy_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanComeInCharged canComeInCharged = model.Rooms["Post Crocomire Farming Room"].Links[5].To[1].Strats["Shinespark"]
                .Requires.LogicalElement<Or>(0)
                .LogicalRequirements.LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.FromNode.Id == 4);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 46)
                .ApplyEnterRoom("Crocomire's Room", 1)
                .ApplyEnterRoom("Post Crocomire Farming Room", 4)
                .ApplyVisitNode(5, "Base");

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RequiresShinespark_HasExcessFrames_NotEnoughEnergy_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanComeInCharged canComeInCharged = model.Rooms["Landing Site"].Links[3].To[1].Strats["Shinespark"].Requires
                .LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.ShinesparkFrames == 125 && canComeInCharged.ExcessShinesparkFrames == 33);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 79)
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom("Landing Site", 3);

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RequiresShinespark_HasExcessFrames_BarelyEnoughEnergy_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanComeInCharged canComeInCharged = model.Rooms["Landing Site"].Links[3].To[1].Strats["Shinespark"].Requires
                .LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.ShinesparkFrames == 125 && canComeInCharged.ExcessShinesparkFrames == 33);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 78)
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom("Landing Site", 3);

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ExpectRunwayUsed("Base Runway - Landing Site Top Right Door (to Power Bombs)", "Base")
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -92)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_RequiresShinespark_HasExcessFrames_EnoughEnergyForSomeExcessFrames_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanComeInCharged canComeInCharged = model.Rooms["Landing Site"].Links[3].To[1].Strats["Shinespark"].Requires
                .LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.ShinesparkFrames == 125 && canComeInCharged.ExcessShinesparkFrames == 33);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 68)
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom("Landing Site", 3);

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ExpectRunwayUsed("Base Runway - Landing Site Top Right Door (to Power Bombs)", "Base")
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -102)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_RequiresShinespark_HasExcessFrames_EnoughEnergyForMoreThanExcessFrames_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanComeInCharged canComeInCharged = model.Rooms["Landing Site"].Links[3].To[1].Strats["Shinespark"].Requires
                .LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.ShinesparkFrames == 125 && canComeInCharged.ExcessShinesparkFrames == 33);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom("Landing Site", 3);

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ExpectRunwayUsed("Base Runway - Landing Site Top Right Door (to Power Bombs)", "Base")
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -125)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_UsesOnlyInRoomRunway_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanComeInCharged canComeInCharged = model.Rooms["Parlor and Alcatraz"].Links[1].To[8].Strats["SpeedBooster"]
                .Requires.LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.FromNode.Id == 1);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom("Terminator Room", 2)
                .ApplyEnterRoom("Parlor and Alcatraz", 1);

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectRunwayUsed("Base Runway - Parlor Top Left Door (to Terminator)", "Base")
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_ShinesparkDisabledButNoSparkNeeded_Succeeds()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canShinespark");
            model.ApplyLogicalOptions(logicalOptions);
            CanComeInCharged canComeInCharged = model.Rooms["Parlor and Alcatraz"].Links[1].To[8].Strats["SpeedBooster"]
                .Requires.LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.FromNode.Id == 1);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom("Terminator Room", 2)
                .ApplyEnterRoom("Parlor and Alcatraz", 1);

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectRunwayUsed("Base Runway - Parlor Top Left Door (to Terminator)", "Base")
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_UsesOtherRoomRunway_Succeeds()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 20;
            model.ApplyLogicalOptions(logicalOptions);
            CanComeInCharged canComeInCharged = model.Rooms["Parlor and Alcatraz"].Links[1].To[8].Strats["SpeedBooster"]
                .Requires.LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.FromNode.Id == 1);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom("Terminator Room", 2)
                .ApplyEnterRoom("Parlor and Alcatraz", 1);

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectRunwayUsed("Base Runway - Terminator Right Door (to Parlor)", "Base")
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_NeedsToCombineBothRunways_Succeeds()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TilesToShineCharge = 14;
            model.ApplyLogicalOptions(logicalOptions);
            CanComeInCharged canComeInCharged = model.Rooms["Mother Brain Room"].Links[8].To[3].Strats["Speed Zebetite Skip"]
                .Requires.LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.FromNode.Id == 2);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom("Rinka Shaft", 3)
                .ApplyEnterRoom("Mother Brain Room", 2)
                .ApplyVisitNode(8, "Base");

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectRunwayUsed("Base Runway - Mother Brain Room Right Door (to Rinka Shaft)", "Base")
                .ExpectRunwayUsed("Base Runway - Rinka Shaft Bottom Left Door (to Mother Brain)", "Base")
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -15)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_InRoomPathNotRespected_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanComeInCharged canComeInCharged = model.Rooms["Post Crocomire Farming Room"].Links[5].To[1].Strats["Shinespark"]
                .Requires.LogicalElement<Or>(0)
                .LogicalRequirements.LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.FromNode.Id == 4);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyEnterRoom("Crocomire's Room", 1)
                .ApplyEnterRoom("Post Crocomire Farming Room", 4)
                .ApplyVisitNode(5, "Base")
                .ApplyVisitNode(2, "Base")
                .ApplyVisitNode(5, "Base");

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        // Something with a remote CanLeaveCharged
        [Fact]
        public void Execute_RemoteCanLeaveCharged_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            CanComeInCharged canComeInCharged = model.Rooms["Gauntlet Entrance"].Links[2].To[1].Strats["Shinespark"]
                .Requires.LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.FromNode.Id == 2);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources()
                .ApplyAddItem("Charge")
                .ApplyAddItem("Wave")
                .ApplyEnterRoom("Landing Site", 3)
                .ApplyVisitNode(1, "Shinespark")
                .ApplyEnterRoom("Gauntlet Entrance", 2);

            // When
            ExecutionResult result = canComeInCharged.Execute(model, inGameState);

            // Expect
            CanLeaveCharged expectedCanLeaveCharged = model.Rooms["Landing Site"].Nodes[1].CanLeaveCharged
                .Where(canLeaveCharged => canLeaveCharged?.InitiateRemotely.MustOpenDoorFirst is false)
                .First();
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved("Charge")
                .ExpectItemInvolved("Wave")
                .ExpectItemInvolved(SuperMetroidModel.SPEED_BOOSTER_NAME)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -100)
                .ExpectCanLeaveChargedExecuted(expectedCanLeaveCharged, "Gauntlet Wraparound Shot")
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
            CanComeInCharged canComeInCharged = model.Rooms["Golden Torizo's Room"].Links[1].To[3].Strats["Shinespark"].Requires.LogicalElement<CanComeInCharged>(0);
            Assert.True(canComeInCharged.LogicallyRelevant);
            Assert.True(canComeInCharged.LogicallyNever);
            Assert.False(canComeInCharged.LogicallyAlways);
            Assert.False(canComeInCharged.LogicallyFree);

            CanComeInCharged noSparkCanComeInCharged = model.Rooms["Parlor and Alcatraz"].Links[1].To[8].Strats["SpeedBooster"].Requires.LogicalElement<CanComeInCharged>(0);
            Assert.True(noSparkCanComeInCharged.LogicallyRelevant);
            Assert.False(noSparkCanComeInCharged.LogicallyNever);
            Assert.False(noSparkCanComeInCharged.LogicallyAlways);
            Assert.False(noSparkCanComeInCharged.LogicallyFree);
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
            CanComeInCharged canComeInCharged = model.Rooms["Golden Torizo's Room"].Links[1].To[3].Strats["Shinespark"].Requires.LogicalElement<CanComeInCharged>(0);
            Assert.True(canComeInCharged.LogicallyRelevant);
            Assert.False(canComeInCharged.LogicallyNever);
            Assert.False(canComeInCharged.LogicallyAlways);
            Assert.False(canComeInCharged.LogicallyFree);
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
            CanComeInCharged canComeInCharged = model.Rooms["Landing Site"].Links[3].To[1].Strats["Shinespark"].Requires
                .LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.ShinesparkFrames == 125 && canComeInCharged.ExcessShinesparkFrames == 33);
            Assert.True(canComeInCharged.LogicallyRelevant);
            Assert.True(canComeInCharged.LogicallyNever);
            Assert.False(canComeInCharged.LogicallyAlways);
            Assert.False(canComeInCharged.LogicallyFree);
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
            CanComeInCharged canComeInCharged = model.Rooms["Landing Site"].Links[3].To[1].Strats["Shinespark"].Requires
                .LogicalElement<CanComeInCharged>(0, canComeInCharged => canComeInCharged.ShinesparkFrames == 125 && canComeInCharged.ExcessShinesparkFrames == 33);
            Assert.True(canComeInCharged.LogicallyRelevant);
            Assert.False(canComeInCharged.LogicallyNever);
            Assert.False(canComeInCharged.LogicallyAlways);
            Assert.False(canComeInCharged.LogicallyFree);
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

            CanComeInCharged canComeInCharged = model.Rooms["Golden Torizo's Room"].Links[1].To[3].Strats["Shinespark"].Requires.LogicalElement<CanComeInCharged>(0);
            Assert.True(canComeInCharged.LogicallyRelevant);
            Assert.False(canComeInCharged.LogicallyNever);
            Assert.False(canComeInCharged.LogicallyAlways);
            Assert.False(canComeInCharged.LogicallyFree);
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
            CanComeInCharged canComeInCharged = model.Rooms["Parlor and Alcatraz"].Links[1].To[8].Strats["SpeedBooster"].Requires.LogicalElement<CanComeInCharged>(0);
            Assert.True(canComeInCharged.LogicallyRelevant);
            Assert.True(canComeInCharged.LogicallyNever);
            Assert.False(canComeInCharged.LogicallyAlways);
            Assert.False(canComeInCharged.LogicallyFree);
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
            CanComeInCharged noSparkCanComeInCharged = model.Rooms["Parlor and Alcatraz"].Links[1].To[8].Strats["SpeedBooster"].Requires.LogicalElement<CanComeInCharged>(0);
            Assert.True(noSparkCanComeInCharged.LogicallyRelevant);
            Assert.False(noSparkCanComeInCharged.LogicallyNever);
            Assert.False(noSparkCanComeInCharged.LogicallyAlways);
            Assert.False(noSparkCanComeInCharged.LogicallyFree);

            CanComeInCharged canComeInCharged = model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Links[10].To[9].Strats["Shinespark"].Requires.LogicalElement<CanComeInCharged>(0);
            Assert.True(canComeInCharged.LogicallyRelevant);
            Assert.True(canComeInCharged.LogicallyNever);
            Assert.False(canComeInCharged.LogicallyAlways);
            Assert.False(canComeInCharged.LogicallyFree);
        }

        #endregion
    }
}
