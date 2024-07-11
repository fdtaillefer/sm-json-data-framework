using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Tests.TestTools
{
    /// <summary>
    /// <para>
    /// A utility class that can be used to assert expected changes in an <see cref="ExecutionResult"/> compared to an initial
    /// <see cref="InGameState"/>, while asserting that everything has not changed.
    /// </para>
    /// <para>
    /// This implicitly expects that the execution was successful (and so its ExecutionResult is not null)
    /// </para>
    /// </summary>
    public class ExecutionResultValidator
    {
        private SuperMetroidModel Model { get; }
        private InGameState InitialInGameState { get; }
        private Strat? ExpectedLastLinkStrat { get;  set; } = null;
        private bool ExpectsNulledLastLinkStrat { get; set; } = false;
        private List<(Runway runwayUsed, Strat stratUsed)> ExpectedRunwaysUsed { get; } = new();
        private List<(CanLeaveCharged canLeavechargedExecuted, Strat stratUsed)> ExpectedCanLeaveChargedExecuted { get; } = new();
        private List<Item> ExpectedItemInvolved { get; } = new();
        private List<GameFlag> ExpectedGameFlagsActivated { get; } = new();
        private List<(NodeLock lockOpened, Strat stratUsed)> ExpectedLocksOpened { get;} = new();
        private List<(NodeLock lockBypassed, Strat stratUsed)> ExpectedLocksBypassed { get; } = new();
        private List<Item> ExpectedDamageReducingItemsInvolved { get; } = new();
        private List<IndividualEnemyKillResult> ExpectedKilledEnemies { get; } = new();
        private ResourceCount ExpectedResourceVariation { get; } = new ResourceCount();
        private ResourceCount ExpectedResourceMaximumVariation { get; } = new ResourceCount();
        private List<RoomNode> ExpectedItemLocationsTaken { get; } = new();
        private ItemInventory ExpectedItemsAdded { get; }
        private Room? ExpectedCurrentRoom { get; set; }
        private RoomNode? ExpectedCurrentNode { get; set; }
        private HashSet<string> ExpectedDestroyedObstacleIds { get; } = new();

        ////////

        public ExecutionResultValidator(SuperMetroidModel model, InGameState initialInGameState)
        {
            Model = model;
            InitialInGameState = initialInGameState;
            ExpectedItemsAdded = new ItemInventory(Model.CreateInitialGameState().BaseResourceMaximums.Clone());
        }

        public ExecutionResultValidator ExpectLastLinkStrat(Strat strat)
        {
            ExpectedLastLinkStrat = strat;
            return this;
        }

        public ExecutionResultValidator ExpectNulledLastLinkStrat(bool expectNulled)
        {
            ExpectsNulledLastLinkStrat = expectNulled;
            return this;
        }

        public ExecutionResultValidator ExpectRunwayUsed(string runwayName, string stratName)
        {
            Runway runway = Model.Runways[runwayName];
            Strat strat = runway.Strats[stratName];
            ExpectedRunwaysUsed.Add((runwayUsed: runway, stratUsed: strat));
            return this;
        }

        public ExecutionResultValidator ExpectCanLeaveChargedExecuted(CanLeaveCharged canLeaveCharged, string stratName)
        {
            Strat strat = canLeaveCharged.Strats[stratName];
            ExpectedCanLeaveChargedExecuted.Add((canLeavechargedExecuted: canLeaveCharged, stratUsed: strat));
            return this;
        }

        public ExecutionResultValidator ExpectItemInvolved(string itemName)
        {
            Item item = Model.Items[itemName];
            ExpectedItemInvolved.Add(item);
            return this;
        }

        public ExecutionResultValidator ExpectDamageReducingItemInvolved(string itemName)
        {
            Item item = Model.Items[itemName];
            ExpectedDamageReducingItemsInvolved.Add(item);
            return this;
        }

        public ExecutionResultValidator ExpectGameFlagActivated(string gameFlagName)
        {
            GameFlag gameFlag = Model.GameFlags[gameFlagName];
            ExpectedGameFlagsActivated.Add(gameFlag);
            return this;
        }

        public ExecutionResultValidator ExpectLockOpened(string lockName, string stratName)
        {
            NodeLock nodeLock = Model.Locks[lockName];
            Strat strat = nodeLock.UnlockStrats[stratName];
            ExpectedLocksOpened.Add((lockOpened: nodeLock, stratUsed: strat));
            return this;
        }

        public ExecutionResultValidator ExpectLockBypassed(string lockName, string stratName)
        {
            NodeLock nodeLock = Model.Locks[lockName];
            Strat strat = nodeLock.UnlockStrats[stratName];
            ExpectedLocksBypassed.Add((lockBypassed: nodeLock, stratUsed: strat));
            return this;
        }

        public ExecutionResultValidator ExpectKilledEnemy(string enemyName, params (string weaponName, int shots)[] killMethod)
        {
            return ExpectKilledEnemy(enemyName, killMethod.ToList());
        }

        public ExecutionResultValidator ExpectKilledEnemy(string enemyName, IEnumerable<(string weaponName, int shots)> killMethodNames)
        {
            Enemy enemy = Model.Enemies[enemyName];
            List<(Weapon weapon, int shots)> killMethod = killMethodNames.Select(pair => (weapon: Model.Weapons[pair.weaponName], shots: pair.shots)).ToList();
            ExpectedKilledEnemies.Add(new IndividualEnemyKillResult(enemy, killMethod));
            return this;
        }

        public ExecutionResultValidator ExpectResourceVariation(RechargeableResourceEnum resource, int variation)
        {
            ExpectedResourceVariation.ApplyAmount(resource, variation);
            return this;
        }

        public ExecutionResultValidator ExpectResourceMaximumVariation(RechargeableResourceEnum resource, int variation)
        {
            ExpectedResourceMaximumVariation.ApplyAmount(resource, variation);
            return this;
        }

        /// <summary>
        /// Adds the expectation that an item location was taken, and also that its item was added to inventory.
        /// </summary>
        /// <param name="roomName">Name of the room the node is in</param>
        /// <param name="nodeIndex">In-room index of the node</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">If the node is not an item location</exception>
        public ExecutionResultValidator ExpectItemLocationTaken(string roomName, int nodeIndex)
        {
            Room room = Model.Rooms[roomName];
            RoomNode node = room.Nodes[nodeIndex];
            if(node.NodeItem == null)
            {
                throw new ArgumentException("Node must have an item");
            }
            ExpectedItemLocationsTaken.Add(node);
            ExpectedItemsAdded.ApplyAddItem(node.NodeItem);
            return this;
        }

        /// <summary>
        /// Adds the expectation that an item was added to inventory, without the expectation that a corresponding item location was taken.
        /// </summary>
        /// <param name="itemName">Name of the item to expect</param>
        /// <returns></returns>
        public ExecutionResultValidator ExpectItemAdded(string itemName)
        {
            Item item = Model.Items[itemName];
            ExpectedItemsAdded.ApplyAddItem(item);
            return this;
        }

        /// <summary>
        /// Adds an expectation that the current room has changed to the room with the provided name.
        /// This also comes with an expectation that the current node has changed, to the one with the provided in-room ID in the new room.
        /// </summary>
        /// <param name="roomName">Name of the new expected current room</param>
        /// <param name="nodeId">In-room ID (within the new room) of the new expected current node</param>
        /// <returns></returns>
        public ExecutionResultValidator ExpectCurrentRoom(string roomName, int nodeId)
        {
            ExpectedCurrentRoom = Model.Rooms[roomName];
            ExpectedCurrentNode = ExpectedCurrentRoom.Nodes[nodeId];
            return this;
        }

        /// <summary>
        /// Adds an expectation that the current node has changed to the node with the provided in-room ID of the initial room.
        /// </summary>
        /// <param name="nodeId">In-room ID of the expected new current node</param>
        /// <returns></returns>
        public ExecutionResultValidator ExpectCurrentNode(int nodeId)
        {
            ExpectedCurrentNode = InitialInGameState.CurrentRoom.Nodes[nodeId];
            return this;
        }

        /// <summary>
        /// Adds an expectation that the obstacle with the provided in-room ID was destroyed, in the initial room.
        /// This will not be checked if the room is expected to have changed.
        /// </summary>
        /// <param name="obstacleId"></param>
        /// <returns></returns>
        public ExecutionResultValidator ExpectDestroyedObstacle(string obstacleId)
        {
            // We don't use the obstacle, but we can obtain is as a sanity check
            RoomObstacle roomObstacle = InitialInGameState.CurrentRoom.Obstacles[obstacleId];
            ExpectedDestroyedObstacleIds.Add(obstacleId);
            return this;
        }

        /// <summary>
        /// Asserts that the provided ExecutionResult contains all the changes, and only the changes, that are held in this validator.
        /// </summary>
        /// <param name="result">ExecutionResult whose state to assert</param>
        public void AssertRespectedBy(ExecutionResult result)
        {
            // Expect execution success
            Assert.NotNull(result);

            // Runways used
            Assert.Equal(ExpectedRunwaysUsed.Count, result.RunwaysUsed.Count);
            foreach((Runway expectedRunway, Strat expectedStrat) in ExpectedRunwaysUsed)
            {
                Assert.Contains(expectedRunway.Name, result.RunwaysUsed.Keys);
                (Runway actualRunway, Strat actualStrat) = result.RunwaysUsed[expectedRunway.Name];
                Assert.Same(expectedRunway, actualRunway);
                Assert.Same(expectedStrat, actualStrat);
            }

            // CanLeaveCharged executed
            Assert.Equal(ExpectedCanLeaveChargedExecuted.Count, result.CanLeaveChargedExecuted.Count);
            foreach ((CanLeaveCharged expectedCanLeaveCharged, Strat expectedStrat) in ExpectedCanLeaveChargedExecuted)
            {
                (CanLeaveCharged actualCanLeaveCharged, Strat actualStrat)=
                    result.CanLeaveChargedExecuted.Where(canLeaveChargedExecuted => canLeaveChargedExecuted.canLeaveChargedUsed == expectedCanLeaveCharged).First();

                Assert.Same(expectedStrat, actualStrat);
            }

            // Items involved
            Assert.Equal(ExpectedItemInvolved.Count, result.ItemsInvolved.Count);
            foreach(Item expectedItem in ExpectedItemInvolved)
            {
                Assert.Same(expectedItem, result.ItemsInvolved[expectedItem.Name]);
            }

            // Damage-reducing Items involved
            Assert.Equal(ExpectedDamageReducingItemsInvolved.Count, result.DamageReducingItemsInvolved.Count);
            foreach (Item expectedItem in ExpectedDamageReducingItemsInvolved)
            {
                Assert.Same(expectedItem, result.DamageReducingItemsInvolved[expectedItem.Name]);
            }

            // GameFlags activated
            Assert.Equal(ExpectedGameFlagsActivated.Count, result.ActivatedGameFlags.Count);
            foreach(GameFlag expectedGameFlag in ExpectedGameFlagsActivated)
            {
                Assert.Same(expectedGameFlag, result.ActivatedGameFlags[expectedGameFlag.Name]);
            }
            Assert.Equal(InitialInGameState.ActiveGameFlags.Count + ExpectedGameFlagsActivated.Count, result.ResultingState.ActiveGameFlags.Count);
            foreach (GameFlag expectedGameFlag in ExpectedGameFlagsActivated)
            {
                Assert.Same(expectedGameFlag, result.ResultingState.ActiveGameFlags[expectedGameFlag.Name]);
            }

            // NodeLocks opened
            Assert.Equal(ExpectedLocksOpened.Count, result.OpenedLocks.Count);
            foreach ((NodeLock expectedLock, Strat expectedStrat) in ExpectedLocksOpened)
            {
                NodeLock actualLock = result.OpenedLocks[expectedLock.Name].openedLock;
                Strat actualStrat = result.OpenedLocks[expectedLock.Name].stratUsed;
                Assert.Same(expectedLock, actualLock);
                Assert.Same(expectedStrat, actualStrat);
            }
            Assert.Equal(InitialInGameState.OpenedLocks.Count + ExpectedLocksOpened.Count, result.ResultingState.OpenedLocks.Count);
            foreach ((NodeLock expectedLock, _) in ExpectedLocksOpened)
            {
                Assert.Same(expectedLock, result.ResultingState.OpenedLocks[expectedLock.Name]);
            }

            // NodeLocks Bypassed
            Assert.Equal(ExpectedLocksBypassed.Count, result.BypassedLocks.Count);
            foreach ((NodeLock expectedLock, Strat expectedStrat) in ExpectedLocksBypassed)
            {
                NodeLock actualLock = result.BypassedLocks[expectedLock.Name].bypassedLock;
                Strat actualStrat = result.BypassedLocks[expectedLock.Name].stratUsed;
                Assert.Same(expectedLock, actualLock);
                Assert.Same(expectedStrat, actualStrat);
            }

            // KilledEnemies
            List<IndividualEnemyKillResult> remainingKilledEnemies = new(result.KilledEnemies);
            Assert.Equal(ExpectedKilledEnemies.Count, remainingKilledEnemies.Count);
            foreach(IndividualEnemyKillResult expectedEnemyKillResult in ExpectedKilledEnemies)
            {
                int index = remainingKilledEnemies.IndexOf(expectedEnemyKillResult);
                Assert.NotEqual(-1, index);
                remainingKilledEnemies.RemoveAt(index);
            }

            // LastLinkStrat
            if (ExpectedLastLinkStrat != null)
            {
                Assert.Same(ExpectedLastLinkStrat, result.ResultingState.LastLinkStrat);
            }
            else if (ExpectsNulledLastLinkStrat)
            {
                Assert.Null(result.ResultingState.LastLinkStrat);
            }
            else
            {
                Assert.Same(InitialInGameState.LastLinkStrat, result.ResultingState.LastLinkStrat);
            }

            // Resource variation
            ResourceCount actualResourceVariation = result.ResultingState.GetResourceVariationWith(InitialInGameState);
            Assert.Equal(ExpectedResourceVariation, actualResourceVariation);

            // Resource maximum variation
            ResourceCount actualResourceMaximumVariation = result.ResultingState.ResourceMaximums.GetVariationWith(InitialInGameState.ResourceMaximums);
            Assert.Equal(ExpectedResourceMaximumVariation, actualResourceMaximumVariation);

            // Item locations taken
            Assert.Equal(InitialInGameState.TakenItemLocations.Count + ExpectedItemLocationsTaken.Count, result.ResultingState.TakenItemLocations.Count);
            foreach (RoomNode expectedItemLocation in ExpectedItemLocationsTaken)
            {
                Assert.Same(expectedItemLocation, result.ResultingState.TakenItemLocations[expectedItemLocation.Name]);
            }

            // Items added (directly or via a taken location
            ItemInventory addedItems = result.ResultingState.Inventory.ExceptIn(InitialInGameState.Inventory);
            Assert.True(ExpectedItemsAdded.ExceptIn(addedItems).Empty);
            Assert.True(addedItems.ExceptIn(ExpectedItemsAdded).Empty);

            // Destroyed obstacles
            if (ExpectedCurrentRoom == null)
            {
                Assert.Equal(InitialInGameState.InRoomState.DestroyedObstacleIds.Count + ExpectedDestroyedObstacleIds.Count,
                    result.ResultingState.InRoomState.DestroyedObstacleIds.Count);
                foreach(string expectedObstacleId in ExpectedDestroyedObstacleIds)
                {
                    Assert.Contains(expectedObstacleId, result.ResultingState.InRoomState.DestroyedObstacleIds);
                }
                foreach (string expectedObstacleId in InitialInGameState.InRoomState.DestroyedObstacleIds)
                {
                    Assert.Contains(expectedObstacleId, result.ResultingState.InRoomState.DestroyedObstacleIds);
                }
            }

            // Current Node
            Assert.Same(ExpectedCurrentNode ?? InitialInGameState.CurrentNode, result.ResultingState.CurrentNode);

            // Current Room
            Assert.Same(ExpectedCurrentRoom ?? InitialInGameState.CurrentRoom, result.ResultingState.CurrentRoom);
        }
    }
}
