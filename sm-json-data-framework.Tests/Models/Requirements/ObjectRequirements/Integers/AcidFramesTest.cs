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
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements;
using System.Reflection;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.Integers
{
    public class AcidFramesTest
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
            AcidFrames acidFrames = model.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires.LogicalElement<AcidFrames>(0);
            Assert.Equal(30, acidFrames.Frames);
        }

        #endregion

        #region Tests for CalculateDamage()

        [Fact]
        public void CalculateDamage_Suitless_ReturnsCorrectValue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AcidFrames acidFrames = model.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires
                .LogicalElement<AcidFrames>(0, acidFrames => acidFrames.Frames == 30);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            int result = acidFrames.CalculateDamage(model, inGameState);

            // Expect
            Assert.Equal(45, result);
        }

        [Fact]
        public void CalculateDamage_WithVaria_ReturnsCorrectValue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AcidFrames acidFrames = model.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires
                .LogicalElement<AcidFrames>(0, acidFrames => acidFrames.Frames == 30);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME);

            // When
            int result = acidFrames.CalculateDamage(model, inGameState);

            // Expect
            Assert.Equal(22, result);
        }

        [Fact]
        public void CalculateDamage_WithGravity_ReturnsCorrectValue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AcidFrames acidFrames = model.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires
                .LogicalElement<AcidFrames>(0, acidFrames => acidFrames.Frames == 30);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.GRAVITY_SUIT_NAME);

            // When
            int result = acidFrames.CalculateDamage(model, inGameState);

            // Expect
            Assert.Equal(11, result);
        }

        [Fact]
        public void CalculateDamage_WithLeniency_ReturnsCorrectValue()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            AcidFrames acidFrames = model.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires
                .LogicalElement<AcidFrames>(0, acidFrames => acidFrames.Frames == 30);
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.AcidLeniencyMultiplier = 1.5M;
            model.ApplyLogicalOptions(logicalOptions);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME);

            // When
            int result = acidFrames.CalculateDamage(model, inGameState);

            // Expect
            Assert.Equal(33, result);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_NotEnoughEnergy_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AcidFrames acidFrames = model.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires
                .LogicalElement<AcidFrames>(0, acidFrames => acidFrames.Frames == 30);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 54);

            // When
            ExecutionResult result = acidFrames.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_EnoughEnergy_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AcidFrames acidFrames = model.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires
                .LogicalElement<AcidFrames>(0, acidFrames => acidFrames.Frames == 30);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 53);

            // When
            ExecutionResult result = acidFrames.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -45)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_EnoughEnergyWithVaria_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AcidFrames acidFrames = model.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires
                .LogicalElement<AcidFrames>(0, acidFrames => acidFrames.Frames == 30);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 76)
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME);

            // When
            ExecutionResult result = acidFrames.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -22)
                .ExpectDamageReducingItemInvolved(SuperMetroidModel.VARIA_SUIT_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_EnoughEnergyWithGravity_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AcidFrames acidFrames = model.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires
                .LogicalElement<AcidFrames>(0, acidFrames => acidFrames.Frames == 30);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.GRAVITY_SUIT_NAME);

            // When
            ExecutionResult result = acidFrames.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -11)
                .ExpectDamageReducingItemInvolved(SuperMetroidModel.GRAVITY_SUIT_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_WithLeniencyMultiplier_ConsumesCorrectAmount()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            AcidFrames acidFrames = model.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires
                .LogicalElement<AcidFrames>(0, acidFrames => acidFrames.Frames == 30);
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.AcidLeniencyMultiplier = 1.5M;
            model.ApplyLogicalOptions(logicalOptions);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = acidFrames.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -67)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_MultipleTimes_ConsumesCorrectAmount()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AcidFrames acidFrames = model.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires
                .LogicalElement<AcidFrames>(0, acidFrames => acidFrames.Frames == 30);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = acidFrames.Execute(model, inGameState, times:2);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -90)
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
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 29);
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
            AcidFrames acidFrames = model.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires
                .LogicalElement<AcidFrames>(0, acidFrames => acidFrames.Frames == 30);
            Assert.True(acidFrames.LogicallyRelevant);
            Assert.True(acidFrames.LogicallyNever);
            Assert.False(acidFrames.LogicallyAlways);
            Assert.False(acidFrames.LogicallyFree);
            Assert.Equal(LogicalOptions.DefaultFrameLeniencyMultiplier, acidFrames.AcidLeniencyMultiplier);
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
            AcidFrames acidFrames = model.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires
                .LogicalElement<AcidFrames>(0, acidFrames => acidFrames.Frames == 30);
            Assert.True(acidFrames.LogicallyRelevant);
            Assert.False(acidFrames.LogicallyNever);
            Assert.False(acidFrames.LogicallyAlways);
            Assert.False(acidFrames.LogicallyFree);
            Assert.Equal(LogicalOptions.DefaultFrameLeniencyMultiplier, acidFrames.AcidLeniencyMultiplier);
        }

        [Fact]
        public void ApplyLogicalOptions_BothSuitsFree_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 29);
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
            AcidFrames acidFrames = model.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires
                .LogicalElement<AcidFrames>(0, acidFrames => acidFrames.Frames == 30);
            Assert.True(acidFrames.LogicallyRelevant);
            Assert.False(acidFrames.LogicallyNever);
            Assert.False(acidFrames.LogicallyAlways);
            Assert.False(acidFrames.LogicallyFree);
            Assert.Equal(LogicalOptions.DefaultFrameLeniencyMultiplier, acidFrames.AcidLeniencyMultiplier);
        }

        [Fact]
        public void ApplyLogicalOptions_DifferentAcidLeniency_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.AcidLeniencyMultiplier = 2;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            AcidFrames acidFrames = model.Rooms["Metroid Room 1"].Links[3].To[2].Strats["Fearless Dive"].Requires
                .LogicalElement<AcidFrames>(0, acidFrames => acidFrames.Frames == 30);
            Assert.Equal(2, acidFrames.AcidLeniencyMultiplier);
        }

        #endregion
    }
}
