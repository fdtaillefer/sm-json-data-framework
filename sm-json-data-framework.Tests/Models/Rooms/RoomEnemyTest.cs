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
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            RoomEnemy roomEnemy = Model.RoomEnemies["Early Supers Zeb"];
            Assert.Equal("e1", roomEnemy.Id);
            Assert.Equal("Early Supers Zeb", roomEnemy.GroupName);
            Assert.Same(Model.Enemies["Zeb"], roomEnemy.Enemy);
            Assert.Equal(1, roomEnemy.Quantity);
            Assert.Same(Model.Rooms["Early Supers Room"], roomEnemy.Room);

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


            RoomEnemy betweenNodesRoomEnemy = Model.RoomEnemies["Waterway Puyos"];
            Assert.Empty(betweenNodesRoomEnemy.HomeNodes);
            Assert.Equal(2, betweenNodesRoomEnemy.BetweenNodes.Count);
            Assert.Contains(1, betweenNodesRoomEnemy.BetweenNodes.Keys);
            Assert.Contains(2, betweenNodesRoomEnemy.BetweenNodes.Keys);
            Assert.Empty(betweenNodesRoomEnemy.FarmCycles);
            Assert.False(betweenNodesRoomEnemy.IsSpawner);

            RoomEnemy roomEnemyWithSpawnConditions = Model.RoomEnemies["Alcatraz Ripper"];
            Assert.Equal(1, roomEnemyWithSpawnConditions.Spawn.LogicalElements.Count);
            Assert.NotNull(roomEnemyWithSpawnConditions.Spawn.LogicalElement<GameFlagLogicalElement>(0));
            Assert.Equal(1, roomEnemyWithSpawnConditions.StopSpawn.LogicalElements.Count);
            Assert.NotNull(roomEnemyWithSpawnConditions.StopSpawn.LogicalElement<GameFlagLogicalElement>(0));

            RoomEnemy roomEnemyWithDropRequirements= Model.RoomEnemies["Fast Pillars Standing Pirates"];
            Assert.Equal(1, roomEnemyWithDropRequirements.DropRequires.LogicalElements.Count);
            Assert.NotNull(roomEnemyWithDropRequirements.DropRequires.LogicalElement<HelperLogicalElement>(0));
        }

        #endregion

        #region Tests for Spawns()

        [Fact]
        public void Spawns_SpawnConditionNotMet_ReturnsFalse()
        {
            // Given
            RoomEnemy roomEnemy = Model.RoomEnemies["Alcatraz Ripper"];
            InGameState inGameState = Model.CreateInitialGameState();

            // When
            bool result = roomEnemy.Spawns(Model, inGameState);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void Spawns_SpawnConditionMetAndStopSpawnNotMet_ReturnsTrue()
        {
            // Given
            RoomEnemy roomEnemy = Model.RoomEnemies["Alcatraz Ripper"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddGameFlag(Model.GameFlags["f_ZebesAwake"]);

            // When
            bool result = roomEnemy.Spawns(Model, inGameState);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void Spawns_SpawnAndStopSpawnConditionMet_ReturnsFalse()
        {
            // Given
            RoomEnemy roomEnemy = Model.RoomEnemies["Alcatraz Ripper"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddGameFlag(Model.GameFlags["f_ZebesAwake"])
                .ApplyAddGameFlag(Model.GameFlags["f_ZebesSetAblaze"]);

            // When
            bool result = roomEnemy.Spawns(Model, inGameState);

            // Expect
            Assert.False(result);
        }

        #endregion

        #region Tests for SpawnerFarmExecution.Execute()

        [Fact]
        public void SpawnFarmExecutionExecute_NotSpawner_Fails()
        {
            // Given
            RoomEnemy roomEnemy = Model.RoomEnemies["Green Shaft Top Zeelas"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(roomEnemy.HomeNodes[12]);

            // When
            ExecutionResult result = roomEnemy.SpawnerFarmExecution.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void SpawnFarmExecutionExecute_FarmPossible_RefillsFarmableResources()
        {
            // Given
            RoomEnemy roomEnemy = Model.RoomEnemies["Etecoon E-Tank Left Zebbo"];
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(Model.Items[SuperMetroidModel.MISSILE_NAME])
                .ApplyAddItem(Model.Items[SuperMetroidModel.SUPER_NAME])
                .ApplyAddItem(Model.Items[SuperMetroidModel.POWER_BOMB_NAME])
                .ApplyAddItem(Model.Items[SuperMetroidModel.RESERVE_TANK_NAME])
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Missile, 4)
                .ApplyConsumeResource(ConsumableResourceEnum.Super, 4)
                .ApplyConsumeResource(ConsumableResourceEnum.PowerBomb, 4)
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 150)
                .ApplyEnterRoom(roomEnemy.HomeNodes[4]);

            // When
            ExecutionResult result = roomEnemy.SpawnerFarmExecution.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
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
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canSuitlessMaridia");
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);
            RoomEnemy roomEnemy = ModelWithOptions.RoomEnemies["Botwoon E-Tank Zoas"];
            InGameState inGameState = ModelWithOptions.CreateInitialGameState()
                .ApplyEnterRoom(roomEnemy.HomeNodes[4]);

            // When
            ExecutionResult result = roomEnemy.SpawnerFarmExecution.Execute(ModelWithOptions, inGameState);

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
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Bombs");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions)
                .StartingGameFlags(new List<GameFlag> { ModelWithOptions.GameFlags["f_DefeatedRidley"] })
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            RoomEnemy alwaysSpawns = ModelWithOptions.RoomEnemies["Post Crocomire Farming Room Ripper 2"];
            Assert.True(alwaysSpawns.LogicallyRelevant);
            Assert.True(alwaysSpawns.LogicallyAlwaysSpawns);
            Assert.False(alwaysSpawns.LogicallyNeverSpawns);

            RoomEnemy neverMeetsSpawnConditions = ModelWithOptions.RoomEnemies["Bomb Torizo"];
            Assert.False(neverMeetsSpawnConditions.LogicallyRelevant);
            Assert.False(neverMeetsSpawnConditions.LogicallyAlwaysSpawns);
            Assert.True(neverMeetsSpawnConditions.LogicallyNeverSpawns);

            RoomEnemy alwaysMeetsStopSpawnConditions = ModelWithOptions.RoomEnemies["Ridley"];
            Assert.False(alwaysMeetsStopSpawnConditions.LogicallyRelevant);
            Assert.False(alwaysMeetsStopSpawnConditions.LogicallyAlwaysSpawns);
            Assert.True(alwaysMeetsStopSpawnConditions.LogicallyNeverSpawns);

            RoomEnemy notAlwaysSpawnConditions = ModelWithOptions.RoomEnemies["Attic Atomics"];
            Assert.True(notAlwaysSpawnConditions.LogicallyRelevant);
            Assert.False(notAlwaysSpawnConditions.LogicallyAlwaysSpawns);
            Assert.False(notAlwaysSpawnConditions.LogicallyNeverSpawns);

            RoomEnemy alwaysSpawnNotAlwaysStopSpawnConditions = ModelWithOptions.RoomEnemies["Flyway Mellows"];
            Assert.True(alwaysSpawnNotAlwaysStopSpawnConditions.LogicallyRelevant);
            Assert.False(alwaysSpawnNotAlwaysStopSpawnConditions.LogicallyAlwaysSpawns);
            Assert.False(alwaysSpawnNotAlwaysStopSpawnConditions.LogicallyNeverSpawns);
        }

        #endregion
    }
}
