using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;

namespace sm_json_data_framework.Tests.Models.Rooms
{
    public class FarmCycleTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            FarmCycle farmCycle = Model.RoomEnemies["Early Supers Zeb"].FarmCycles["Crouch over spawn point"];
            Assert.Equal("Crouch over spawn point", farmCycle.Name);
            Assert.Equal(120, farmCycle.CycleFrames);
            Assert.NotNull(farmCycle.Requires);
            Assert.Empty(farmCycle.Requires.LogicalElements);
            Assert.Same(Model.RoomEnemies["Early Supers Zeb"], farmCycle.RoomEnemy);
        }

        #endregion

        #region Tests for IsFree()

        [Fact]
        public void IsFree_AlwaysFree_ReturnsTrue()
        {
            // Given
            FarmCycle farmCycle = Model.RoomEnemies["Early Supers Zeb"].FarmCycles["Crouch over spawn point"];
            InGameState inGameState = Model.CreateInitialGameState();

            // When
            bool result = farmCycle.IsFree(Model, inGameState);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsFree_FreeThanksToVaria_ReturnsTrue()
        {
            // Given
            FarmCycle farmCycle = Model.RoomEnemies["Plowerhouse Room Left Zebbo"].FarmCycles["Crouch over spawn point"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME);

            // When
            bool result = farmCycle.IsFree(Model, inGameState);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsFree_DoableButCostsResources_ReturnsFalse()
        {
            // Given
            FarmCycle farmCycle = Model.RoomEnemies["Plowerhouse Room Left Zebbo"].FarmCycles["Crouch over spawn point"];
            InGameState inGameState = Model.CreateInitialGameState();

            // When
            bool result = farmCycle.IsFree(Model, inGameState);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsFree_NotDoable_ReturnsFalse()
        {
            // Given
            FarmCycle farmCycle = Model.RoomEnemies["Gauntlet E-Tank Zebbo"].FarmCycles["Grapple three tiles away"];
            InGameState inGameState = Model.CreateInitialGameState();

            // When
            bool result = farmCycle.IsFree(Model, inGameState);

            // Expect
            Assert.False(result);
        }

        #endregion

        #region Tests for RequirementsExecution.Execute()

        [Fact]
        public void RequirementsExecutionExecute_Possible_Succeeds()
        {
            // Given
            FarmCycle farmCycle = Model.RoomEnemies["Gauntlet E-Tank Zebbo"].FarmCycles["Grapple three tiles away"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem("Grapple");

            // When
            ExecutionResult result = farmCycle.RequirementExecution.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .ExpectItemInvolved("Grapple")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void RequirementsExecutionExecute_PossibleButNotFree_Succeeds()
        {
            // Given
            FarmCycle farmCycle = Model.RoomEnemies["Plowerhouse Room Left Zebbo"].FarmCycles["Crouch over spawn point"];
            InGameState inGameState = Model.CreateInitialGameState();

            // When
            ExecutionResult result = farmCycle.RequirementExecution.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                // This execution only executes the requirements, it doesn't add any drops
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -30)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void RequirementsExecutionExecute_NotPossible_Fails()
        {
            // Given
            FarmCycle farmCycle = Model.RoomEnemies["Gauntlet E-Tank Zebbo"].FarmCycles["Grapple three tiles away"];
            InGameState inGameState = Model.CreateInitialGameState();

            // When
            ExecutionResult result = farmCycle.RequirementExecution.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        #endregion

        #region Tests for FarmExecution.Execute()

        [Fact]
        public void FarmExecutionExecute_Free_RefillsLogicallyFarmableResources()
        {
            // Given
            // Test with a 2-second Zebbo cycle. Drop rates for missile and PowerBomb are too low to logically expect by default.
            FarmCycle farmCycle = Model.RoomEnemies["Etecoon E-Tank Middle Zebbo"].FarmCycles["Crouch over spawn point"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyAddItem(SuperMetroidModel.SUPER_NAME)
                .ApplyAddItem(SuperMetroidModel.POWER_BOMB_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Missile, 4)
                .ApplyConsumeResource(ConsumableResourceEnum.Super, 4)
                .ApplyConsumeResource(ConsumableResourceEnum.PowerBomb, 4)
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 80)
                .ApplyEnterRoom(farmCycle.RoomEnemy.HomeNodes[4]);

            // When
            ExecutionResult result = farmCycle.FarmExecution.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, 80)
                .ExpectResourceVariation(RechargeableResourceEnum.Super, 4)
                // Missile drop rate is horrendous, but it becomes super high once energy is filled so it becomes farmable
                .ExpectResourceVariation(RechargeableResourceEnum.Missile, 4)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void FarmExecutionExecute_UnstableResource_WouldStabilizeInTimeButUpFrontCostTooHigh_Fails()
        {
            // Given
            // Test with a 2-second 5-Gamet heated cycle. Energy will be unstable until missiles are filled.
            // But, that takes 5 cycles. We will not have the 30 energy needed to execute the last cycle before stabilizing.
            FarmCycle farmCycle = Model.RoomEnemies["Upper Norfair Farming Room Gamets"].FarmCycles["Gamet down shots"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyAddItem(SuperMetroidModel.SUPER_NAME)
                .ApplyAddItem(SuperMetroidModel.SUPER_NAME)
                .ApplyAddItem(SuperMetroidModel.POWER_BOMB_NAME)
                .ApplyAddItem(SuperMetroidModel.POWER_BOMB_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Missile, 10)
                .ApplyConsumeResource(ConsumableResourceEnum.Super, 9)
                .ApplyConsumeResource(ConsumableResourceEnum.PowerBomb, 9)
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 54)
                .ApplyEnterRoom(farmCycle.RoomEnemy.HomeNodes[5]);

            // When
            ExecutionResult result = farmCycle.FarmExecution.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void FarmExecutionExecute_UnstableResourceStabilizesInTime_RefillsLogicallyFarmableResources()
        {
            // Given
            // Test with a 2-second 5-Gamet heated cycle. Energy will be unstable until missiles are filled.
            FarmCycle farmCycle = Model.RoomEnemies["Upper Norfair Farming Room Gamets"].FarmCycles["Gamet down shots"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyAddItem(SuperMetroidModel.SUPER_NAME)
                .ApplyAddItem(SuperMetroidModel.SUPER_NAME)
                .ApplyAddItem(SuperMetroidModel.POWER_BOMB_NAME)
                .ApplyAddItem(SuperMetroidModel.POWER_BOMB_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Missile, 10)
                .ApplyConsumeResource(ConsumableResourceEnum.Super, 9)
                .ApplyConsumeResource(ConsumableResourceEnum.PowerBomb, 9)
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 49)
                .ApplyEnterRoom(farmCycle.RoomEnemy.HomeNodes[5]);

            // When
            ExecutionResult result = farmCycle.FarmExecution.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, 49)
                .ExpectResourceVariation(RechargeableResourceEnum.Super, 9)
                // Missile drop rate is horrendous, but it becomes super high once energy is filled so it becomes farmable
                .ExpectResourceVariation(RechargeableResourceEnum.Missile, 10)
                .AssertRespectedBy(result);
        }
        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Grapple");

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            FarmCycle impossibleCycle = ModelWithOptions.RoomEnemies["Gauntlet E-Tank Zebbo"].FarmCycles["Grapple three tiles away"];
            Assert.False(impossibleCycle.LogicallyRelevant);
            Assert.True(impossibleCycle.LogicallyNever);

            FarmCycle possibleCycle = ModelWithOptions.RoomEnemies["Gauntlet E-Tank Zebbo"].FarmCycles["Shoot and jump three tiles away"];
            Assert.True(possibleCycle.LogicallyRelevant);
            Assert.False(possibleCycle.LogicallyNever);
        }

        #endregion
    }
}
