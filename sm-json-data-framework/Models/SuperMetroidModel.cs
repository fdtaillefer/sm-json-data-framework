using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Navigation;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace sm_json_data_framework.Models
{
    // STITCHME This idea doesn't belong here, but there' no place to put it yet.
    // Maybe one way to go is figure out the direct requirements to get back to the start for all nodes, by moving backwards from the start.
    // If we can remember this cost for every node, then it'd be easy to know at any time what is required to be able to get out after reaching it.
    // And if we know we can get out, we know an acquired item, flag, or lock break is in logic.
    // This should pretty much do every possible route and stop evaluating any branch once it reaches the same situation a second time.
    // It can also stop evaluating anytime it reaches a point with a known escape that is strictly <= the current accumulated escape requirements.
    // Doing this will also require the ability to simplify logical requirements.
    //
    // The thing this will not account for is indirect route that use refills - that's another level.
    // I guess one way to do that would be to figure out the cost of reaching refill spots (maybe just from a given number of nodes or rooms away).
    // Then we could pick out nodes whose escape resource cost is above a given threshold and evaluate the option of going to the refill + escaping.
    // This will need a distinction between having X current resource and having a max of Y resource.
    // Example: I'm sitting somewhere in Norfair. I need to have 240 current energy to reach a refill, then I need at least 412 max energy to escape from there.
    //
    // Once that's all done, there would still be a forward movement pass to reach absolute requirements for putting anything in full logic w/ included escape.
    //
    // The SMZ3 version of this would evaluate not only from the ship, but also from all cross-game portals, because reaching LttP should count as a successful escape.
    // Should probably jump from one escape point to the next so that the later parts of one point's evaluation can run into the already-computed simpler escapes
    // around the portals, and stop processing.
    //
    // This whole process can probably still be applied if I decide to preprocess rooms to eliminate issues of in-room movement with obstacles.

    /// <summary>
    /// Represents a Super Metroid world, for a set of logical options. Think of this as being able to represent 
    /// e.g. the vanilla game or a randomizer seed.
    /// </summary>
    public class SuperMetroidModel
    {
        public const string GRAVITY_SUIT_NAME = "Gravity";
        public const string VARIA_SUIT_NAME = "Varia";
        public const string SPEED_BOOSTER_NAME = "SpeedBooster";

        public SuperMetroidModel()
        {
            // Weapons can't have an initializer directly on itself because of the custom setter
            Weapons = new Dictionary<string, Weapon>();
        }

        /// <summary>
        /// Indicates whether this model's contents has been "initialized".
        /// If true, a lot of pre-processing was done to initialize additional properties in many objects within the returned model.
        /// If false, the objects in the returned model contain mostly just raw data.
        /// </summary>
        public bool Initialized { get; set; }
        
        /// <summary>
        /// Options that describe what the player is expected to be able or unable to do.
        /// </summary>
        public LogicalOptions LogicalOptions { get; set; }

        /// <summary>
        /// A repository of game rules we are operating by.
        /// </summary>
        public SuperMetroidRules Rules { get; set; }

        /// <summary>
        /// Describes the start condition for the game.
        /// </summary>
        public StartConditions StartConditions { get; set; }

        /// <summary>
        /// Creates and assigns to this model internal start conditions, based on the provided ItemContainer.
        /// This should not be called unless rooms, items, and game flags have already been assigned to this model.
        /// </summary>
        /// <param name="itemContainer"></param>
        public void AssignStartingConditions(ItemContainer itemContainer)
        {
            if (!Rooms.TryGetValue(itemContainer.StartingRoomName, out Room startingRoom))
            {
                throw new Exception($"Starting room '{itemContainer.StartingRoomName}' not found.");
            }

            if (!startingRoom.Nodes.TryGetValue(itemContainer.StartingNodeId, out RoomNode startingNode))
            {
                throw new Exception($"Starting node ID {itemContainer.StartingNodeId} not found in room '{startingRoom.Name}'.");
            }

            List<GameFlag> startingFlags = new List<GameFlag>();
            foreach(string flagName in itemContainer.StartingGameFlagNames)
            {
                if (!GameFlags.TryGetValue(flagName, out GameFlag flag))
                {
                    throw new Exception($"Starting game flag {flagName} not found.");
                }
                startingFlags.Add(flag);
            }

            List<NodeLock> startingLocks = new List<NodeLock>();
            foreach(string lockName in itemContainer.StartingNodeLockNames)
            {
                if(!Locks.TryGetValue(lockName, out NodeLock nodeLock))
                {
                    throw new Exception($"Starting node lock {lockName} not found.");
                }
                startingLocks.Add(nodeLock);
            }

            ResourceCount startingResources = new ResourceCount();
            foreach(ResourceCapacity capacity in itemContainer.StartingResources)
            {
                startingResources.ApplyAmount(capacity.Resource, capacity.MaxAmount);
            }

            ItemInventory startingInventory = new ItemInventory(startingResources);
            foreach(string itemName in itemContainer.StartingItemNames)
            {
                if (!Items.TryGetValue(itemName, out Item item))
                {
                    throw new Exception($"Starting item {itemName} not found.");
                }
                startingInventory.ApplyAddItem(item);
            }

            StartConditions = new StartConditions
            {
                StartingNode = startingNode,
                StartingGameFlags = startingFlags,
                StartingOpenLocks = startingLocks,
                BaseResourceMaximums = startingResources,
                // Default starting resource counts to the starting maximum
                StartingResources = startingResources.Clone(),
                StartingInventory = startingInventory
            };
        }

        /// <summary>
        /// The helpers in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Helper> Helpers { get; set; } = new Dictionary<string, Helper>();

        /// <summary>
        /// The techs in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Tech> Techs { get; set; } = new Dictionary<string, Tech>();

        /// <summary>
        /// Returns whether the shinespark tech is enabled.
        /// </summary>
        /// <returns></returns>
        public bool CanShinespark()
        {
            return LogicalOptions.IsTechEnabled(Techs["canShinespark"]);
        }

        /// <summary>
        /// The items in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Item> Items { get; set; } = new Dictionary<string, Item>();

        /// <summary>
        /// The game flags in this model, mapped by name.
        /// </summary>
        public IDictionary<string, GameFlag> GameFlags { get; set; } = new Dictionary<string, GameFlag>();

        /// <summary>
        /// The rooms in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Room> Rooms { get; set; } = new Dictionary<string, Room>();

        /// <summary>
        /// The nodes in this model, mapped by name.
        /// </summary>
        public IDictionary<string, RoomNode> Nodes { get; set; } = new Dictionary<string, RoomNode>();

        /// <summary>
        /// The node locks in this model, mapped by name.
        /// </summary>
        public IDictionary<string, NodeLock> Locks { get; set; } = new Dictionary<string, NodeLock>();

        /// <summary>
        /// A dictionary that maps a node's IdentifyingString to a one-way connection with that node as the origin.
        /// </summary>
        public IDictionary<string, Connection> Connections { get; set; } = new Dictionary<string, Connection>();

        private IDictionary<string, Weapon> _weapons;
        /// <summary>
        /// The weapons in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Weapon> Weapons {
            get { return _weapons; }
            set
            {
                _weapons = value;
                WeaponsByCategory = Weapons.Values
                    .SelectMany(w => w.Categories.Select(c => (weapon: w, category: c)))
                    .GroupBy(pair => pair.category)
                    .ToDictionary(g => g.Key, g => g.ToList().Select(pair => pair.weapon));
            }
        }

        /// <summary>
        /// A dicionary mapping all weapon categories to a list of weapons in that category.
        /// </summary>
        public IDictionary<WeaponCategoryEnum, IEnumerable<Weapon>> WeaponsByCategory { get; private set; }

        /// <summary>
        /// The normal enemies in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Enemy> Enemies { get; set; } = new Dictionary<string, Enemy>();

        /// <summary>
        /// The boss enemies in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Enemy> Bosses { get; set; } = new Dictionary<string, Enemy>();

        public InGameState InitialGameState { private get; set; }

        /// <summary>
        /// Compares the two provided game states, using the comparer returned by <see cref="GetInGameStateComparer"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int CompareInGameStates(InGameState x, InGameState y)
        {
            return GetInGameStateComparer().Compare(x, y);
        }

        /// <summary>
        /// Returns an <see cref="InGameStateComparer"/>, initialized with the internal relative resource values.
        /// </summary>
        /// <returns></returns>
        public InGameStateComparer GetInGameStateComparer()
        {
            return LogicalOptions.InGameStateComparer;
        }

        /// <summary>
        /// <para>Given an enumeration of executables, attempts to find the least costly one that can be successfully executed.
        /// Returns the associated execution result.
        /// If a no-cost executable is found, its result is returned immediately.</para>
        /// <para>If there are no executables, this is an automatic failure.</para>
        /// </summary>
        /// <param name="initialInGameState">The initial in-game state. Will not be modified by this method.</param>
        /// <param name="executables">An enumeration of executables to attempt executing.</param>
        /// <param name="times">The number of consecutive times the executables should be executed.
        /// Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// (whenever in-room state is relevant).</param>
        /// <param name="acceptationCondition">An optional Predicate that is checked against the resulting in-game state of executions.
        /// Executions whose resulting state does not respect the predicate are rejected.</param>
        /// <returns>The best executable, alongside its ExecutionResult, or default values if none succeeded</returns>
        public (T bestExecutable, ExecutionResult result) ExecuteBest<T>(IEnumerable<T> executables, InGameState initialInGameState, int times = 1,
            bool usePreviousRoom = false, Predicate<InGameState> acceptationCondition = null) where T:IExecutable
        {
            InGameStateComparer comparer = GetInGameStateComparer();

            // Try to execute all executables, returning whichever spends the lowest amount of resources
            (T bestExecutable, ExecutionResult result) bestResult = (default(T), null);
            foreach (T currentExecutable in executables)
            {
                ExecutionResult currentResult = currentExecutable.Execute(this, initialInGameState, times: times, usePreviousRoom: usePreviousRoom);

                // If the fulfillment was successful
                if (currentResult != null && (acceptationCondition == null || acceptationCondition.Invoke(currentResult.ResultingState)))
                {

                    // If the fulfillment did not reduce the amount of resources, return immediately
                    if (comparer.Compare(currentResult.ResultingState, initialInGameState) == 0)
                    {
                        return (currentExecutable, currentResult);
                    }

                    // If the resulting state is the best we've found yet, retain it
                    if (bestResult.result == null
                        || comparer.Compare(currentResult.ResultingState, bestResult.result.ResultingState) > 0)
                    {
                        bestResult = (currentExecutable, currentResult);
                    }
                }
            }

            return bestResult;
        }

        /// <summary>
        /// <para>Given an enumeration of executables, executes them all successively, starting from the provided initialGameState.</para>
        /// <para>This method will give up at the first failed execution and return null.</para>
        /// <para>If there are no executables, this is an automatic success.</para>
        /// </summary>
        /// <typeparam name="T">The type of the executables to execute.</typeparam>
        /// <param name="initialInGameState">The initial in-game state. Will not be modified by this method.</param>
        /// <param name="executables">An enumeration of executables. This must not modify the InGameState provided to it.</param>
        /// <param name="executionFunction">A function that executes an executable.</param>
        /// <returns>The InGameState obtained by executing all executables, or null if any execution failed.
        /// This will never return the initialInGameState instance.</returns>
        public ExecutionResult ExecuteAll(IEnumerable<IExecutable> executables, InGameState initialInGameState, int times = 1, bool usePreviousRoom = false)
        {
            // If there are no executables, this is an instant success. Clone the inGameState to respect the contract.
            if(!executables.Any())
            {
                return new ExecutionResult(initialInGameState.Clone());
            }

            // Iterate over all executables, attempting to fulfill them
            ExecutionResult result = null;
            foreach (IExecutable currentExecutable in executables)
            {
                // If this is the first execution, generate an initial result
                if(result == null)
                {
                    result = currentExecutable.Execute(this, initialInGameState, times: times, usePreviousRoom: usePreviousRoom);
                }
                // If this is not the first execution, apply this execution on top of previous result
                else
                {
                    result = result.AndThen(currentExecutable, this, times: times, usePreviousRoom: usePreviousRoom);
                }

                // If we failed to execute, give up immediately
                if (result == null)
                {
                    return null;
                }
            }
            return result;
        }

        /// <summary>
        /// Creates and returns a copy of the initial game state. Requires this model to have been initialized.
        /// </summary>
        /// <returns></returns>
        public InGameState CreateInitialGameStateCopy()
        {
            if (!Initialized)
            {
                throw new Exception("InGameState is not available for a model that's not initialized.");
            }
            return new InGameState(InitialGameState);
        }

        /// <summary>
        /// Creates and returns a game navigator at the starting location and with starting resources.
        /// Requires this model to have been initialized.
        /// </summary>
        /// <param name="maxPreviousStatesSize">The maximum number of previous states that the created navigator
        /// should keep in memory.</param>
        /// <returns></returns>
        public GameNavigator CreateInitialGameNavigator(int maxPreviousStatesSize)
        {
            if (!Initialized)
            {
                throw new Exception("GameNavigator is not available for a model that's not initialized.");
            }
            return new GameNavigator(this, CreateInitialGameStateCopy(), maxPreviousStatesSize);
        }
    }
}
