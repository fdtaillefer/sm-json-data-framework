using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.InGameStates;

namespace sm_json_data_framework.Tests.Models.Rooms.Nodes
{
    public class NodeLockTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            NodeLock nodeLock = Model.Locks["West Ocean Ship Exit Grey Lock (to Gravity Suit Room)"];
            Assert.Equal(LockTypeEnum.Permanent, nodeLock.LockType);
            Assert.Empty(nodeLock.Lock.LogicalElements);
            Assert.Equal("West Ocean Ship Exit Grey Lock (to Gravity Suit Room)", nodeLock.Name);
            Assert.Equal(1, nodeLock.UnlockStrats.Count);
            Assert.Contains("Base", nodeLock.UnlockStrats.Keys);
            Assert.Equal(1, nodeLock.BypassStrats.Count);
            Assert.Contains("Bowling Skip", nodeLock.BypassStrats.Keys);
            Assert.Empty(nodeLock.Yields);
            Assert.Same(Model.Rooms["West Ocean"].Nodes[4], nodeLock.Node);

            NodeLock nodeLockWithYields = Model.Locks["Pit Room Left Grey Lock (to Climb)"];
            Assert.Equal(1, nodeLockWithYields.Yields.Count);
            Assert.Same(Model.GameFlags["f_ZebesAwake"], nodeLockWithYields.Yields["f_ZebesAwake"]);

            NodeLock nonSystematicLock = Model.Locks["Morph Ball Room Grey Lock (to Green Hill Zone)"];
            Assert.NotNull(nonSystematicLock.Lock);
            Assert.Equal(1, nonSystematicLock.Lock.LogicalElements.Count);
            Assert.NotNull(nonSystematicLock.Lock.LogicalElement<GameFlagLogicalElement>(0));
        }

        #endregion

        #region Tests for IsOpen()

        [Fact]
        public void IsOpen_NotOpen_ReturnsFalse()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState();
            NodeLock nodeLock = Model.Locks["West Ocean Ship Exit Grey Lock (to Gravity Suit Room)"];

            // When
            bool result = nodeLock.IsOpen(Model, inGameState);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsOpen_Open_ReturnsTrue()
        {
            // Given
            NodeLock nodeLock = Model.Locks["Landing Site Bottom Right Green Lock (to Crateria Tube)"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyOpenLock(nodeLock, applyToRoomState: false);

            // When
            bool result = nodeLock.IsOpen(Model, inGameState);

            // Expect
            Assert.True(result);
        }

        #endregion

        #region Tests for IsActive()

        [Fact]
        public void IsActive_InitiallyActiveNotOpen_ReturnsTrue()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState();
            NodeLock nodeLock = Model.Locks["West Ocean Ship Exit Grey Lock (to Gravity Suit Room)"];

            // When
            bool result = nodeLock.IsActive(Model, inGameState);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsActive_InitiallyActiveButOpen_ReturnsFalse()
        {
            // Given
            NodeLock nodeLock = Model.Locks["Landing Site Bottom Right Green Lock (to Crateria Tube)"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyOpenLock(nodeLock, applyToRoomState: false);

            // When
            bool result = nodeLock.IsActive(Model, inGameState);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsActive_LockConditionsNotMet_ReturnsFalse()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState();
            NodeLock nodeLock = Model.Locks["Attic Bottom Grey Lock (to Main Shaft)"];

            // When
            bool result = nodeLock.IsActive(Model, inGameState);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsActive_LockConditionsMetNotOpen_ReturnsTrue()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddGameFlag("f_DefeatedPhantoon");
            NodeLock nodeLock = Model.Locks["Attic Bottom Grey Lock (to Main Shaft)"];

            // When
            bool result = nodeLock.IsActive(Model, inGameState);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsActive_LockConditionsMetAndLockOpen_ReturnsFalse()
        {
            // Given
            NodeLock nodeLock = Model.Locks["Attic Bottom Grey Lock (to Main Shaft)"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddGameFlag("f_DefeatedPhantoon")
                .ApplyOpenLock(nodeLock, applyToRoomState: false);

            // When
            bool result = nodeLock.IsActive(Model, inGameState);

            // Expect
            Assert.False(result);
        }

        #endregion

        #region Tests for OpenExecution.Execute()

        [Fact]
        public void OpenExecutionExecute_InactiveLock_Fails()
        {
            // Given
            InGameState inGameState = Model.CreateInitialGameState();
            NodeLock nodeLock = Model.Locks["Attic Bottom Grey Lock (to Main Shaft)"];

            // When
            ExecutionResult result = nodeLock.OpenExecution.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void OpenExecutionExecute_ActiveAndOpenable_Succeeds()
        {
            // Given
            NodeLock nodeLock = Model.Locks["Landing Site Bottom Right Green Lock (to Crateria Tube)"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.SUPER_NAME)
                .ApplyRefillResources()
                .ApplyEnterRoom(nodeLock.Node);

            // When
            ExecutionResult result = nodeLock.OpenExecution.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .ExpectLockOpened(nodeLock.Name, "Base")
                .ExpectItemInvolved(SuperMetroidModel.SUPER_NAME)
                .ExpectResourceVariation(RechargeableResourceEnum.Super, -1)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void OpenExecutionExecute_ActiveAndNotFulfillable_Fails()
        {
            // Given
            NodeLock nodeLock = Model.Locks["West Ocean Ship Exit Grey Lock (to Gravity Suit Room)"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(nodeLock.Node);

            // When
            ExecutionResult result = nodeLock.OpenExecution.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void OpenExecutionExecute_OpenableWithYields_ActivatesFlag()
        {
            // Given
            NodeLock nodeLock = Model.Locks["Pit Room Left Grey Lock (to Climb)"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyAddItem("Morph")
                .ApplyEnterRoom(nodeLock.Node);

            // When
            ExecutionResult result = nodeLock.OpenExecution.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .ExpectLockOpened(nodeLock.Name, "Base")
                .ExpectItemInvolved(SuperMetroidModel.MISSILE_NAME)
                .ExpectItemInvolved("Morph")
                .ExpectGameFlagActivated("f_ZebesAwake")
                .AssertRespectedBy(result);
        }

        #endregion

        #region Tests for BypassExecution.Execute()

        [Fact]
        public void BypassExecutionExecute_InactiveLock_Fails()
        {
            // Given
            NodeLock nodeLock = Model.Locks["Butterfly Room Grey Lock (to West Cactus Alley)"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem("Ice")
                .ApplyAddItem("Wave")
                .ApplyOpenLock(nodeLock, applyToRoomState: false)
                .ApplyEnterRoom(nodeLock.Node);

            // When
            ExecutionResult result = nodeLock.BypassExecution.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void BypassExecutionExecute_NoBypassStrats_Fails()
        {
            // Given
            NodeLock nodeLock = Model.Locks["Landing Site Bottom Right Green Lock (to Crateria Tube)"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(nodeLock.Node);

            // When
            ExecutionResult result = nodeLock.BypassExecution.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void BypassExecutionExecute_NotFulfillable_Fails()
        {
            // Given
            NodeLock nodeLock = Model.Locks["Butterfly Room Grey Lock (to West Cactus Alley)"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(nodeLock.Node);

            // When
            ExecutionResult result = nodeLock.BypassExecution.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void BypassExecutionExecute_Fulfillable_Succeeds()
        {
            // Given
            NodeLock nodeLock = Model.Locks["Butterfly Room Grey Lock (to West Cactus Alley)"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem("Ice")
                .ApplyAddItem("Wave")
                .ApplyEnterRoom(nodeLock.Node);

            // When
            ExecutionResult result = nodeLock.BypassExecution.Execute(Model, inGameState);

            // Expect
            Assert.NotNull(result);

            Assert.Single(result.BypassedLocks);
            Assert.Empty(result.OpenedLocks);
            Assert.Same(nodeLock, result.BypassedLocks[nodeLock.Name].bypassedLock);
            Assert.Same(nodeLock.BypassStrats["Botwoon Skip"], result.BypassedLocks[nodeLock.Name].stratUsed);

            Assert.Empty(result.CanLeaveChargedExecuted);
            Assert.Equal(2, result.ItemsInvolved.Count);
            Assert.Same(Model.Items["Ice"], result.ItemsInvolved["Ice"]);
            Assert.Same(Model.Items["Wave"], result.ItemsInvolved["Wave"]);

            Assert.Empty(result.ActivatedGameFlags);
            Assert.Empty(result.DamageReducingItemsInvolved);
            Assert.Empty(result.KilledEnemies);
            Assert.Empty(result.RunwaysUsed);

            Assert.Equal(69, result.ResultingState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(inGameState.ResourceMaximums, result.ResultingState.ResourceMaximums);
            Assert.Equal(inGameState.OpenedLocks.Count, result.ResultingState.OpenedLocks.Count);
            Assert.DoesNotContain(nodeLock.Name, result.ResultingState.OpenedLocks.Keys);
            Assert.Equal(inGameState.ActiveGameFlags.Count, result.ResultingState.ActiveGameFlags.Count);
            Assert.Equal(inGameState.TakenItemLocations.Count, result.ResultingState.TakenItemLocations.Count);
            Assert.Same(inGameState.CurrentRoom, result.ResultingState.CurrentRoom);
            Assert.Same(inGameState.CurrentNode, result.ResultingState.CurrentNode);
            Assert.True(result.ResultingState.Inventory.ExceptIn(inGameState.Inventory).Empty);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Bombs");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions)
                .StartingGameFlags(new List<string> { "f_ZebesAwake" })
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            NodeLock neverActiveLock = ModelWithOptions.Rooms["Bomb Torizo Room"].Nodes[1].Locks["Bomb Torizo Room Grey Lock (to Flyway)"];
            Assert.False(neverActiveLock.LogicallyRelevant);
            Assert.False(neverActiveLock.LogicallyNever);
            Assert.True(neverActiveLock.LogicallyAlways);
            Assert.True(neverActiveLock.LogicallyFree);

            NodeLock unpassableLock = ModelWithOptions.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[7].Locks["Etecoon Exit Grey Lock"];
            Assert.True(unpassableLock.LogicallyRelevant);
            Assert.True(unpassableLock.LogicallyNever);
            Assert.False(unpassableLock.LogicallyAlways);
            Assert.False(unpassableLock.LogicallyFree);

            NodeLock greyPossibleBypassableLock = ModelWithOptions.Rooms["West Ocean"].Nodes[4].Locks["West Ocean Ship Exit Grey Lock (to Gravity Suit Room)"];
            Assert.True(greyPossibleBypassableLock.LogicallyRelevant);
            Assert.False(greyPossibleBypassableLock.LogicallyNever);
            Assert.False(greyPossibleBypassableLock.LogicallyAlways);
            Assert.False(greyPossibleBypassableLock.LogicallyFree);

            NodeLock freeUnlockLock = ModelWithOptions.Rooms["Morph Ball Room"].Nodes[5].Locks["Blue Brinstar Power Bombs Spawn Lock"];
            Assert.True(freeUnlockLock.LogicallyRelevant);
            Assert.False(freeUnlockLock.LogicallyNever);
            Assert.True(freeUnlockLock.LogicallyAlways);
            Assert.True(freeUnlockLock.LogicallyFree);

            NodeLock possibleUnlockableLock = ModelWithOptions.Rooms["Construction Zone"].Nodes[2].Locks["Construction Zone Red Lock (to Ceiling E-Tank)"];
            Assert.True(possibleUnlockableLock.LogicallyRelevant);
            Assert.False(possibleUnlockableLock.LogicallyNever);
            Assert.False(possibleUnlockableLock.LogicallyAlways);
            Assert.False(possibleUnlockableLock.LogicallyFree);
        }

        #endregion
    }
}
