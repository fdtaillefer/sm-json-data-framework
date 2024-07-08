using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;

namespace sm_json_data_framework.Tests.Models.Rooms
{
    public class RoomEnemyTest
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
            RoomEnemy roomEnemy = model.RoomEnemies["Early Supers Zeb"];
            Assert.Equal("e1", roomEnemy.Id);
            Assert.Equal("Early Supers Zeb", roomEnemy.GroupName);
            Assert.Same(model.Enemies["Zeb"], roomEnemy.Enemy);
            Assert.Equal(1, roomEnemy.Quantity);
            Assert.Same(model.Rooms["Early Supers Room"], roomEnemy.Room);

            Assert.Equal(1, roomEnemy.HomeNodes.Count);
            Assert.Contains(1, roomEnemy.HomeNodes.Keys);
            Assert.Empty(roomEnemy.BetweenNodes);

            Assert.NotNull(roomEnemy.Spawn);
            Assert.Empty(roomEnemy.Spawn.LogicalElements);

            Assert.NotNull(roomEnemy.StopSpawn);
            Assert.Equal(1, roomEnemy.StopSpawn.LogicalElements.Count);
            Assert.NotNull(roomEnemy.StopSpawn.LogicalElement<NeverLogicalElement>(0));

            Assert.NotNull(roomEnemy.DropRequires);
            Assert.Empty(roomEnemy.DropRequires.LogicalElements);

            Assert.Equal(1, roomEnemy.FarmCycles.Count);
            Assert.Contains("Crouch over spawn point", roomEnemy.FarmCycles.Keys);
            Assert.True(roomEnemy.IsSpawner);


            RoomEnemy betweenNodesRoomEnemy = model.RoomEnemies["Waterway Puyos"];
            Assert.Empty(betweenNodesRoomEnemy.HomeNodes);
            Assert.Equal(2, betweenNodesRoomEnemy.BetweenNodes.Count);
            Assert.Contains(1, betweenNodesRoomEnemy.BetweenNodes.Keys);
            Assert.Contains(2, betweenNodesRoomEnemy.BetweenNodes.Keys);
            Assert.Empty(betweenNodesRoomEnemy.FarmCycles);
            Assert.False(betweenNodesRoomEnemy.IsSpawner);

            RoomEnemy roomEnemyWithSpawnConditions = model.RoomEnemies["Alcatraz Ripper"];
            Assert.Equal(1, roomEnemyWithSpawnConditions.Spawn.LogicalElements.Count);
            Assert.NotNull(roomEnemyWithSpawnConditions.Spawn.LogicalElement<GameFlagLogicalElement>(0));
            Assert.Equal(1, roomEnemyWithSpawnConditions.StopSpawn.LogicalElements.Count);
            Assert.NotNull(roomEnemyWithSpawnConditions.StopSpawn.LogicalElement<GameFlagLogicalElement>(0));

            RoomEnemy roomEnemyWithDropRequirements= model.RoomEnemies["Fast Pillars Standing Pirates"];
            Assert.Equal(1, roomEnemyWithDropRequirements.DropRequires.LogicalElements.Count);
            Assert.NotNull(roomEnemyWithDropRequirements.DropRequires.LogicalElement<HelperLogicalElement>(0));
        }

        #endregion

        #region Tests for Spawns()

        [Fact]
        public void Spawns_SpawnConditionNotMet_ReturnsFalse()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomEnemy roomEnemy = model.RoomEnemies["Alcatraz Ripper"];
            InGameState inGameState = model.CreateInitialGameState();

            // When
            bool result = roomEnemy.Spawns(model, inGameState);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void Spawns_SpawnConditionMetAndStopSpawnNotMet_ReturnsTrue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomEnemy roomEnemy = model.RoomEnemies["Alcatraz Ripper"];
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddGameFlag(model.GameFlags["f_ZebesAwake"]);

            // When
            bool result = roomEnemy.Spawns(model, inGameState);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void Spawns_SpawnAndStopSpawnConditionMet_ReturnsFalse()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomEnemy roomEnemy = model.RoomEnemies["Alcatraz Ripper"];
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddGameFlag(model.GameFlags["f_ZebesAwake"])
                .ApplyAddGameFlag(model.GameFlags["f_ZebesSetAblaze"]);

            // When
            bool result = roomEnemy.Spawns(model, inGameState);

            // Expect
            Assert.False(result);
        }

        #endregion

        #region Tests for SpawnerFarmExecution.Execute()

        [Fact]
        public void SpawnFarmExecutionExecute_NotSpawner_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomEnemy roomEnemy = model.RoomEnemies["Green Shaft Top Zeelas"];
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(roomEnemy.HomeNodes[12]);

            // When
            ExecutionResult result = roomEnemy.SpawnerFarmExecution.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void SpawnFarmExecutionExecute_FarmPossible_RefillsFarmableResources()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomEnemy roomEnemy = model.RoomEnemies["Etecoon E-Tank Left Zebbo"];
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyAddItem(SuperMetroidModel.SUPER_NAME)
                .ApplyAddItem(SuperMetroidModel.POWER_BOMB_NAME)
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Missile, 4)
                .ApplyConsumeResource(ConsumableResourceEnum.Super, 4)
                .ApplyConsumeResource(ConsumableResourceEnum.PowerBomb, 4)
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 150)
                .ApplyEnterRoom(roomEnemy.BetweenNodes[4]);

            // When
            ExecutionResult result = roomEnemy.SpawnerFarmExecution.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, 98)
                .ExpectResourceVariation(RechargeableResourceEnum.ReserveEnergy, 52)
                .ExpectResourceVariation(RechargeableResourceEnum.Super, 4)
                // Missile drop rate is horrendous, but it becomes super high once energy is filled so it becomes farmable
                .ExpectResourceVariation(RechargeableResourceEnum.Missile, 4)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void SpawnFarmExecutionExecute_FarmNotPossible_Fails()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canSuitlessMaridia");
            model.ApplyLogicalOptions(logicalOptions);
            RoomEnemy roomEnemy = model.RoomEnemies["Botwoon E-Tank Zoas"];
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(roomEnemy.HomeNodes[4]);

            // When
            ExecutionResult result = roomEnemy.SpawnerFarmExecution.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        // There's no spawners that have any spawn conditions so we can't test for that...

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Bombs");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingGameFlags(new List<string> { "f_DefeatedRidley" })
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomEnemy alwaysSpawns = model.RoomEnemies["Post Crocomire Farming Room Ripper 2"];
            Assert.True(alwaysSpawns.LogicallyRelevant);
            Assert.True(alwaysSpawns.LogicallyAlwaysSpawns);
            Assert.False(alwaysSpawns.LogicallyNeverSpawns);

            RoomEnemy neverMeetsSpawnConditions = model.RoomEnemies["Bomb Torizo"];
            Assert.False(neverMeetsSpawnConditions.LogicallyRelevant);
            Assert.False(neverMeetsSpawnConditions.LogicallyAlwaysSpawns);
            Assert.True(neverMeetsSpawnConditions.LogicallyNeverSpawns);

            RoomEnemy alwaysMeetsStopSpawnConditions = model.RoomEnemies["Ridley"];
            Assert.False(alwaysMeetsStopSpawnConditions.LogicallyRelevant);
            Assert.False(alwaysMeetsStopSpawnConditions.LogicallyAlwaysSpawns);
            Assert.True(alwaysMeetsStopSpawnConditions.LogicallyNeverSpawns);

            RoomEnemy notAlwaysSpawnConditions = model.RoomEnemies["Attic Atomics"];
            Assert.True(notAlwaysSpawnConditions.LogicallyRelevant);
            Assert.False(notAlwaysSpawnConditions.LogicallyAlwaysSpawns);
            Assert.False(notAlwaysSpawnConditions.LogicallyNeverSpawns);

            RoomEnemy alwaysSpawnNotAlwaysStopSpawnConditions = model.RoomEnemies["Flyway Mellows"];
            Assert.True(alwaysSpawnNotAlwaysStopSpawnConditions.LogicallyRelevant);
            Assert.False(alwaysSpawnNotAlwaysStopSpawnConditions.LogicallyAlwaysSpawns);
            Assert.False(alwaysSpawnNotAlwaysStopSpawnConditions.LogicallyNeverSpawns);
        }

        #endregion
    }
}
