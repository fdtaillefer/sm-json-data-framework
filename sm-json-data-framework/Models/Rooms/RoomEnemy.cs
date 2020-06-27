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

        public LogicalRequirements StopSpawn { get; set; } = new LogicalRequirements();

        public LogicalRequirements DropRequires { get; set; } = new LogicalRequirements();

        public IEnumerable<FarmCycle> FarmCycles { get; set; } = Enumerable.Empty<FarmCycle>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The Room in which this enemy group is.</para>
        /// </summary>
        [JsonIgnore]
        public Room Room { get; set; }

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
        public ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            (FarmCycle bestCycle, ExecutionResult result) bestResult = (null, null);
            InGameStateComparer comparer = model.GetInGameStateComparer();

            // Order farm cycles, with the shortest execution going first.
            // We'll execute them all in order, remembering the one that refilled the most resources.
            // We'll stop as soon as we find one that we can execute and which costs no resources. 
            // Then we'll return the best result.
            IEnumerable<FarmCycle> orderedFarmCycles = RoomEnemy.FarmCycles.OrderBy(cycle => cycle.CycleFrames);
            foreach(FarmCycle currentFarmCycle in orderedFarmCycles)
            {
                var currentFarmResult = currentFarmCycle.FarmExecution.Execute(model, inGameState, times: times, usePreviousRoom: usePreviousRoom);
                
                // If the farming succeeded, evaluate the results.
                // Otherwise, just skip to the next cycle.
                if (currentFarmResult != null)
                {
                    // If this farm cycle was cost free, we won't find a better one later on.
                    // Immediately return the best result we've encountered so far.
                    if(currentFarmCycle.IsFree(model, inGameState, usePreviousRoom: usePreviousRoom))
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
            // If we've found none, this will return null which is a failure
            return bestResult.result;
        }
    }
}
