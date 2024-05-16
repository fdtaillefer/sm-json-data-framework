using sm_json_data_framework.Converters;
using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Navigation;
using sm_json_data_framework.Models.Raw;
using sm_json_data_framework.Models.Raw.Connections;
using sm_json_data_framework.Models.Raw.Helpers;
using sm_json_data_framework.Models.Raw.Items;
using sm_json_data_framework.Models.Raw.Techs;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Represents a Super Metroid world, possibly altered by a set of logical options. Think of this as being able to represent the ROM of
    /// e.g. the vanilla game or a randomizer seed.
    /// </para>
    /// <para>
    /// This model is immutable.
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

        public SuperMetroidModel(UnfinalizedSuperMetroidModel sourceModel)
        {
            ModelFinalizationMappings mappings = new ModelFinalizationMappings();
            Items = sourceModel.Items.Values.Select(item => item.Finalize(mappings)).ToDictionary(item => item.Name).AsReadOnly();
            GameFlags = sourceModel.GameFlags.Values.Select(flag => flag.Finalize(mappings)).ToDictionary(flag => flag.Name).AsReadOnly();
            Weapons = sourceModel.Weapons.Values.Select(weapon => weapon.Finalize(mappings)).ToDictionary(weapon => weapon.Name).AsReadOnly();
            WeaponsByCategory = sourceModel.WeaponsByCategory
                .Select(kvp => new KeyValuePair<WeaponCategoryEnum, IReadOnlyList<Weapon>>(kvp.Key, kvp.Value.Select(weapon => weapon.Finalize(mappings)).ToList().AsReadOnly()))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value).AsReadOnly();
            Enemies = sourceModel.Enemies.Values.Select(enemy => enemy.Finalize(mappings)).ToDictionary(enemy => enemy.Name).AsReadOnly();
            Helpers = sourceModel.Helpers.Values.Select(helper => helper.Finalize(mappings)).ToDictionary(helper => helper.Name).AsReadOnly();
            Techs = sourceModel.Techs.Values.Select(tech => tech.Finalize(mappings)).ToDictionary(tech => tech.Name).AsReadOnly();
            Rooms = sourceModel.Rooms.Values.Select(room => room.Finalize(mappings)).ToDictionary(room => room.Name).AsReadOnly();
            Nodes = sourceModel.Nodes.Values.Select(node => node.Finalize(mappings)).ToDictionary(node => node.Name).AsReadOnly();
            Runways = sourceModel.Runways.Values.Select(runway => runway.Finalize(mappings)).ToDictionary(runway => runway.Name).AsReadOnly();
            Locks = sourceModel.Locks.Values.Select(locks => locks.Finalize(mappings)).ToDictionary(locks => locks.Name).AsReadOnly();
            RoomEnemies = sourceModel.RoomEnemies.Values.Select(roomEnemy => roomEnemy.Finalize(mappings)).ToDictionary(roomEnemy => roomEnemy.GroupName).AsReadOnly();
            Connections = sourceModel.Connections.Select(kvp => new KeyValuePair<string, Connection>(kvp.Key, kvp.Value.Finalize(mappings)))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value).AsReadOnly();
            Rules = sourceModel.Rules;
            StartConditions = sourceModel.StartConditions.Finalize(mappings);
            InitialGameState = new InGameState(StartConditions);
        }

        /// <summary>
        /// The items in this model, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Item> Items { get; }

        /// <summary>
        /// The game flags in this model, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, GameFlag> GameFlags { get; }

        /// <summary>
        /// The weapons in this model, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Weapon> Weapons { get; }

        /// <summary>
        /// A dicionary mapping all weapon categories to a list of weapons in that category.
        /// </summary>
        public IReadOnlyDictionary<WeaponCategoryEnum, IReadOnlyList<Weapon>> WeaponsByCategory { get; }

        /// <summary>
        /// The normal enemies and boss enemies in this model, mapped by name.
        /// </summary>
        public IDictionary<string, Enemy> Enemies { get; }

        /// <summary>
        /// The helpers in this model, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Helper> Helpers { get; }

        /// <summary>
        /// The techs in this model, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Tech> Techs { get; }

        /// <summary>
        /// The rooms in this model, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Room> Rooms { get; }

        /// <summary>
        /// The nodes in this model, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, RoomNode> Nodes { get; }

        /// <summary>
        /// The runways in this model, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Runway> Runways { get; }

        /// <summary>
        /// The node locks in this model, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, NodeLock> Locks { get; }

        /// <summary>
        /// All groups of enemies found in any room, mapped by their group name.
        /// </summary>
        public IReadOnlyDictionary<string, RoomEnemy> RoomEnemies { get; }

        /// <summary>
        /// A dictionary that maps a node's IdentifyingString to a one-way connection with that node as the origin.
        /// </summary>
        public IReadOnlyDictionary<string, Connection> Connections { get; }

        /// <summary>
        /// A repository of game rules we are operating by.
        /// </summary>
        public SuperMetroidRules Rules { get; }

        /// <summary>
        /// Describes the start condition for the game, complete with relevant objects within this model.
        /// </summary>
        public StartConditions StartConditions { get; }

        /// <summary>
        /// An <see cref="InGameStateComparer"/> which is either a default implementation or obtained from applied <see cref="LogicalOptions"/>.
        /// </summary>
        public InGameStateComparer InGameStateComparer { get; private set; } = LogicalOptions.DefaultInGameStateComparer;

        public ReadOnlyInGameState InitialGameState { get; }

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
        /// Clones the provided LogicalOptions, and applies then to this model.
        /// </summary>
        /// <param name="logicalOptions">The LogicalOptions to apply. If null, this instead removes all alterations from logical options.</param>
        public void ApplyLogicalOptions(LogicalOptions logicalOptions)
        {
            ReadOnlyLogicalOptions logicalOptionsToApply = null;

            if (logicalOptions == null)
            {
                InGameStateComparer = LogicalOptions.DefaultInGameStateComparer;
            }
            else
            {
                logicalOptionsToApply = logicalOptions.Clone().AsReadOnly();
                InGameStateComparer = logicalOptionsToApply.InGameStateComparer;
            }

            foreach (GameFlag gameFlag in GameFlags.Values)
            {
                gameFlag.ApplyLogicalOptions(logicalOptionsToApply);
            }

            foreach (Helper helper in Helpers.Values)
            {
                helper.ApplyLogicalOptions(logicalOptionsToApply);
            }

            foreach (Item item in Items.Values)
            {
                item.ApplyLogicalOptions(logicalOptionsToApply);
            }

            foreach (Tech tech in Techs.Values)
            {
                tech.ApplyLogicalOptions(logicalOptionsToApply);
            }

            foreach (Weapon weapon in Weapons.Values)
            {
                weapon.ApplyLogicalOptions(logicalOptionsToApply);
            }

            foreach (Enemy enemy in Enemies.Values)
            {
                enemy.ApplyLogicalOptions(logicalOptionsToApply);
            }

            foreach (Connection connection in Connections.Values)
            {
                connection.ApplyLogicalOptions(logicalOptionsToApply);
            }

            foreach (Room room in Rooms.Values)
            {
                room.ApplyLogicalOptions(logicalOptionsToApply);
            }
        }

        /// <summary>
        /// Compares the two provided game states, using the internal comparer.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int CompareInGameStates(ReadOnlyInGameState x, ReadOnlyInGameState y)
        {
            return InGameStateComparer.Compare(x, y);
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
            int previousRoomCount = 0, Predicate<ReadOnlyInGameState> acceptationCondition = null) where T : IExecutable
        {
            // Try to execute all executables, returning whichever spends the lowest amount of resources
            (T bestExecutable, ExecutionResult result) bestResult = (default(T), null);
            foreach (T currentExecutable in executables)
            {
                ExecutionResult currentResult = currentExecutable.Execute(this, initialInGameState, times: times, previousRoomCount: previousRoomCount);

                // If the fulfillment was successful
                if (currentResult != null && (acceptationCondition == null || acceptationCondition.Invoke(currentResult.ResultingState)))
                {

                    // If the fulfillment did not reduce the amount of resources, return immediately
                    if (InGameStateComparer.Compare(currentResult.ResultingState, initialInGameState) == 0)
                    {
                        return (currentExecutable, currentResult);
                    }

                    // If the resulting state is the best we've found yet, retain it
                    if (bestResult.result == null
                        || InGameStateComparer.Compare(currentResult.ResultingState, bestResult.result.ResultingState) > 0)
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
            if (!executables.Any())
            {
                return new ExecutionResult(initialInGameState.Clone());
            }

            // Iterate over all executables, attempting to fulfill them
            ExecutionResult result = null;
            foreach (IExecutable currentExecutable in executables)
            {
                // If this is the first execution, generate an initial result
                if (result == null)
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
            return new GameNavigator(this, CreateInitialGameStateCopy(), maxPreviousStatesSize);
        }
    }

    public class UnfinalizedSuperMetroidModel
    {
        public UnfinalizedSuperMetroidModel()
        {
            // Weapons can't have an initializer directly on itself because of the custom setter
            Weapons = new Dictionary<string, UnfinalizedWeapon>();
        }

        /// <summary>
        /// Creates a SuperMetroidModel using the data in the provided RawSuperMetroidModel, as well as other parameters
        /// </summary>
        /// <param name="rawModel">Raw model containing raw data presumably obtained from json model</param>
        /// <param name="rules">A repository of game rules to operate by.
        /// If null, will use the default constructor of SuperMetroidRules, giving vanilla rules.</param>
        /// <param name="basicStartConditionsCustomizer">An optional object that can apply modifications to the <see cref="BasicStartConditions"/> that will
        /// be created and assigned to this model.</param>
        /// <param name="overrideObjectTypes">A sequence of tuples, pairing together an ObjectLogicalElementTypeEnum and the C# type that should be used to 
        /// to represent that ObjectLogicalElementTypeEnum when converting logical requirements from a raw equivalent.
        /// The provided C# types must extend the default type that is normally used for any given ObjectLogicalElementTypeEnum.</param>
        /// <param name="overrideStringTypes">A sequence of tuples, pairing together a StringLogicalElementTypeEnum and the C# type that should be used to 
        /// to represent that StringLogicalElementTypeEnum when converting logical requirements from a raw equivalent.
        /// The provided C# types must extend the default type that is normally used for any given StringLogicalElementTypeEnum.</param>
        /// <exception cref="Exception">If this method fails to interpret any logical element</exception>
        public UnfinalizedSuperMetroidModel(RawSuperMetroidModel rawModel, SuperMetroidRules rules = null,
            IBasicStartConditionsCustomizer basicStartConditionsCustomizer = null,
            IEnumerable<(ObjectLogicalElementTypeEnum typeEnum, Type type)> overrideObjectTypes = null,
            IEnumerable<(StringLogicalElementTypeEnum typeEnum, Type type)> overrideStringTypes = null)
        {
            rules ??= new SuperMetroidRules();

            Rules = rules;

            // Put items in model
            Items = rawModel.ItemContainer.ImplicitItems
                    .Select(n => new UnfinalizedItem(n))
                    .Concat(rawModel.ItemContainer.UpgradeItems.Select(rawItem => new UnfinalizedInGameItem(rawItem)))
                    .Concat(rawModel.ItemContainer.ExpansionItems.Select(rawItem => new UnfinalizedExpansionItem(rawItem)))
                    .ToDictionary(i => i.Name);

            // Put game flags in model
            GameFlags = rawModel.ItemContainer.GameFlags
                .Select(n => new UnfinalizedGameFlag(n))
                .ToDictionary(f => f.Name);

            // Put basic starting conditions in model
            BasicStartConditions basicStartConditions = new BasicStartConditions(rawModel.ItemContainer);
            basicStartConditionsCustomizer?.Customize(basicStartConditions);
            BasicStartConditions = basicStartConditions;

            // Put helpers in model
            Helpers = rawModel.HelperContainer.Helpers.Select(rawHelper => new UnfinalizedHelper(rawHelper)).ToDictionary(h => h.Name);

            // Put techs in model
            Techs = rawModel.TechContainer.SelectTopLevelTechs()
                .Select(rawTech => new UnfinalizedTech(rawTech))
                .SelectMany(tech => tech.SelectWithExtensions())
                .ToDictionary(tech => tech.Name);

            // At this point, Techs and Helpers don't contain their LogicalRequirements, we skipped them because
            // they could reference Techs and Helpers that didn't exist yet. Go back and assign them.
            LogicalElementCreationKnowledgeBase knowledgeBase = LogicalElementCreationUtils.CreateLogicalElementCreationKnowledgeBase(this,
                overrideObjectTypes: overrideObjectTypes, overrideStringTypes: overrideStringTypes);
            foreach (RawHelper rawHelper in rawModel.HelperContainer.Helpers)
            {
                Helpers[rawHelper.Name].Requires = rawHelper.Requires.ToLogicalRequirements(knowledgeBase);
            }

            foreach (RawTech rawTech in rawModel.TechContainer.SelectAllTechs())
            {
                Techs[rawTech.Name].Requires = rawTech.Requires.ToLogicalRequirements(knowledgeBase);
            }

            // Put weapons in model
            Weapons = rawModel.WeaponContainer.Weapons.Select(rawWeapon => new UnfinalizedWeapon(rawWeapon, knowledgeBase))
                .ToDictionary(weapon => weapon.Name);

            // Put regular enemies and bosses in model
            Enemies = rawModel.EnemyContainer.Enemies.Concat(rawModel.BossContainer.Enemies).Select(rawEnemy => new UnfinalizedEnemy(rawEnemy))
                .ToDictionary(enemy => enemy.Name);

            // Put connections in model
            foreach (RawConnection rawConnection in rawModel.ConnectionContainer.Connections)
            {
                RawConnectionNode rawNode1 = rawConnection.Nodes.ElementAt(0);
                RawConnectionNode rawNode2 = rawConnection.Nodes.ElementAt(1);

                // If the forward direction is applicable for this json connection, create and add a corresponding forward one-way connection
                if (rawConnection.Direction == ConnectionDirectionEnum.Forward
                    || rawConnection.Direction == ConnectionDirectionEnum.Bidirectional)
                {
                    UnfinalizedConnection forwardConnection = new UnfinalizedConnection(rawConnection, rawNode1, rawNode2);
                    Connections.Add(forwardConnection.FromNode.IdentifyingString, forwardConnection);
                }

                // If the backward direction is applicable for this json connection, create and add a corresponding backward one-way connection
                if (rawConnection.Direction == ConnectionDirectionEnum.Backward
                    || rawConnection.Direction == ConnectionDirectionEnum.Bidirectional)
                {
                    UnfinalizedConnection backwardConnection = new UnfinalizedConnection(rawConnection, rawNode2, rawNode1);
                    Connections.Add(backwardConnection.FromNode.IdentifyingString, backwardConnection);
                }
            }

            // Put rooms in model
            Rooms = rawModel.RoomContainer.Rooms.Select(rawRoom => new UnfinalizedRoom(rawRoom, knowledgeBase)).ToDictionary(room => room.Name);

            // Now we've created all models in a basic state...
            InitializeBaseModel();
        }

        // Make this private once we get rid of the ModelReader code?
        /// <summary>
        /// <para>
        /// Initializes all data in this model that isn't part of the "base" model.
        /// This will initialize FK references and calculated values in sub-models as well as some top-level convenience properties.
        /// It may also clean up some sub-models that are found to contribute nothing useful.
        /// </para>
        /// <para>
        /// Calling this on an instance that has already has this called previously has no adverse effect, but will accomplish nothing
        /// and waste processing.
        /// </para>
        /// </summary>
        /// <exception cref="Exception">If a logical element references an object that isn't found in this model - attempting to initialize it
        /// will result in an exception.</exception>
        private void InitializeBaseModel()
        {
            // Initialize a few top-level convenience maps
            Dictionary<string, UnfinalizedRoomEnemy> roomEnemies = new Dictionary<string, UnfinalizedRoomEnemy>();
            Dictionary<string, UnfinalizedNodeLock> locks = new Dictionary<string, UnfinalizedNodeLock>();
            Dictionary<string, UnfinalizedRoomNode> nodes = new Dictionary<string, UnfinalizedRoomNode>();
            Dictionary<string, UnfinalizedRunway> runways = new Dictionary<string, UnfinalizedRunway>();
            foreach (UnfinalizedRoom room in Rooms.Values)
            {
                foreach (UnfinalizedRoomEnemy roomEnemy in room.Enemies.Values)
                {
                    roomEnemies.Add(roomEnemy.GroupName, roomEnemy);
                }

                foreach (UnfinalizedRoomNode node in room.Nodes.Values)
                {
                    nodes.Add(node.Name, node);
                    foreach (UnfinalizedRunway runway in node.Runways.Values)
                    {
                        runways.Add(runway.Name, runway);
                    }
                    foreach (KeyValuePair<string, UnfinalizedNodeLock> kvp in node.Locks)
                    {
                        locks.Add(kvp.Key, kvp.Value);
                    }
                }
            }
            Locks = locks;
            Nodes = nodes;
            Runways = runways;
            RoomEnemies = roomEnemies;


            // Initialize properties of objects within the model
            foreach (UnfinalizedEnemy enemy in Enemies.Values)
            {
                enemy.InitializeProperties(this);
            }
            foreach (UnfinalizedRoom room in Rooms.Values)
            {
                room.InitializeProperties(this);
            }

            // Now that rooms, flags, and items are in the model, create and assign start conditions
            StartConditions = new UnfinalizedStartConditions(this);

            // Create and assign initial game state
            InitialGameState = new UnfinalizedInGameState(StartConditions);

            // Initialize all references within logical elements
            List<string> unhandledLogicalElementProperties = new List<string>();

            foreach (UnfinalizedHelper helper in Helpers.Values)
            {
                unhandledLogicalElementProperties.AddRange(helper.InitializeReferencedLogicalElementProperties(this));
            }

            foreach (UnfinalizedTech tech in Techs.Values)
            {
                unhandledLogicalElementProperties.AddRange(tech.InitializeReferencedLogicalElementProperties(this));
            }

            foreach (UnfinalizedWeapon weapon in Weapons.Values)
            {
                unhandledLogicalElementProperties.AddRange(weapon.InitializeReferencedLogicalElementProperties(this));
            }

            foreach (UnfinalizedRoom room in Rooms.Values)
            {
                unhandledLogicalElementProperties.AddRange(room.InitializeReferencedLogicalElementProperties(this));
            }

            // If there was any logical element property we failed to resolve, consider that an error
            if (unhandledLogicalElementProperties.Any())
            {
                throw new Exception($"The following logical element property values could not be resolved " +
                    $"to an object of their expected type: {string.Join(", ", unhandledLogicalElementProperties.Distinct().Select(s => $"'{s}'"))}");
            }
        }

        /// <summary>
        /// A repository of game rules we are operating by.
        /// </summary>
        public SuperMetroidRules Rules { get; set; }

        /// <summary>
        /// Describes the start condition for the game, with basic int and string foreign keys.
        /// </summary>
        public BasicStartConditions BasicStartConditions { get; set; }

        /// <summary>
        /// Describes the start condition for the game, complete with relevant objects within this model.
        /// </summary>
        public UnfinalizedStartConditions StartConditions { get; set; }

        /// <summary>
        /// The helpers in this model, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedHelper> Helpers { get; set; } = new Dictionary<string, UnfinalizedHelper>();

        /// <summary>
        /// The techs in this model, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedTech> Techs { get; set; } = new Dictionary<string, UnfinalizedTech>();

        /// <summary>
        /// The items in this model, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedItem> Items { get; set; } = new Dictionary<string, UnfinalizedItem>();

        /// <summary>
        /// The game flags in this model, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedGameFlag> GameFlags { get; set; } = new Dictionary<string, UnfinalizedGameFlag>();

        /// <summary>
        /// The rooms in this model, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedRoom> Rooms { get; set; } = new Dictionary<string, UnfinalizedRoom>();

        /// <summary>
        /// The nodes in this model, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedRoomNode> Nodes { get; set; } = new Dictionary<string, UnfinalizedRoomNode>();

        /// <summary>
        /// Gets and returns a node, referenced by its room name and node ID.
        /// </summary>
        /// <param name="roomName">Name of the room that contains the node to find</param>
        /// <param name="nodeId">ID of the node to find within the room</param>
        /// <returns>The node</returns>
        /// <exception cref="Exception">If the room or node is not found</exception>
        public UnfinalizedRoomNode GetNodeInRoom(string roomName, int nodeId)
        {
            if (!Rooms.TryGetValue(roomName, out UnfinalizedRoom room))
            {
                throw new Exception($"Room '{roomName}' not found.");
            }

            if (!room.Nodes.TryGetValue(nodeId, out UnfinalizedRoomNode node))
            {
                throw new Exception($"Node ID {nodeId} not found in room '{room.Name}'.");
            }

            return node;
        }

        /// <summary>
        /// The runways in this model, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedRunway> Runways { get; set; } = new Dictionary<string, UnfinalizedRunway>();

        /// <summary>
        /// The node locks in this model, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedNodeLock> Locks { get; set; } = new Dictionary<string, UnfinalizedNodeLock>();

        /// <summary>
        /// All groups of enemies found in any room, mapped by their group name.
        /// </summary>
        public IDictionary<string, UnfinalizedRoomEnemy> RoomEnemies { get; set; } = new Dictionary<string, UnfinalizedRoomEnemy>();

        /// <summary>
        /// A dictionary that maps a node's IdentifyingString to a one-way connection with that node as the origin.
        /// </summary>
        public IDictionary<string, UnfinalizedConnection> Connections { get; set; } = new Dictionary<string, UnfinalizedConnection>();

        private IDictionary<string, UnfinalizedWeapon> _weapons;
        /// <summary>
        /// The weapons in this model, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedWeapon> Weapons {
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
        public IDictionary<WeaponCategoryEnum, IEnumerable<UnfinalizedWeapon>> WeaponsByCategory { get; private set; }

        /// <summary>
        /// The normal enemies and boss enemies in this model, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedEnemy> Enemies { get; set; } = new Dictionary<string, UnfinalizedEnemy>();

        /// <summary>
        /// An <see cref="InGameStateComparer"/> which is either a default implementation or obtained from applied <see cref="LogicalOptions"/>.
        /// </summary>
        public InGameStateComparer InGameStateComparer { get; private set; } = LogicalOptions.DefaultInGameStateComparer;

        /// <summary>
        /// Clones the provided LogicalOptions, and applies then to this model.
        /// </summary>
        /// <param name="logicalOptions">The LogicalOptions to apply. If null, this instead removes all alterations from logical options.</param>
        public void ApplyLogicalOptions(LogicalOptions logicalOptions)
        {
            ReadOnlyLogicalOptions logicalOptionsToApply = null;

            if(logicalOptions == null)
            {
                InGameStateComparer = LogicalOptions.DefaultInGameStateComparer;
            }
            else 
            {
                logicalOptionsToApply = logicalOptions.Clone().AsReadOnly();
                InGameStateComparer = logicalOptionsToApply.InGameStateComparer;
            }

            foreach(UnfinalizedGameFlag gameFlag in GameFlags.Values) {
                gameFlag.ApplyLogicalOptions(logicalOptionsToApply);
            }

            foreach (UnfinalizedHelper helper in Helpers.Values)
            {
                helper.ApplyLogicalOptions(logicalOptionsToApply);
            }

            foreach (UnfinalizedItem item in Items.Values)
            {
                item.ApplyLogicalOptions(logicalOptionsToApply);
            }

            foreach (UnfinalizedTech tech in Techs.Values)
            {
                tech.ApplyLogicalOptions(logicalOptionsToApply);
            }

            foreach (UnfinalizedWeapon weapon in Weapons.Values)
            {
                weapon.ApplyLogicalOptions(logicalOptionsToApply);
            }

            foreach (UnfinalizedEnemy enemy in Enemies.Values)
            {
                enemy.ApplyLogicalOptions(logicalOptionsToApply);
            }

            foreach (UnfinalizedConnection connection in Connections.Values)
            {
                connection.ApplyLogicalOptions(logicalOptionsToApply);
            }

            foreach (UnfinalizedRoom room in Rooms.Values)
            {
                room.ApplyLogicalOptions(logicalOptionsToApply);
            }
        }

        private ReadOnlyUnfinalizedInGameState _initialGameState;
        public ReadOnlyUnfinalizedInGameState InitialGameState {
            get { return _initialGameState; }
            set { _initialGameState = value?.Clone(); } 
        }

        /// <summary>
        /// Compares the two provided game states, using the internal comparer.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int CompareInGameStates(ReadOnlyUnfinalizedInGameState x, ReadOnlyUnfinalizedInGameState y)
        {
            return InGameStateComparer.Compare(x, y);
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
        public (T bestExecutable, UnfinalizedExecutionResult result) ExecuteBest<T>(IEnumerable<T> executables, ReadOnlyUnfinalizedInGameState initialInGameState, int times = 1,
            int previousRoomCount = 0, Predicate<ReadOnlyUnfinalizedInGameState> acceptationCondition = null) where T:IExecutableUnfinalized
        {
            // Try to execute all executables, returning whichever spends the lowest amount of resources
            (T bestExecutable, UnfinalizedExecutionResult result) bestResult = (default(T), null);
            foreach (T currentExecutable in executables)
            {
                UnfinalizedExecutionResult currentResult = currentExecutable.Execute(this, initialInGameState, times: times, previousRoomCount: previousRoomCount);

                // If the fulfillment was successful
                if (currentResult != null && (acceptationCondition == null || acceptationCondition.Invoke(currentResult.ResultingState)))
                {

                    // If the fulfillment did not reduce the amount of resources, return immediately
                    if (InGameStateComparer.Compare(currentResult.ResultingState, initialInGameState) == 0)
                    {
                        return (currentExecutable, currentResult);
                    }

                    // If the resulting state is the best we've found yet, retain it
                    if (bestResult.result == null
                        || InGameStateComparer.Compare(currentResult.ResultingState, bestResult.result.ResultingState) > 0)
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
        public UnfinalizedExecutionResult ExecuteAll(IEnumerable<IExecutableUnfinalized> executables, ReadOnlyUnfinalizedInGameState initialInGameState, int times = 1, int previousRoomCount = 0)
        {
            // If there are no executables, this is an instant success. Clone the inGameState to respect the contract.
            if(!executables.Any())
            {
                return new UnfinalizedExecutionResult(initialInGameState.Clone());
            }

            // Iterate over all executables, attempting to fulfill them
            UnfinalizedExecutionResult result = null;
            foreach (IExecutableUnfinalized currentExecutable in executables)
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
        public UnfinalizedInGameState CreateInitialGameStateCopy()
        {
            return InitialGameState.Clone();
        }
    }
}
