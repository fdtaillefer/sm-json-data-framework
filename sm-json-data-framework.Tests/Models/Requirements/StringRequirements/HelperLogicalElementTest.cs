using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.InGameStates;

namespace sm_json_data_framework.Tests.Models.Requirements.StringRequirements
{
    public class HelperLogicalElementTest
    {
        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel NewModelForOptions() => StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            SuperMetroidModel model = ReusableModel();
            HelperLogicalElement helperLogicalElement = model.Techs["canHBJ"].Requires
                .LogicalElement<HelperLogicalElement>(0, element => element.Helper.Name == "h_canUseMorphBombs");
            Assert.Same(model.Helpers["h_canUseMorphBombs"], helperLogicalElement.Helper);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_HelperRequirementsNotMet_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HelperLogicalElement manyTriesHelperElement = model.Helpers["h_canHeatedGreenGateGlitch"].Requires
                .LogicalElement<HelperLogicalElement>(0, element => element.Helper.Name == "h_canGreenGateGlitch");
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = manyTriesHelperElement.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_HelperRequirementsMet_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HelperLogicalElement manyTriesHelperElement = model.Helpers["h_canHeatedGreenGateGlitch"].Requires
                .LogicalElement<HelperLogicalElement>(0, element => element.Helper.Name == "h_canGreenGateGlitch");
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SUPER_NAME)
                .ApplyRefillResources();

            // When
            ExecutionResult result = manyTriesHelperElement.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.Super, -1)
                .ExpectItemInvolved(SuperMetroidModel.SUPER_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_HelperRequirementsMet_MultipleTriesConfigured_ConsumesMoreResources()
        {

            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterHelperTries("h_canGreenGateGlitch", 3);
            model.ApplyLogicalOptions(logicalOptions);
            HelperLogicalElement manyTriesHelperElement = model.Helpers["h_canHeatedGreenGateGlitch"].Requires
                .LogicalElement<HelperLogicalElement>(0, element => element.Helper.Name == "h_canGreenGateGlitch");
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SUPER_NAME)
                .ApplyRefillResources();

            // When
            ExecutionResult result = manyTriesHelperElement.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.Super, -3)
                .ExpectItemInvolved(SuperMetroidModel.SUPER_NAME)
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
                .RegisterDisabledTech("canGateGlitch")
                .RegisterHelperTries("h_canGreenGateGlitch", 2);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items["Morph"])
                    .ApplyAddItem(model.Items["Bombs"])
                )
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            HelperLogicalElement impossibleHelperElement = model.Helpers["h_canHeatedBlueGateGlitch"].Requires.LogicalElement<HelperLogicalElement>(0, element => element.Helper.Name == "h_canBlueGateGlitch");
            Assert.True(impossibleHelperElement.LogicallyRelevant);
            Assert.False(impossibleHelperElement.LogicallyAlways);
            Assert.False(impossibleHelperElement.LogicallyFree);
            Assert.True(impossibleHelperElement.LogicallyNever);
            Assert.Equal(1, impossibleHelperElement.Tries);

            HelperLogicalElement nonFreeHelperElement = model.Helpers["h_canOpenYellowDoors"].Requires.LogicalElement<HelperLogicalElement>(0, element => element.Helper.Name == "h_canUsePowerBombs");
            Assert.True(nonFreeHelperElement.LogicallyRelevant);
            Assert.False(nonFreeHelperElement.LogicallyAlways);
            Assert.False(nonFreeHelperElement.LogicallyFree);
            Assert.False(nonFreeHelperElement.LogicallyNever);
            Assert.Equal(1, nonFreeHelperElement.Tries);

            HelperLogicalElement freeHelperElement = model.Techs["canHBJ"].Requires.LogicalElement<HelperLogicalElement>(0, element => element.Helper.Name == "h_canUseMorphBombs");
            Assert.True(freeHelperElement.LogicallyRelevant);
            Assert.True(freeHelperElement.LogicallyAlways);
            Assert.True(freeHelperElement.LogicallyFree);
            Assert.False(freeHelperElement.LogicallyNever);
            Assert.Equal(1, freeHelperElement.Tries);

            HelperLogicalElement manyTriesHelperElement = model.Helpers["h_canHeatedGreenGateGlitch"].Requires
                .LogicalElement<HelperLogicalElement>(0, element => element.Helper.Name == "h_canGreenGateGlitch");
            Assert.Equal(2, manyTriesHelperElement.Tries);
        }

        #endregion
    }
}
