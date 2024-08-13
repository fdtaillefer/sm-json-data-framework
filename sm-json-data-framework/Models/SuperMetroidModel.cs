using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Navigation;
using sm_json_data_framework.Models.Raw;
using sm_json_data_framework.Models.Raw.Connections;
using sm_json_data_framework.Models.Raw.Helpers;
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
using sm_json_data_framework.Rules.InitialState;
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

        public const string SHINESPARK_TECH_NAME = "canShinespark";
        public const string USE_RESERVES_FOR_SHINESPARK_TECH_NAME = "canShinesparkWithReserve";

        public SuperMetroidModel(UnfinalizedSuperMetroidModel sourceModel, LogicalOptions logicalOptions = null)
        {
            ModelFinalizationMappings mappings = new ModelFinalizationMappings(this);
            Items = sourceModel.Items.Values.Select(item => item.Finalize(mappings)).ToDictionary(item => item.Name).AsReadOnly();
            GameFlags = sourceModel.GameFlags.Values.Select(flag => flag.Finalize(mappings)).ToDictionary(flag => flag.Name).AsReadOnly();
            Weapons = sourceModel.Weapons.Values.Select(weapon => weapon.Finalize(mappings)).ToDictionary(weapon => weapon.Name).AsReadOnly();
            WeaponsByCategory = sourceModel.WeaponsByCategory
                .Select(kvp => new KeyValuePair<WeaponCategoryEnum, IReadOnlyList<Weapon>>(kvp.Key, kvp.Value.Select(weapon => weapon.Finalize(mappings)).ToList().AsReadOnly()))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value).AsReadOnly();
            Enemies = sourceModel.Enemies.Values.Select(enemy => enemy.Finalize(mappings)).ToDictionary(enemy => enemy.Name).AsReadOnly();
            Helpers = sourceModel.Helpers.Values.Select(helper => helper.Finalize(mappings)).ToDictionary(helper => helper.Name).AsReadOnly();
            TechCategories = sourceModel.TechCategories.Values.Select(techCategory => techCategory.Finalize(mappings)).ToDictionary(techCategory => techCategory.Name).AsReadOnly();
            Techs = sourceModel.Techs.Values.Select(tech => tech.Finalize(mappings)).ToDictionary(tech => tech.Name).AsReadOnly();
            Rooms = sourceModel.Rooms.Values.Select(room => room.Finalize(mappings)).ToDictionary(room => room.Name).AsReadOnly();
            Nodes = sourceModel.Nodes.Values.Select(node => node.Finalize(mappings)).ToDictionary(node => node.Name).AsReadOnly();
            Runways = sourceModel.Runways.Values.Select(runway => runway.Finalize(mappings)).ToDictionary(runway => runway.Name).AsReadOnly();
            Locks = sourceModel.Locks.Values.Select(locks => locks.Finalize(mappings)).ToDictionary(locks => locks.Name).AsReadOnly();
            RoomEnemies = sourceModel.RoomEnemies.Values.Select(roomEnemy => roomEnemy.Finalize(mappings)).ToDictionary(roomEnemy => roomEnemy.GroupName).AsReadOnly();
            Connections = sourceModel.Connections.Select(kvp => new KeyValuePair<string, Connection>(kvp.Key, kvp.Value.Finalize(mappings)))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value).AsReadOnly();
            Rules = sourceModel.Rules;
            InternalStartConditions = sourceModel.StartConditions.Finalize(mappings);

            // Initialize logical state so that all elements of the model always have logical options available
            ApplyLogicalOptions(logicalOptions, mappings);
        }

        /// <summary>
        /// The logical options currently applied to this model.
        /// Note that the <see cref="StartConditions"/> within this instance is never null.
        /// </summary>
        public ReadOnlyLogicalOptions AppliedLogicalOptions { get; private set; }
        // STITCHME Maybe move these to Logical Options?
        /// <summary>
        /// An item inventory that contains all items that could be obtained according to currently applied logical options (except for expansion item counts) .
        /// </summary>
        public ReadOnlyItemInventory BestCaseInventory { get; private set; }
        /// <summary>
        /// An item inventory that contains only items that Samus starts with according to currently applied logical options.
        /// </summary>
        public ReadOnlyItemInventory WorstCaseInventory { get; private set; }
        /// <summary>
        /// An inventory with just Varia, made available as a miscellaneous tool for some calculations.
        /// </summary>
        public ReadOnlyItemInventory VariaOnlyInventory { get; private set; }
        /// <summary>
        /// An inventory with just Gravity, made available as a miscellaneous tool for some calculations.
        /// </summary>
        public ReadOnlyItemInventory GravityOnlyInventory { get; private set; }
        /// <summary>
        /// An inventory with just Varia and Gravity, made available as a miscellaneous tool for some calculations.
        /// </summary>
        public ReadOnlyItemInventory VariaGravityOnlyInventory { get; private set; }

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
        /// The tech categories in this model, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, TechCategory> TechCategories { get; }

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
        /// Describes the start conditions for the game, complete with relevant objects within this model.
        /// May have been overridden by applied logical options.
        /// </summary>
        public StartConditions StartConditions => AppliedLogicalOptions.StartConditions;

        /// <summary>
        /// The start conditions that will be defaulted to if not overridden by logical options.
        /// </summary>
        protected StartConditions InternalStartConditions { get; }

        /// <summary>
        /// An InGameStateComparer, which can be used to decide which of two InGameStates has "better" resources.
        /// </summary>
        public InGameStateComparer InGameStateComparer => AppliedLogicalOptions.InGameStateComparer;

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
        /// Internal method for applying logical options on this model.
        /// This is able to interpret <see cref="UnfinalizedStartConditions"/> in the logical options.
        /// </summary>
        /// <param name="logicalOptions">The LogicalOptions to apply.</param>
        /// <param name="mappings">If model finalization is ongoing, these are the mappings used for it. Leave null otherwise.
        /// If null, this instead removes all alterations made by logical options, by applying default logical options.</param>
        private void ApplyLogicalOptions(LogicalOptions logicalOptions, ModelFinalizationMappings mappings)
        {
            AppliedLogicalOptions = PrepareLogicalOptionsToApply(logicalOptions, mappings);

            ItemInventory worstCaseInventory = AppliedLogicalOptions.StartConditions.StartingInventory.Clone();
            IEnumerable<Item> bestCaseItems = Items.Keys
                            .Except(AppliedLogicalOptions.RemovedItems)
                            .Select(itemName => Items[itemName]);
            ItemInventory bestCaseInventory = new ItemInventory(AppliedLogicalOptions.StartConditions.StartingInventory.Clone());
            foreach(Item item in bestCaseItems)
            {
                bestCaseInventory.ApplyAddItem(item);
            }
            WorstCaseInventory = worstCaseInventory;
            BestCaseInventory = bestCaseInventory;
            VariaOnlyInventory = new ItemInventory(AppliedLogicalOptions.StartConditions.StartingInventory.Clone())
                .ApplyAddItem(Items[SuperMetroidModel.VARIA_SUIT_NAME]);
            GravityOnlyInventory = new ItemInventory(AppliedLogicalOptions.StartConditions.StartingInventory.Clone())
                .ApplyAddItem(Items[SuperMetroidModel.GRAVITY_SUIT_NAME]);
            VariaGravityOnlyInventory = new ItemInventory(AppliedLogicalOptions.StartConditions.StartingInventory.Clone())
                .ApplyAddItem(Items[SuperMetroidModel.VARIA_SUIT_NAME])
                .ApplyAddItem(Items[SuperMetroidModel.GRAVITY_SUIT_NAME]);

            foreach (GameFlag gameFlag in GameFlags.Values)
            {
                gameFlag.ApplyLogicalOptions(AppliedLogicalOptions, this);
            }

            foreach (Helper helper in Helpers.Values)
            {
                helper.ApplyLogicalOptions(AppliedLogicalOptions, this);
            }

            foreach (Item item in Items.Values)
            {
                item.ApplyLogicalOptions(AppliedLogicalOptions, this);
            }

            foreach (Tech tech in Techs.Values)
            {
                tech.ApplyLogicalOptions(AppliedLogicalOptions, this);
            }

            foreach (TechCategory techCategory in TechCategories.Values)
            {
                techCategory.ApplyLogicalOptions(AppliedLogicalOptions, this);
            }

            foreach (Weapon weapon in Weapons.Values)
            {
                weapon.ApplyLogicalOptions(AppliedLogicalOptions, this);
            }

            foreach (Enemy enemy in Enemies.Values)
            {
                enemy.ApplyLogicalOptions(AppliedLogicalOptions, this);
            }

            foreach (Connection connection in Connections.Values)
            {
                connection.ApplyLogicalOptions(AppliedLogicalOptions, this);
            }

            foreach (Room room in Rooms.Values)
            {
                room.ApplyLogicalOptions(AppliedLogicalOptions, this);
            }
        }

        /// <summary>
        /// Does all pre-processing on the provided logical options to apply, including cloning, replacing nulls with defaults, 
        /// and enforcing consistency on some redundant values. Does not apply the logical options.
        /// </summary>
        /// <param name="logicalOptions">Logical options to prepare</param>
        /// <param name="mappings">If model finalization is ongoing, these are the mappings used for it. Null otherwise.</param>
        /// <returns>The new, prepares instance as ReadOnlyLogicalOptions</returns>
        private ReadOnlyLogicalOptions PrepareLogicalOptionsToApply(LogicalOptions logicalOptions, ModelFinalizationMappings mappings)
        {
            // Clone the logical options. If null, clone the default logical options instead.
            // Once we've cloned, we're free to make any alterations we want without impacting the calling.
            logicalOptions = (logicalOptions ?? LogicalOptions.DefaultLogicalOptions).Clone();

            // If we're applying logical options during model finalization, then we must interpret only the options' UnfinalizedStartConditions
            // If it's not during model finalization, then we must interpret only the options' StartConditions
            if (mappings != null)
            {
                logicalOptions.InternalStartConditions = logicalOptions.InternalUnfinalizedStartConditions?.Finalize(mappings);
            }
            logicalOptions.InternalUnfinalizedStartConditions = null;

            // We want the operation that applies logical options to have access to whatever the StartConditions are,
            // so even though they're optional, we'll replace them with the model's internal StartConditions if null
            logicalOptions.InternalStartConditions ??= InternalStartConditions;

            // If we're applying logical options during model finalization, then we must interpret only the options' UnfinalizedAvailableResourceInventory
            // If it's not during model finalization, then we must interpret only the options' AvailableResourceInventory
            if (mappings != null)
            {
                if (logicalOptions.InternalUnfinalizedAvailableResourceInventory == null)
                {
                    logicalOptions.InternalAvailableResourceInventory = null;
                }
                else
                {
                    logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(logicalOptions.InternalUnfinalizedAvailableResourceInventory,
                        logicalOptions.InternalStartConditions.BaseResourceMaximums, mappings);
                }
            }
            logicalOptions.InternalUnfinalizedAvailableResourceInventory = null;

            // Enforce base resource maximum consistency
            if(logicalOptions.InternalAvailableResourceInventory != null)
            {
                logicalOptions.InternalAvailableResourceInventory 
                    = logicalOptions.InternalAvailableResourceInventory.WithBaseResourceMaximums(logicalOptions.StartConditions.BaseResourceMaximums);
            }

            return logicalOptions.AsReadOnly();
        }

        /// <summary>
        /// Clones the provided LogicalOptions, and applies them to this model.
        /// </summary>
        /// <param name="logicalOptions">The LogicalOptions to apply.
        /// If null, this instead removes all alterations made by logical options, by applying default logical options.</param>
        public void ApplyLogicalOptions(LogicalOptions logicalOptions)
        {
            ApplyLogicalOptions(logicalOptions, null);
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
        /// Creates and returns a copy of the initial game state.
        /// </summary>
        /// <returns></returns>
        public InGameState CreateInitialGameState()
        {
            return new InGameState(StartConditions);
        }

        /// <summary>
        /// Creates and returns a game navigator at the starting location and with starting resources.
        /// </summary>
        /// <param name="maxPreviousStatesSize">The maximum number of previous states that the created navigator
        /// should keep in memory.</param>
        /// <returns></returns>
        public GameNavigator CreateInitialGameNavigator(int maxPreviousStatesSize)
        {
            return new GameNavigator(this, CreateInitialGameState(), maxPreviousStatesSize);
        }
    }

    /// <summary>
    /// <para>
    /// An unfinalized Super Metroid model, representing the game's ROM. It has fully-formed model elements that cross-reference each other, 
    /// but allows modifications (at the caller's own risk of introducing inconsistencies). 
    /// </para>
    /// <para>
    /// An UnfinalizedSuperMetroidModel can be finalized into a <see cref="SuperMetroidModel"/>.
    /// </para>
    /// </summary>
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
        /// <param name="overrideObjectTypes">A sequence of tuples, pairing together an ObjectLogicalElementTypeEnum and the C# type that should be used to 
        /// to represent that ObjectLogicalElementTypeEnum when converting logical requirements from a raw equivalent.
        /// The provided C# types must extend the default type that is normally used for any given ObjectLogicalElementTypeEnum.</param>
        /// <param name="overrideStringTypes">A sequence of tuples, pairing together a StringLogicalElementTypeEnum and the C# type that should be used to 
        /// to represent that StringLogicalElementTypeEnum when converting logical requirements from a raw equivalent.
        /// The provided C# types must extend the default type that is normally used for any given StringLogicalElementTypeEnum.</param>
        /// <exception cref="Exception">If this method fails to interpret any logical element</exception>
        public UnfinalizedSuperMetroidModel(RawSuperMetroidModel rawModel, SuperMetroidRules rules = null,
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
            BasicStartConditions = new BasicStartConditions(rawModel.ItemContainer);

            // Put helpers in model
            Helpers = rawModel.HelperContainer.Helpers.Select(rawHelper => new UnfinalizedHelper(rawHelper)).ToDictionary(h => h.Name);

            // Put techs and techCategories in model
            TechCategories = rawModel.TechContainer.TechCategories
                .Select(rawTechCategory => new UnfinalizedTechCategory(rawTechCategory)).ToDictionary(techCategory => techCategory.Name);
            Techs = TechCategories.Values.SelectMany(techCategory => techCategory.Techs)
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

        /// <summary>
        /// Finalizes this model into a SuperMetroidModel, whose rom-equivalent data can no longer be modified.
        /// </summary>
        /// <param name="logicalOptions">A set of logical options to apply.
        /// Can be null, in which case default options (allowing pretty much everything) will be used.</param>
        /// <returns></returns>
        public SuperMetroidModel Finalize(LogicalOptions logicalOptions = null)
        {
            return new SuperMetroidModel(this, logicalOptions);
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
        /// Tech tech categories in this model, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedTechCategory> TechCategories { get; set; } = new Dictionary<string, UnfinalizedTechCategory>();

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
            get => _weapons;
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
    }
}
