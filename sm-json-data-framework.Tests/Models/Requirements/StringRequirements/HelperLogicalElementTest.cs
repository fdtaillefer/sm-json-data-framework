using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Items;

namespace sm_json_data_framework.Tests.Models.Requirements.StringRequirements
{
    public class HelperLogicalElementTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            HelperLogicalElement helperLogicalElement = Model.Techs["canHBJ"].Requires.LogicalElement<HelperLogicalElement>(0);
            Assert.Same(Model.Helpers["h_canUseMorphBombs"], helperLogicalElement.Helper);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_HelperRequirementsNotMet_Fails()
        {
            // Given
            HelperLogicalElement manyTriesHelperElement = Model.Helpers["h_canHeatedGreenGateGlitch"].Requires.LogicalElement<HelperLogicalElement>(1);
            InGameState inGameState = Model.CreateInitialGameState();

            // When
            ExecutionResult result = manyTriesHelperElement.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_HelperRequirementsMet_Succeeds()
        {
            // Given
            HelperLogicalElement manyTriesHelperElement = Model.Helpers["h_canHeatedGreenGateGlitch"].Requires.LogicalElement<HelperLogicalElement>(1);
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SUPER_NAME)
                .ApplyRefillResources();

            // When
            ExecutionResult result = manyTriesHelperElement.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.Super, -1)
                .ExpectItemInvolved(SuperMetroidModel.SUPER_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_HelperRequirementsMet_MultipleTriesConfigured_ConsumesMoreResources()
        {

            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterHelperTries("h_canGreenGateGlitch", 3);
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);
            HelperLogicalElement manyTriesHelperElement = ModelWithOptions.Helpers["h_canHeatedGreenGateGlitch"].Requires.LogicalElement<HelperLogicalElement>(1);
            InGameState inGameState = ModelWithOptions.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SUPER_NAME)
                .ApplyRefillResources();

            // When
            ExecutionResult result = manyTriesHelperElement.Execute(ModelWithOptions, inGameState);

            // Expect
            new ExecutionResultValidator(ModelWithOptions, inGameState)
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
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canGateGlitch")
                .RegisterHelperTries("h_canGreenGateGlitch", 2);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Morph"])
                    .ApplyAddItem(ModelWithOptions.Items["Bombs"])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            HelperLogicalElement impossibleHelperElement = ModelWithOptions.Helpers["h_canHeatedBlueGateGlitch"].Requires.LogicalElement<HelperLogicalElement>(0, element => element.Helper.Name == "h_canBlueGateGlitch");
            Assert.True(impossibleHelperElement.LogicallyRelevant);
            Assert.False(impossibleHelperElement.LogicallyAlways);
            Assert.False(impossibleHelperElement.LogicallyFree);
            Assert.True(impossibleHelperElement.LogicallyNever);
            Assert.Equal(1, impossibleHelperElement.Tries);

            HelperLogicalElement nonFreeHelperElement = ModelWithOptions.Helpers["h_canOpenYellowDoors"].Requires.LogicalElement<HelperLogicalElement>(0, element => element.Helper.Name == "h_canUsePowerBombs");
            Assert.True(nonFreeHelperElement.LogicallyRelevant);
            Assert.False(nonFreeHelperElement.LogicallyAlways);
            Assert.False(nonFreeHelperElement.LogicallyFree);
            Assert.False(nonFreeHelperElement.LogicallyNever);
            Assert.Equal(1, nonFreeHelperElement.Tries);

            HelperLogicalElement freeHelperElement = ModelWithOptions.Techs["canHBJ"].Requires.LogicalElement<HelperLogicalElement>(0, element => element.Helper.Name == "h_canUseMorphBombs");
            Assert.True(freeHelperElement.LogicallyRelevant);
            Assert.True(freeHelperElement.LogicallyAlways);
            Assert.True(freeHelperElement.LogicallyFree);
            Assert.False(freeHelperElement.LogicallyNever);
            Assert.Equal(1, freeHelperElement.Tries);

            HelperLogicalElement manyTriesHelperElement = ModelWithOptions.Helpers["h_canHeatedGreenGateGlitch"].Requires.LogicalElement<HelperLogicalElement>(1);
            Assert.Equal(2, manyTriesHelperElement.Tries);
        }

        #endregion
    }
}
