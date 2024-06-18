using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.Integers
{
    public class DraygonElectricityFramesTest
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
            DraygonElectricityFrames electricityFrames = model.Techs["canDraygonTurretGrappleJump"].Requires.LogicalElement<DraygonElectricityFrames>(0);
            Assert.Equal(60, electricityFrames.Frames);
        }

        #endregion

        #region Tests for CalculateDamage()

        [Fact]
        public void CalculateDamage_Suitless_ReturnsCorrectValue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            DraygonElectricityFrames electricityFrames = model.Techs["canDraygonTurretGrappleJump"].Requires
                .LogicalElement<DraygonElectricityFrames>(0, element => element.Frames == 60);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            int result = electricityFrames.CalculateDamage(model, inGameState);

            // Expect
            Assert.Equal(60, result);
        }

        [Fact]
        public void CalculateDamage_WithVaria_ReturnsCorrectValue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            DraygonElectricityFrames electricityFrames = model.Techs["canDraygonTurretGrappleJump"].Requires
                .LogicalElement<DraygonElectricityFrames>(0, element => element.Frames == 60);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME);

            // When
            int result = electricityFrames.CalculateDamage(model, inGameState);

            // Expect
            Assert.Equal(30, result);
        }

        [Fact]
        public void CalculateDamage_WithGravity_ReturnsCorrectValue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            DraygonElectricityFrames electricityFrames = model.Techs["canDraygonTurretGrappleJump"].Requires
                .LogicalElement<DraygonElectricityFrames>(0, element => element.Frames == 60);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.GRAVITY_SUIT_NAME);

            // When
            int result = electricityFrames.CalculateDamage(model, inGameState);

            // Expect
            Assert.Equal(15, result);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_NotEnoughEnergy_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            DraygonElectricityFrames electricityFrames = model.Techs["canDraygonTurretGrappleJump"].Requires
                .LogicalElement<DraygonElectricityFrames>(0, element => element.Frames == 60);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 39);

            // When
            ExecutionResult result = electricityFrames.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_EnoughEnergy_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            DraygonElectricityFrames electricityFrames = model.Techs["canDraygonTurretGrappleJump"].Requires
                .LogicalElement<DraygonElectricityFrames>(0, element => element.Frames == 60);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 38);

            // When
            ExecutionResult result = electricityFrames.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -60)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_EnoughEnergyWithVaria_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            DraygonElectricityFrames electricityFrames = model.Techs["canDraygonTurretGrappleJump"].Requires
                .LogicalElement<DraygonElectricityFrames>(0, element => element.Frames == 60);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME);

            // When
            ExecutionResult result = electricityFrames.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -30)
                .ExpectDamageReducingItemInvolved(SuperMetroidModel.VARIA_SUIT_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_EnoughEnergyWithGravity_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            DraygonElectricityFrames electricityFrames = model.Techs["canDraygonTurretGrappleJump"].Requires
                .LogicalElement<DraygonElectricityFrames>(0, element => element.Frames == 60);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.GRAVITY_SUIT_NAME);

            // When
            ExecutionResult result = electricityFrames.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -15)
                .ExpectDamageReducingItemInvolved(SuperMetroidModel.GRAVITY_SUIT_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_MultipleTimes_ConsumesCorrectAmount()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            DraygonElectricityFrames electricityFrames = model.Techs["canDraygonTurretGrappleJump"].Requires
                .LogicalElement<DraygonElectricityFrames>(0, element => element.Frames == 60);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources();

            // When
            ExecutionResult result = electricityFrames.Execute(model, inGameState, times: 2);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -120)
                .AssertRespectedBy(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_LessPossibleEnergyThanBestCaseDamage_SetsLogicalPropertiesOnDamageLogicalElements()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .RegisterRemovedItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            baseResouces.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 60);
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
            DraygonElectricityFrames electricityFrames = model.Techs["canDraygonTurretGrappleJump"].Requires
                .LogicalElement<DraygonElectricityFrames>(0, element => element.Frames == 60);
            Assert.True(electricityFrames.LogicallyRelevant);
            Assert.True(electricityFrames.LogicallyNever);
            Assert.False(electricityFrames.LogicallyAlways);
            Assert.False(electricityFrames.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_NormalPossibleEnergy_SetsLogicalPropertiesOnDamageLogicalElements()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .RegisterRemovedItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
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
            DraygonElectricityFrames electricityFrames = model.Techs["canDraygonTurretGrappleJump"].Requires
                .LogicalElement<DraygonElectricityFrames>(0, element => element.Frames == 60);
            Assert.True(electricityFrames.LogicallyRelevant);
            Assert.False(electricityFrames.LogicallyNever);
            Assert.False(electricityFrames.LogicallyAlways);
            Assert.False(electricityFrames.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_BothSuitsFree_SetsLogicalPropertiesOnDamageLogicalElements()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            baseResouces.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 29);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(
                    ItemInventory.CreateVanillaStartingInventory(model)
                        .ApplyAddItem(model.Items[SuperMetroidModel.VARIA_SUIT_NAME])
                        .ApplyAddItem(model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME])
                )
                .BaseResourceMaximums(baseResouces)
                .StartingResources(baseResouces)
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            DraygonElectricityFrames electricityFrames = model.Techs["canDraygonTurretGrappleJump"].Requires
                .LogicalElement<DraygonElectricityFrames>(0, element => element.Frames == 60);
            Assert.True(electricityFrames.LogicallyRelevant);
            Assert.False(electricityFrames.LogicallyNever);
            Assert.False(electricityFrames.LogicallyAlways);
            Assert.False(electricityFrames.LogicallyFree);
        }

        #endregion
    }
}
