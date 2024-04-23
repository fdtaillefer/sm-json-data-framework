using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class RoomEnemy : InitializablePostDeserializeInRoom
    {
        public string Id { get; set; }

        public string GroupName { get; set; }

        public string EnemyName { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The actual Enemy this RoomEnemy represents.</para>
        /// </summary>
        [JsonIgnore]
        public Enemy Enemy { get; set; }

        public int Quantity { get; set; }

        [JsonPropertyName("homeNodes")]
        public IEnumerable<int> HomeNodeIds { get; set; } = Enumerable.Empty<int>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The nodes in which this enemy roams, mapped by their node ID. Mutually-exclusive with <see cref="BetweenNodes"/>.</para>
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, RoomNode> HomeNodes { get; set; }

        [JsonPropertyName("betweenNodes")]
        public IEnumerable<int> BetweenNodeIds { get; set; } = Enumerable.Empty<int>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>Contains two nodes between which this enemy roams (without ever actually being in either), mapped by their node ID. Mutually-exclusive with <see cref="HomeNodes"/>.</para>
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, RoomNode> BetweenNodes { get; set; }

        public LogicalRequirements Spawn { get; set; } = new LogicalRequirements();

        // Empty requirements default to "always" behavior, but enemies with no StopSpawn behavior never stop spawning.
        // Make the default state never instead.
        public LogicalRequirements StopSpawn { get; set; } = LogicalRequirements.Never();

        public LogicalRequirements DropRequires { get; set; } = new LogicalRequirements();

        public IEnumerable<FarmCycle> FarmCycles { get; set; } = Enumerable.Empty<FarmCycle>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The Room in which this enemy group is.</para>
        /// </summary>
        [JsonIgnore]
        public Room Room { get; set; }

        [JsonIgnore]
        public bool IsSpawner { get => FarmCycles.Any(); }

        public IEnumerable<Action> Initialize(SuperMetroidModel model, Room room)
        {
            List<Action> postInitializeActions = new List<Action>();

            Room = room;

            Enemy = model.Enemies[EnemyName];

            HomeNodes = HomeNodeIds.Select(id => room.Nodes[id]).ToDictionary(n => n.Id);

            BetweenNodes = BetweenNodeIds.Select(id => room.Nodes[id]).ToDictionary(n => n.Id);

            model.RoomEnemies.Add(GroupName, this);

            foreach (var farmCycle in FarmCycles)
            {
                postInitializeActions.AddRange(farmCycle.Initialize(model, room, this));
            }

            return postInitializeActions;
        }

        public void InitializeForeignProperties(SuperMetroidModel model, Room room)
        {
            Room = room;
            Enemy = model.Enemies[EnemyName];
            HomeNodes = HomeNodeIds.Select(id => room.Nodes[id]).ToDictionary(n => n.Id);
            BetweenNodes = BetweenNodeIds.Select(id => room.Nodes[id]).ToDictionary(n => n.Id);

            foreach (var farmCycle in FarmCycles)
            {
                farmCycle.InitializeForeignProperties(model, room, this);
            }
        }

        public void InitializeOtherProperties(SuperMetroidModel model, Room room)
        {
            model.RoomEnemies.Add(GroupName, this);

            foreach (var farmCycle in FarmCycles)
            {
                farmCycle.InitializeOtherProperties(model, room, this);
            }
        }

        public bool CleanUpUselessValues(SuperMetroidModel model, Room room)
        {
            FarmCycles = FarmCycles.Where(farmCycle => farmCycle.CleanUpUselessValues(model, room, this));

            // A room enemy is never useless
            return true;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Spawn.InitializeReferencedLogicalElementProperties(model, room));

            unhandled.AddRange(StopSpawn.InitializeReferencedLogicalElementProperties(model, room));

            unhandled.AddRange(DropRequires.InitializeReferencedLogicalElementProperties(model, room));

            foreach(var farmCycle in FarmCycles)
            {
                unhandled.AddRange(farmCycle.InitializeReferencedLogicalElementProperties(model, room, this));
            }

            return unhandled.Distinct();
        }

        /// <summary>
        /// Indicates whether this room enemy will spawn, given the provided model and inGameState.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to use to check. This will NOT be altered by this method.</param>
        /// <returns></returns>
        public bool Spawns(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            // If spawn conditions for this room enemy aren't met, it's impossible for the enemy to spawn
            if (Spawn.Execute(model, inGameState) == null)
            {
                return false;
            }

            // If conditions for this room enemy to stop spawning have been met, it's impossible for the enemy to spawn
            if (StopSpawn.Execute(model, inGameState) != null)
            {
                return false;
            }

            return true;
        }

        IExecutable _spawnerFarmExecution = null;
        /// <summary>
        /// An IExecutable that corresponds to farming this group of enemies, by camping its spawner(s).
        /// </summary>
        public IExecutable SpawnerFarmExecution
        {
            get
            {
                if (_spawnerFarmExecution == null)
                {
                    _spawnerFarmExecution = new SpawnerFarmExecution(this);
                }
                return _spawnerFarmExecution;
            }
        }
    }

    /// <summary>
    /// A class that encloses the farming of a RoomEnemy in an IExecutable interface.
    /// </summary>
    internal class SpawnerFarmExecution : IExecutable
    {
        private RoomEnemy RoomEnemy { get; set; }

        public SpawnerFarmExecution(RoomEnemy roomEnemy)
        {
            RoomEnemy = roomEnemy;
        }
        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // The enemy can only be farmed if it currently spawns
            if (!RoomEnemy.Spawns(model, inGameState))
            {
                return null;
            }

            (FarmCycle bestCycle, ExecutionResult result) bestResult = (null, null);
            InGameStateComparer comparer = model.GetInGameStateComparer();

            // Order farm cycles, with the shortest execution going first.
            // We'll execute them all in order, remembering the one that refilled the most resources.
            // We'll stop as soon as we find one that we can execute and which costs no resources. 
            // Then we'll return the best result.
            IEnumerable<FarmCycle> orderedFarmCycles = RoomEnemy.FarmCycles.OrderBy(cycle => cycle.CycleFrames);
            foreach(FarmCycle currentFarmCycle in orderedFarmCycles)
            {
                var currentFarmResult = currentFarmCycle.FarmExecution.Execute(model, inGameState, times: times, previousRoomCount: previousRoomCount);
                
                // If the farming succeeded, evaluate the results.
                // Otherwise, just skip to the next cycle.
                if (currentFarmResult != null)
                {
                    // If this farm cycle was cost free, we won't find a better one later on.
                    // Immediately return the best result we've encountered so far.
                    if(currentFarmCycle.IsFree(model, inGameState, previousRoomCount: previousRoomCount))
                    {
                        // If the resulting state is the best we've found yet, retain it
                        if (bestResult.result == null
                            || comparer.Compare(currentFarmResult.ResultingState, bestResult.result.ResultingState) > 0)
                        {
                            bestResult = (currentFarmCycle, currentFarmResult);
                        }

                        return bestResult.result;
                    }
                    else
                    {
                        // If the resulting state is the best we've found yet, retain it
                        if (bestResult.result == null
                            || comparer.Compare(currentFarmResult.ResultingState, bestResult.result.ResultingState) > 0)
                        {
                            bestResult = (currentFarmCycle, currentFarmResult);
                        }

                        // But because this farm cycle was not free, it's possible the next one has better results.
                        // So don't return yet.
                    }
                } // End if farming the cycle succeeded
            }// Done iterating over farm cycles

            // If we haven't returned while iterating, return the best result we've found
            // If we've found none, this will return null which is a failure.
            // Notably, this happens for any room enemy that doesn't have farm cycles, i.e. which offers no possibility of farming spawners.
            return bestResult.result;
        }
    }
}
