using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Models.Items;

namespace sm_json_data_framework.Tests.Models.Requirements
{
    public class LogicalRequirementsTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            LogicalRequirements logicalRequirements = Model.Techs["canNonTrivialIceClip"].Requires;
            Assert.Equal(3, logicalRequirements.LogicalElements.Count);
            Assert.True(logicalRequirements.LogicalElements.First() is TechLogicalElement);
            Assert.Equal("canCeilingClip", ((TechLogicalElement)logicalRequirements.LogicalElements.First()).Tech.Name);
            Assert.True(logicalRequirements.LogicalElements.Skip(1).First() is TechLogicalElement);
            Assert.Equal("canTrickyUseFrozenEnemies", ((TechLogicalElement)logicalRequirements.LogicalElements.Skip(1).First()).Tech.Name);
            Assert.True(logicalRequirements.LogicalElements.Skip(2).First() is Or);
        }

        #endregion

        #region Tests for LogicalElement<T>()

        [Fact]
        public void LogicalElementTyped_NoElementsAtAll_ReturnsNull()
        {
            // Given
            LogicalRequirements logicalRequirements = Model.Techs["canBePatient"].Requires;

            // When
            Or result0 = logicalRequirements.LogicalElement<Or>(0);
            Or result1 = logicalRequirements.LogicalElement<Or>(1);

            // Expect
            Assert.Null(result0);
            Assert.Null(result1);
        }

        [Fact]
        public void LogicalElementTyped_NoElementsOfType_ReturnsNull()
        {
            // Given
            LogicalRequirements logicalRequirements = Model.Techs["canSunkenDualWallClimb"].Requires;

            // When
            Or result0 = logicalRequirements.LogicalElement<Or>(0);
            Or result1 = logicalRequirements.LogicalElement<Or>(1);

            // Expect
            Assert.Null(result0);
            Assert.Null(result1);
        }

        [Fact]
        public void LogicalElementTyped_NoElementsOfType_ReturnsNthElement()
        {
            // Given
            LogicalRequirements logicalRequirements = Model.Helpers["h_hasBeamUpgrade"].Requires.LogicalElement<Or>(0).LogicalRequirements;

            // When
            ItemLogicalElement result3 = logicalRequirements.LogicalElement<ItemLogicalElement>(3);
            ItemLogicalElement result6 = logicalRequirements.LogicalElement<ItemLogicalElement>(6);

            // Expect
            Assert.NotNull(result3);
            Assert.Equal("Spazer", result3.Item.Name);
            Assert.Null(result6);
        }

        #endregion

        #region Tests for LogicalElementsWhere<T>()

        [Fact]
        public void LogicalElementsTyped_NoElementsAtAll_ReturnsNull()
        {
            // Given
            LogicalRequirements logicalRequirements = Model.Techs["canBePatient"].Requires;

            // When
            IEnumerable<Or> result = logicalRequirements.LogicalElementsTyped<Or>();

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void LogicalElementsTyped_NoElementsOfType_ReturnsNull()
        {
            // Given
            LogicalRequirements logicalRequirements = Model.Techs["canSunkenDualWallClimb"].Requires;

            // When
            IEnumerable<Or> result = logicalRequirements.LogicalElementsTyped<Or>();

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void LogicalElemensTyped_ReturnsSubset()
        {
            // Given
            LogicalRequirements logicalRequirements = Model.Helpers["h_canHeatedBlueGateGlitch"].Requires;

            // When
            IEnumerable<HelperLogicalElement> result = logicalRequirements.LogicalElementsTyped<HelperLogicalElement>();

            // Expect
            Assert.Equal(2, result.Count());
        }

        #endregion

        #region Tests for LogicalElementsWhere<T>()

        [Fact]
        public void LogicalElementsWhere_NoElementsAtAll_ReturnsNull()
        {
            // Given
            LogicalRequirements logicalRequirements = Model.Techs["canBePatient"].Requires;

            // When
            IEnumerable<GameFlagLogicalElement> result = logicalRequirements.LogicalElementsWhere<GameFlagLogicalElement>(flagElement => flagElement.GameFlag.Name == "f_ZebesAwake");

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void LogicalElementsWhere_NoElementsOfType_ReturnsNull()
        {
            // Given
            LogicalRequirements logicalRequirements = Model.Techs["canSunkenDualWallClimb"].Requires;

            // When
            IEnumerable<GameFlagLogicalElement> result = logicalRequirements.LogicalElementsWhere<GameFlagLogicalElement>(flagElement => flagElement.GameFlag.Name == "f_ZebesAwake");

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void LogicalElemensWhere_ReturnsSubset()
        {
            // Given
            LogicalRequirements logicalRequirements = Model.Helpers["h_canHeatedBlueGateGlitch"].Requires;

            // When
            IEnumerable<HelperLogicalElement> result = logicalRequirements.LogicalElementsWhere<HelperLogicalElement>(helper => helper.Helper.Name == "h_canBlueGateGlitch");

            // Expect
            Assert.Equal(1, result.Count());
            Assert.Same(Model.Helpers["h_canBlueGateGlitch"], result.First().Helper);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_SomeElementsPossibleSomeNot_Fails()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState();
            LogicalRequirements logicalRequirements = Model.Helpers["h_canPlasmaHitbox"].Requires;

            // When
            ExecutionResult result = logicalRequirements.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_AllElementsPossible_SucceedsAndCombinesAllCosts()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SUPER_NAME)
                .ApplyRefillResources();
            LogicalRequirements logicalRequirements = Model.Helpers["h_canHeatedGreenGateGlitch"].Requires;

            // When
            ExecutionResult result = logicalRequirements.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .ExpectItemInvolved(SuperMetroidModel.SUPER_NAME)
                .ExpectResourceVariation(RechargeableResourceEnum.Super, -1)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -15)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_NoElements_SucceedsWithNoChanges()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState();
            LogicalRequirements logicalRequirements = Model.Techs["canBePatient"].Requires;

            // When
            ExecutionResult result = logicalRequirements.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .AssertRespectedBy(result);
        }

        #endregion

        #region Tests for ExecuteOneOrAll()

        [Fact]
        public void ExecuteOneOrAll_SomeElementsPossibleSomeNot_Succeeds()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem("Charge");
            LogicalRequirements logicalRequirements = Model.Techs["canWrapAroundShot"].Requires;

            // When
            ExecutionResult result = logicalRequirements.ExecuteOneOrAll(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .ExpectItemInvolved("Charge")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void ExecuteOneOrAll_SomeElementsFreeSomeNot_ExecutesFree()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem("Morph")
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyRefillResources();
            LogicalRequirements logicalRequirements = Model.Techs["canCrystalFlash"].Requires;

            // When
            ExecutionResult result = logicalRequirements.ExecuteOneOrAll(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .ExpectItemInvolved("Morph")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void ExecuteOneOrAll_NoElementsFree_ExecutesCheapest()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyAddItem(SuperMetroidModel.SUPER_NAME)
                .ApplyRefillResources();
            LogicalRequirements logicalRequirements = Model.Helpers["h_canBlueGateGlitch"].Requires.LogicalElement<Or>(0).LogicalRequirements;

            // When
            ExecutionResult result = logicalRequirements.ExecuteOneOrAll(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .ExpectItemInvolved(SuperMetroidModel.MISSILE_NAME)
                .ExpectResourceVariation(RechargeableResourceEnum.Missile, -1)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void ExecuteOneOrAll_NoElementsPossible_Fails()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState();
            LogicalRequirements logicalRequirements = Model.Techs["canBlueSpaceJump"].Requires;

            // When
            ExecutionResult result = logicalRequirements.ExecuteOneOrAll(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void ExecuteOneOrAll_NoElements_SucceedsWithNoChanges()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState();
            LogicalRequirements logicalRequirements = Model.Techs["canBePatient"].Requires;

            // When
            ExecutionResult result = logicalRequirements.ExecuteOneOrAll(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .AssertRespectedBy(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledGameFlag("f_ZebesAwake")
                .RegisterDisabledTech("canCrouchJump")
                .RegisterDisabledTech("canDownGrab");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Morph"])
                    .ApplyAddItem(ModelWithOptions.Items["Bombs"])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            LogicalRequirements oneFreeOneNeverOnePossible = ModelWithOptions.Rooms["Blue Brinstar Energy Tank Room"].Links[1].To[3].Strats["Ceiling E-Tank Dboost"].Requires;
            Assert.True(oneFreeOneNeverOnePossible.LogicallyRelevant);
            Assert.True(oneFreeOneNeverOnePossible.LogicallyNever);
            Assert.False(oneFreeOneNeverOnePossible.LogicallyAlways);
            Assert.False(oneFreeOneNeverOnePossible.LogicallyFree);
            Assert.False(oneFreeOneNeverOnePossible.LogicallyOrNever);
            Assert.True(oneFreeOneNeverOnePossible.LogicallyOrAlways);
            Assert.True(oneFreeOneNeverOnePossible.LogicallyOrFree);

            LogicalRequirements allFree = ModelWithOptions.Helpers["h_canUseMorphBombs"].Requires;
            Assert.True(allFree.LogicallyRelevant);
            Assert.False(allFree.LogicallyNever);
            Assert.True(allFree.LogicallyAlways);
            Assert.True(allFree.LogicallyFree);
            Assert.False(allFree.LogicallyOrNever);
            Assert.True(allFree.LogicallyOrAlways);
            Assert.True(allFree.LogicallyOrFree);

            LogicalRequirements allNever = ModelWithOptions.Helpers["h_canCrouchJumpDownGrab"].Requires;
            Assert.True(allNever.LogicallyRelevant);
            Assert.True(allNever.LogicallyNever);
            Assert.False(allNever.LogicallyAlways);
            Assert.False(allNever.LogicallyFree);
            Assert.True(allNever.LogicallyOrNever);
            Assert.False(allNever.LogicallyOrAlways);
            Assert.False(allNever.LogicallyOrFree);

            LogicalRequirements allPossible = ModelWithOptions.Helpers["h_canOpenGreenDoors"].Requires;
            Assert.True(allPossible.LogicallyRelevant);
            Assert.False(allPossible.LogicallyNever);
            Assert.False(allPossible.LogicallyAlways);
            Assert.False(allPossible.LogicallyFree);
            Assert.False(allPossible.LogicallyOrNever);
            Assert.False(allPossible.LogicallyOrAlways);
            Assert.False(allPossible.LogicallyOrFree);

            LogicalRequirements empty = ModelWithOptions.Techs["canSuitlessMaridia"].Requires;
            Assert.True(empty.LogicallyRelevant);
            Assert.False(empty.LogicallyNever);
            Assert.True(empty.LogicallyAlways);
            Assert.True(empty.LogicallyFree);
            Assert.False(empty.LogicallyOrNever);
            Assert.True(empty.LogicallyOrAlways);
            Assert.True(empty.LogicallyOrFree);
        }

        #endregion
    }
}
