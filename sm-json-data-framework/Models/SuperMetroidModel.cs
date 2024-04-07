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
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// <para>
    /// Represents a Super Metroid world, for a set of logical options. Think of this as being able to represent 
    /// e.g. the vanilla game or a randomizer seed.
    /// </para>
    /// <para>
    /// Note that the model and some of its contents are NOT immutable, however normal use is not intended to modify them.
    /// </para>
    /// </summary>
    public class SuperMetroidModel
    {
        public const string POWER_BEAM_NAME = "PowerBeam";
        public const string POWER_SUIT_NAME = "PowerSuit";
        public const string GRAVITY_SUIT_NAME = "Gravity";
        public const string VARIA_SUIT_NAME = "Varia";
        public const string SPEED_BOOSTER_NAME = "SpeedBooster";

        public const string MISSILE_NAME = "Missile";
        public const string SUPER_NAME = "Super";
        public const string POWER_BOMB_NAME = "PowerBomb";
        public const string ENERGY_TANK_NAME = "ETank";
        public const string RESERVE_TANK_NAME = "ReserveTank";

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
        /// Gets and returns a node, referenced by its room name and node ID.
        /// </summary>
        /// <param name="roomName">Name of the room that contains the node to find</param>
        /// <param name="nodeId">ID of the node to find within the room</param>
        /// <returns>The node</returns>
        /// <exception cref="Exception">If the room or node is not found</exception>
        public RoomNode GetNodeInRoom(string roomName, int nodeId)
        {
            if (!Rooms.TryGetValue(roomName, out Room room))
            {
                throw new Exception($"Room '{roomName}' not found.");
            }

            if (!room.Nodes.TryGetValue(nodeId, out RoomNode node))
            {
                throw new Exception($"Node ID {nodeId} not found in room '{room.Name}'.");
            }

            return node;
        }

        /// <summary>
        /// The runways in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Runway> Runways { get; set; } = new Dictionary<string, Runway>();

        /// <summary>
        /// The node locks in this model, mapped by name.
        /// </summary>
        public IDictionary<string, NodeLock> Locks { get; set; } = new Dictionary<string, NodeLock>();

        /// <summary>
        /// All groups of enemies found in any room, mapped by their group name.
        /// </summary>
        public IDictionary<string, RoomEnemy> RoomEnemies {get;set;} = new Dictionary<string, RoomEnemy>();

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

        private ReadOnlyInGameState _initialGameState;
        public ReadOnlyInGameState InitialGameState {
            get { return _initialGameState; }
            set { _initialGameState = value?.Clone(); } 
        }

        /// <summary>
        /// Compares the two provided game states, using the comparer returned by <see cref="GetInGameStateComparer"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int CompareInGameStates(ReadOnlyInGameState x, ReadOnlyInGameState y)
        {
            return GetInGameStateComparer().Compare(x, y);
        }

        /// <summary>
        /// Returns an <see cref="InGameStateComparer"/> obtained from this model's <see cref="LogicalOptions"/>, initialized with the internal relative resource values.
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
        /// <param name="previousRoomCount">The number of playable rooms to go back by (whenever in-room state is relevant). 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <param name="acceptationCondition">An optional Predicate that is checked against the resulting in-game state of executions.
        /// Executions whose resulting state does not respect the predicate are rejected.</param>
        /// <returns>The best executable, alongside its ExecutionResult, or default values if none succeeded</returns>
        public (T bestExecutable, ExecutionResult result) ExecuteBest<T>(IEnumerable<T> executables, ReadOnlyInGameState initialInGameState, int times = 1,
            int previousRoomCount = 0, Predicate<ReadOnlyInGameState> acceptationCondition = null) where T:IExecutable
        {
            InGameStateComparer comparer = GetInGameStateComparer();

            // Try to execute all executables, returning whichever spends the lowest amount of resources
            (T bestExecutable, ExecutionResult result) bestResult = (default(T), null);
            foreach (T currentExecutable in executables)
            {
                ExecutionResult currentResult = currentExecutable.Execute(this, initialInGameState, times: times, previousRoomCount: previousRoomCount);

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
        /// <param name="executables">An enumeration of executables. This must not modify the InGameState provided to it.</param>
        /// <param name="initialInGameState">The initial in-game state. Will not be modified by this method.</param>
        /// <param name="times">The number of consecutive times the executables should be executed.
        /// <param name="previousRoomCount">The number of playable rooms to go back by (whenever in-room state is relevant). 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// Only really impacts resource cost, since most items are non-consumable.</param>
        /// <returns>The InGameState obtained by executing all executables, or null if any execution failed.
        /// This will never return the initialInGameState instance.</returns>
        public ExecutionResult ExecuteAll(IEnumerable<IExecutable> executables, ReadOnlyInGameState initialInGameState, int times = 1, int previousRoomCount = 0)
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
                    result = currentExecutable.Execute(this, initialInGameState, times: times, previousRoomCount: previousRoomCount);
                }
                // If this is not the first execution, apply this execution on top of previous result
                else
                {
                    result = result.AndThen(currentExecutable, this, times: times, previousRoomCount: previousRoomCount);
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
            return InitialGameState.Clone();
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
