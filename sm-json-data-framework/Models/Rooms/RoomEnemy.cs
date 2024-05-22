using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Raw.Requirements;
using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    /// <summary>
    /// Represents the presence of a given number of a given enemy somewhere in a room.
    /// </summary>
    public class RoomEnemy : AbstractModelElement<UnfinalizedRoomEnemy, RoomEnemy>
    {
        public RoomEnemy(UnfinalizedRoomEnemy innerElement, Action<RoomEnemy> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            Id = innerElement.Id;
            GroupName = innerElement.GroupName;
            Quantity = innerElement.Quantity;
            Enemy = innerElement.Enemy.Finalize(mappings);
            HomeNodes = innerElement.HomeNodes.Values.Select(node => node.Finalize(mappings)).ToDictionary(node => node.Id).AsReadOnly();
            BetweenNodes = innerElement.BetweenNodes.Values.Select(node => node.Finalize(mappings)).ToDictionary(node => node.Id).AsReadOnly();
            Spawn = innerElement.Spawn.Finalize(mappings);
            StopSpawn = innerElement.StopSpawn.Finalize(mappings);
            DropRequires = innerElement.DropRequires.Finalize(mappings);
            FarmCycles = innerElement.FarmCycles.Values.Select(farmCycle => farmCycle.Finalize(mappings)).ToDictionary(farmCycle => farmCycle.Name).AsReadOnly();
            Room = innerElement.Room.Finalize(mappings);
        }

        /// <summary>
        /// A short identifier for this RoomEnemy that is only unique within the room.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// A name for this roomEnemy, that is unique across the entire model.
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// The actual Enemy this RoomEnemy represents a number of.
        /// </summary>
        public Enemy Enemy { get; }

        /// <summary>
        /// The number of enemies represented by this RoomEnemy.
        /// </summary>
        public int Quantity { get; }

        /// <summary>
        /// The nodes in which this enemy roams, mapped by their node ID. Mutually-exclusive with <see cref="BetweenNodes"/>.
        /// </summary>
        public IReadOnlyDictionary<int, RoomNode> HomeNodes { get; }

        /// <summary>
        /// Contains two nodes between which this enemy roams (without ever actually being truly in either), mapped by their node ID. Mutually-exclusive with <see cref="HomeNodes"/>.
        /// </summary>
        public IReadOnlyDictionary<int, RoomNode> BetweenNodes { get; }

        /// <summary>
        /// LogicalRequirements that must be fulfilled before this RoomEnemy can spawn when the room is entered.
        /// </summary>
        public LogicalRequirements Spawn { get; }

        /// <summary>
        /// LogicalRequirements that, once fulfilled, prevent this RoomEnemy from spawning.
        /// </summary>
        public LogicalRequirements StopSpawn { get; }

        /// <summary>
        /// LogicalRequirements that must be fulfilled in order to obtain this RoomEnemy's drops without taking damage.
        /// </summary>
        public LogicalRequirements DropRequires { get; }

        /// <summary>
        /// Different ways this room enemy can be farmed if it's a spawner, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, FarmCycle> FarmCycles { get; }

        /// <summary>
        /// The Room in which this RoomEnemy is.
        /// </summary>
        public Room Room { get; }

        /// <summary>
        /// Whether this RoomEnemy is an enemy spawner, causing new enemies to spawn to replace those that are killed.
        /// </summary>
        public bool IsSpawner { get => FarmCycles.Any(); }

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

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            foreach (FarmCycle farmCycle in FarmCycles.Values)
            {
                farmCycle.ApplyLogicalOptions(logicalOptions);
            }

            Spawn.ApplyLogicalOptions(logicalOptions);
            StopSpawn.ApplyLogicalOptions(logicalOptions);
            DropRequires.ApplyLogicalOptions(logicalOptions);
        }

        protected override void UpdateLogicalProperties()
        {
            base.UpdateLogicalProperties();
            LogicallyNeverSpawns = CalculateLogicallyNeverSpawns();
            LogicallyAlwaysSpawns = CalculateLogicallyAlwaysSpawns();
        }

        public override bool CalculateLogicallyRelevant()
        {
            // There's nothing that can make a room enemy irrelevant
            return true;
        }

        /// <summary>
        /// If true, then this enemy never ever spawns given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyNeverSpawns { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNeverSpawns"/> should currently be.
        /// </summary>
        /// <returns></returns>
        protected bool CalculateLogicallyNeverSpawns()
        {
            return Spawn.LogicallyNever;
        }

        /// <summary>
        /// If true, then this enemy always spawns given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyAlwaysSpawns { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlwaysSpawns"/> should currently be.
        /// </summary>
        /// <returns></returns>
        protected bool CalculateLogicallyAlwaysSpawns()
        {
            return Spawn.LogicallyFree;
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
            InGameStateComparer comparer = model.InGameStateComparer;

            // Order farm cycles, with the shortest execution going first.
            // We'll execute them all in order, remembering the one that refilled the most resources.
            // We'll stop as soon as we find one that we can execute and which costs no resources. 
            // Then we'll return the best result.
            IEnumerable<FarmCycle> orderedFarmCycles = RoomEnemy.FarmCycles.Values.WhereLogicallyRelevant().OrderBy(cycle => cycle.CycleFrames);
            foreach (FarmCycle currentFarmCycle in orderedFarmCycles)
            {
                var currentFarmResult = currentFarmCycle.FarmExecution.Execute(model, inGameState, times: times, previousRoomCount: previousRoomCount);

                // If the farming succeeded, evaluate the results.
                // Otherwise, just skip to the next cycle.
                if (currentFarmResult != null)
                {
                    // If this farm cycle was cost free, we won't find a better one later on.
                    // Immediately return the best result we've encountered so far.
                    if (currentFarmCycle.IsFree(model, inGameState, previousRoomCount: previousRoomCount))
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

    public class UnfinalizedRoomEnemy : AbstractUnfinalizedModelElement<UnfinalizedRoomEnemy, RoomEnemy>, InitializablePostDeserializeInRoom
    {
        public string Id { get; set; }

        public string GroupName { get; set; }

        public string EnemyName { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The actual Enemy this RoomEnemy represents.</para>
        /// </summary>
        public UnfinalizedEnemy Enemy { get; set; }

        public int Quantity { get; set; }

        public ISet<int> HomeNodeIds { get; set; } = new HashSet<int>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The nodes in which this enemy roams, mapped by their node ID. Mutually-exclusive with <see cref="BetweenNodes"/>.</para>
        /// </summary>
        public Dictionary<int, UnfinalizedRoomNode> HomeNodes { get; set; }

        public ISet<int> BetweenNodeIds { get; set; } = new HashSet<int>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>Contains two nodes between which this enemy roams (without ever actually being in either), mapped by their node ID. Mutually-exclusive with <see cref="HomeNodes"/>.</para>
        /// </summary>
        public Dictionary<int, UnfinalizedRoomNode> BetweenNodes { get; set; }

        public UnfinalizedLogicalRequirements Spawn { get; set; } = new UnfinalizedLogicalRequirements();

        // Empty requirements default to "always" behavior, but enemies with no StopSpawn behavior never stop spawning.
        // Make the default state never instead.
        public UnfinalizedLogicalRequirements StopSpawn { get; set; } = UnfinalizedLogicalRequirements.Never();

        public UnfinalizedLogicalRequirements DropRequires { get; set; } = new UnfinalizedLogicalRequirements();

        /// <summary>
        /// Different ways this room enemy can be farmed if it's a spawner, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedFarmCycle> FarmCycles { get; set; } = new Dictionary<string, UnfinalizedFarmCycle>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The Room in which this enemy group is.</para>
        /// </summary>
        public UnfinalizedRoom Room { get; set; }

        public UnfinalizedRoomEnemy()
        {

        }

        public UnfinalizedRoomEnemy(RawRoomEnemy rawRoomEnemy, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            Id = rawRoomEnemy.Id;
            GroupName = rawRoomEnemy.GroupName;
            EnemyName = rawRoomEnemy.EnemyName;
            Quantity = rawRoomEnemy.Quantity;
            HomeNodeIds = new HashSet<int>(rawRoomEnemy.HomeNodes);
            BetweenNodeIds = new HashSet<int>(rawRoomEnemy.BetweenNodes);
            Spawn = rawRoomEnemy.Spawn.ToLogicalRequirements(knowledgeBase);
            if (rawRoomEnemy.StopSpawn != null)
            {
                StopSpawn = rawRoomEnemy.StopSpawn.ToLogicalRequirements(knowledgeBase);
            }
            DropRequires = rawRoomEnemy.DropRequires.ToLogicalRequirements(knowledgeBase);
            FarmCycles = rawRoomEnemy.FarmCycles.Select(rawCycle => new UnfinalizedFarmCycle(rawCycle, knowledgeBase)).ToDictionary(cycle => cycle.Name);
        }

        protected override RoomEnemy CreateFinalizedElement(UnfinalizedRoomEnemy sourceElement, Action<RoomEnemy> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new RoomEnemy(sourceElement, mappingsInsertionCallback, mappings);
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            Room = room;
            Enemy = model.Enemies[EnemyName];
            HomeNodes = HomeNodeIds.Select(id => room.Nodes[id]).ToDictionary(n => n.Id);
            BetweenNodes = BetweenNodeIds.Select(id => room.Nodes[id]).ToDictionary(n => n.Id);

            foreach (UnfinalizedFarmCycle farmCycle in FarmCycles.Values)
            {
                farmCycle.InitializeProperties(model, room, this);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Spawn.InitializeReferencedLogicalElementProperties(model, room));

            unhandled.AddRange(StopSpawn.InitializeReferencedLogicalElementProperties(model, room));

            unhandled.AddRange(DropRequires.InitializeReferencedLogicalElementProperties(model, room));

            foreach(UnfinalizedFarmCycle farmCycle in FarmCycles.Values)
            {
                unhandled.AddRange(farmCycle.InitializeReferencedLogicalElementProperties(model, room, this));
            }

            return unhandled.Distinct();
        }
    }
}
