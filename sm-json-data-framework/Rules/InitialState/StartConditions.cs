using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Raw.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace sm_json_data_framework.Rules.InitialState
{
    /// <summary>
    /// A container for game start configuration.
    /// </summary>
    public class StartConditions
    {
        /// <summary>
        /// Creates and returns a StartConditionsBuilder primed to build a StartConditions representing the vanilla starting conditions.
        /// Calling is free to alter this before building.
        /// </summary>
        /// <param name="model">Model from which any needed Item/GameFlag/etc. instances will be obtained.</param>
        /// <returns>The StartConditionsBuilder</returns>
        public static StartConditionsBuilder CreateVanillaStartConditionsBuilder(SuperMetroidModel model)
        {
            ItemInventory startingInventory = ItemInventory.CreateVanillaStartingInventory(model);
            // Start with no open locks, taken items, or active game flags, so we can leave those as their empty defaults
            StartConditionsBuilder vanillaStartConditions = new StartConditionsBuilder(model)
                .StartingInventory(startingInventory)
                .StartingResources(startingInventory.BaseResourceMaximums.Clone())
                .StartingNode("Ceres Elevator Room", 1);

            return vanillaStartConditions;
        }

        /// <summary>
        /// Creates and returns StartConditions representing the vanilla starting conditions.
        /// </summary>
        /// <param name="model">Model from which any needed Item/GameFlag/etc. instances will be obtained.</param>
        /// <returns>The StartConditions</returns>
        public static StartConditions CreateVanillaStartConditions(SuperMetroidModel model)
        {
            ItemInventory startingInventory = ItemInventory.CreateVanillaStartingInventory(model);
            // Start with no open locks, taken items, or active game flags, so we can leave those as their empty defaults
            StartConditions vanillaStartConditions = new StartConditions(
                model: model,
                startingInventory: startingInventory,
                startingResources: startingInventory.BaseResourceMaximums.Clone(),
                startingNode: model.GetNodeInRoom("Ceres Elevator Room", 1)
                );

            return vanillaStartConditions;
        }

        public SuperMetroidModel Model { get; }

        public RoomNode StartingNode { get; }

        public ReadOnlyItemInventory StartingInventory { get; }

        public ReadOnlyResourceCount BaseResourceMaximums => StartingInventory?.BaseResourceMaximums;

        public ReadOnlyResourceCount StartingResources { get; }

        /// <summary>
        /// Game flags the game always starts with, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, GameFlag> StartingGameFlags { get; }

        /// <summary>
        /// Locks that are always unlocked at game start, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, NodeLock> StartingOpenLocks { get; }

        /// <summary>
        /// Item locations that are already taken at game start, mapped by name
        /// </summary>
        public IReadOnlyDictionary<string, RoomNode> StartingTakenItemLocations { get; }

        private StartConditions(SuperMetroidModel model)
        {
            Model = model;
        }

        public StartConditions(UnfinalizedStartConditions sourceElement, ModelFinalizationMappings mappings): this(mappings.Model)
        {
            StartingNode = sourceElement.StartingNode.Finalize(mappings);
            StartingInventory = new ItemInventory(sourceElement, mappings).AsReadOnly();
            StartingResources = sourceElement.StartingResources.Clone().AsReadOnly();
            StartingGameFlags = sourceElement.StartingGameFlags.Select(flag => flag.Finalize(mappings)).ToDictionary(flag => flag.Name).AsReadOnly();
            StartingOpenLocks = sourceElement.StartingOpenLocks.Select(nodeLock => nodeLock.Finalize(mappings)).ToDictionary(nodeLock => nodeLock.Name).AsReadOnly();
            StartingTakenItemLocations = sourceElement.StartingTakenItemLocations.Select(node => node.Finalize(mappings)).ToDictionary(node =>  node.Name).AsReadOnly();
        }

        public StartConditions(SuperMetroidModel model, RoomNode startingNode, ReadOnlyItemInventory startingInventory, ReadOnlyResourceCount startingResources,
            ICollection<GameFlag> startingGameFlags = null, ICollection<NodeLock> startingOpenLocks = null, ICollection<RoomNode> startingTakenItemLocations = null)
            : this(model)
        {
            StartingNode = startingNode;
            StartingInventory = startingInventory;
            StartingResources = startingResources;
            StartingGameFlags = startingGameFlags?.ToDictionary(flag => flag.Name).AsReadOnly() ?? new Dictionary<string, GameFlag>().AsReadOnly();
            StartingOpenLocks = startingOpenLocks?.ToDictionary(nodeLock => nodeLock.Name).AsReadOnly() ?? new Dictionary<string, NodeLock>().AsReadOnly();
            StartingTakenItemLocations = startingTakenItemLocations?.ToDictionary(node => node.Name).AsReadOnly() ?? new Dictionary<string, RoomNode>().AsReadOnly();

            // Ensure that all instances in this StartConditions actually exist in the model.
            if (Model.Nodes[StartingNode.Name] != StartingNode)
            {
                throw new ModelElementMismatchException(StartingNode);
            }

            foreach (Item item in StartingInventory.ExpansionItems.Values.Select(pair => pair.item))
            {
                if (Model.Items[item.Name] != item)
                {
                    throw new ModelElementMismatchException(item);
                }
            }
            foreach (Item item in StartingInventory.NonConsumableItems.Values)
            {
                if (Model.Items[item.Name] != item)
                {
                    throw new ModelElementMismatchException(item);
                }
            }

            foreach(GameFlag flag in StartingGameFlags.Values)
            {
                if (Model.GameFlags[flag.Name] != flag)
                {
                    throw new ModelElementMismatchException(flag);
                }
            }

            foreach(NodeLock nodeLock in StartingOpenLocks.Values)
            {
                if (Model.Locks[nodeLock.Name] != nodeLock)
                {
                    throw new ModelElementMismatchException(nodeLock);
                }
            }

            foreach (RoomNode node in StartingTakenItemLocations.Values)
            {
                if (Model.Nodes[node.Name] != node)
                {
                    throw new ModelElementMismatchException(node);
                }
            }
        }

        public StartConditions(StartConditions other): this(other.Model)
        {
            StartingNode = other.StartingNode;
            StartingInventory = other.StartingInventory.Clone().AsReadOnly();
            StartingResources = other.StartingResources.Clone().AsReadOnly();
            StartingGameFlags = other.StartingGameFlags?.Values.ToDictionary(flag => flag.Name).AsReadOnly() ?? new Dictionary<string, GameFlag>().AsReadOnly();
            StartingOpenLocks = other.StartingOpenLocks?.Values.ToDictionary(nodeLock => nodeLock.Name).AsReadOnly() ?? new Dictionary<string, NodeLock>().AsReadOnly();
            StartingTakenItemLocations = other.StartingTakenItemLocations?.Values.ToDictionary(node => node.Name).AsReadOnly() ?? new Dictionary<string, RoomNode>().AsReadOnly();
        }

        public StartConditions Clone()
        {
            return new StartConditions(this);
        }
    }

    public class StartConditionsBuilder
    {
        private SuperMetroidModel _model;

        private RoomNode _startingNode;

        private ItemInventory _startingInventory;

        private ResourceCount _startingResources;

        private ResourceCount _baseResourceMaximums;

        private IList<GameFlag> _startingGameFlags = new List<GameFlag>();

        private IList<NodeLock> _startingOpenLocks = new List<NodeLock>();

        private IList<RoomNode> _startingTakenItemLocations = new List<RoomNode>();

        public StartConditionsBuilder(SuperMetroidModel model)
        {
            _model = model;
        }

        public StartConditionsBuilder StartingNode(RoomNode startingNode)
        {
            _startingNode = startingNode;
            return this;
        }

        public StartConditionsBuilder StartingNode(string roomName, int nodeId)
        {
            _startingNode = _model.Rooms[roomName].Nodes[nodeId];
            return this;
        }

        /// <summary>
        /// Assigns the provided inventory as this builder's starting inventory.
        /// Take note that if base resource maximums have been assigned to this builder, they'll have priority over the inventory's.
        /// </summary>
        /// <param name="startingInventory"></param>
        /// <returns></returns>
        public StartConditionsBuilder StartingInventory(ItemInventory startingInventory)
        {
            _startingInventory = startingInventory;
            return this;
        }

        public StartConditionsBuilder StartingResources(ResourceCount startingResources)
        {
            _startingResources = startingResources;
            return this;
        }

        public StartConditionsBuilder StartingGameFlags(ICollection<GameFlag> startingGameFlags)
        {
            _startingGameFlags = new List<GameFlag>(startingGameFlags);
            return this;
        }

        public StartConditionsBuilder StartingGameFlags(ICollection<string> startingGameFlagNames)
        {
            _startingGameFlags = startingGameFlagNames.Select(flagName => _model.GameFlags[flagName]).ToList();
            return this;
        }

        public StartConditionsBuilder StartingOpenLocks(ICollection<NodeLock> startingOpenLocks)
        {
            _startingOpenLocks = new List<NodeLock>(startingOpenLocks);
            return this;
        }

        public StartConditionsBuilder StartingOpenLocks(ICollection<string> startingOpenLockNames)
        {
            _startingOpenLocks = startingOpenLockNames.Select(lockName => _model.Locks[lockName]).ToList();
            return this;
        }

        public StartConditionsBuilder StartingTakenItemLocations(ICollection<RoomNode> startingTakenItemLocation)
        {
            _startingTakenItemLocations = new List<RoomNode>(startingTakenItemLocation);
            return this;
        }

        public StartConditionsBuilder StartingTakenItemLocations(ICollection<(string roomName, int nodeId)> startingTakenItemLocation)
        {
            _startingTakenItemLocations = startingTakenItemLocation.Select(pair => _model.Rooms[pair.roomName].Nodes[pair.nodeId]).ToList();
            return this;
        }

        /// <summary>
        /// Assigns base resource maximums. If not null, this will have priority over whatever's set in the starting inventory
        /// </summary>
        /// <param name="baseResourceMaximums"></param>
        /// <returns></returns>
        public StartConditionsBuilder BaseResourceMaximums(ResourceCount baseResourceMaximums)
        {
            _baseResourceMaximums = baseResourceMaximums;
            return this;
        }

        public StartConditions Build()
        {
            ItemInventory startingInventory = _baseResourceMaximums == null
                ? _startingInventory : _startingInventory?.WithBaseResourceMaximums(_baseResourceMaximums);
            return new StartConditions(
                model: _model,
                startingNode: _startingNode,
                startingInventory: startingInventory?.AsReadOnly(),
                startingResources: _startingResources?.AsReadOnly(),
                startingGameFlags: _startingGameFlags?.AsReadOnly(),
                startingOpenLocks: _startingOpenLocks?.AsReadOnly(),
                startingTakenItemLocations: _startingTakenItemLocations?.AsReadOnly()
            );
        }
    }

    /// <summary>
    /// A container for game start configuration.
    /// </summary>
    public class UnfinalizedStartConditions
    {
        /// <summary>
        /// Creates and returns StartConditions representing the vanilla starting conditions.
        /// </summary>
        /// <param name="model">Model from which any needed Item/GameFlag/etc. instances will be obtained.</param>
        /// <returns>The StartConditions</returns>
        public static UnfinalizedStartConditions CreateVanillaStartConditions(UnfinalizedSuperMetroidModel model)
        {
            UnfinalizedStartConditions vanillaStartConditions = new UnfinalizedStartConditions();
            vanillaStartConditions.StartingInventory = UnfinalizedItemInventory.CreateVanillaStartingUnfinalizedInventory(model);
            vanillaStartConditions.BaseResourceMaximums = ResourceCount.CreateVanillaBaseResourceMaximums();
            vanillaStartConditions.StartingResources = vanillaStartConditions.BaseResourceMaximums.Clone();
            vanillaStartConditions.StartingNode = model.GetNodeInRoom("Ceres Elevator Room", 1);

            // Start with no open locks, taken items, or active game falgs, so we can leave those as their empty defaults

            return vanillaStartConditions;
        }

        public UnfinalizedStartConditions()
        {

        }

        /// <summary>
        /// Constructor that constructs StartConditions using the provided model and the <see cref="BasicStartConditions"/> it contains.
        /// </summary>
        /// <param name="model">Model from which to construct StartConditions</param>
        public UnfinalizedStartConditions(UnfinalizedSuperMetroidModel model) : this(model, model.BasicStartConditions)
        {

        }

        /// <summary>
        /// Constructor that constructs StartConditions using the provided model and basicStartConditions.
        /// </summary>
        /// <param name="model">Model from which to reference objects for the start conditions</param>
        /// <param name="overrideBasicStartConditions">The basic start conditions to use, regardless of whether
        /// this is the provided model's basicStartConditions or not.</param>
        public UnfinalizedStartConditions(UnfinalizedSuperMetroidModel model, BasicStartConditions overrideBasicStartConditions)
        {
            List<UnfinalizedGameFlag> startingFlags = new List<UnfinalizedGameFlag>();
            foreach (string flagName in overrideBasicStartConditions.StartingFlagNames)
            {
                if (!model.GameFlags.TryGetValue(flagName, out UnfinalizedGameFlag flag))
                {
                    throw new Exception($"Starting game flag {flagName} not found.");
                }
                startingFlags.Add(flag);
            }

            List<UnfinalizedNodeLock> startingLocks = new List<UnfinalizedNodeLock>();
            foreach (string lockName in overrideBasicStartConditions.StartingLockNames)
            {
                if (!model.Locks.TryGetValue(lockName, out UnfinalizedNodeLock nodeLock))
                {
                    throw new Exception($"Starting node lock {lockName} not found.");
                }
                startingLocks.Add(nodeLock);
            }

            ResourceCount startingResources = new ResourceCount();
            foreach (RawResourceCapacity capacity in overrideBasicStartConditions.StartingResources)
            {
                startingResources.ApplyAmount(capacity.Resource, capacity.MaxAmount);
            }

            UnfinalizedItemInventory startingInventory = new UnfinalizedItemInventory();
            foreach (string itemName in overrideBasicStartConditions.StartingItemNames)
            {
                if (!model.Items.TryGetValue(itemName, out UnfinalizedItem item))
                {
                    throw new Exception($"Starting item {itemName} not found.");
                }
                startingInventory.ApplyAddItem(item);
            }


            StartingNode = model.GetNodeInRoom(overrideBasicStartConditions.StartingRoomName, overrideBasicStartConditions.StartingNodeId);
            StartingGameFlags = startingFlags;
            StartingOpenLocks = startingLocks;
            // Default base maximums starting resource counts to the starting resources
            BaseResourceMaximums = startingResources.Clone();
            StartingResources = startingResources.Clone();
            StartingInventory = startingInventory;
        }

        public UnfinalizedStartConditions(UnfinalizedStartConditions other)
        {
            BaseResourceMaximums = other.BaseResourceMaximums.Clone();
            StartingGameFlags = new List<UnfinalizedGameFlag>(other.StartingGameFlags);
            StartingInventory = other.StartingInventory?.Clone();
            StartingNode = other.StartingNode;
            StartingOpenLocks = new List<UnfinalizedNodeLock>(other.StartingOpenLocks);
            StartingResources = other.StartingResources?.Clone();
            StartingTakenItemLocations = new List<UnfinalizedRoomNode>(other.StartingTakenItemLocations);
        }

        public UnfinalizedStartConditions Clone()
        {
            return new UnfinalizedStartConditions(this);
        }

        public StartConditions Finalize(ModelFinalizationMappings mappings)
        {
            return new StartConditions(this, mappings);
        }

        public UnfinalizedRoomNode StartingNode { get; set; }

        private UnfinalizedItemInventory _itemInventory;
        public UnfinalizedItemInventory StartingInventory { get; set; }

        public ReadOnlyResourceCount BaseResourceMaximums { get; set; }

        public ResourceCount StartingResources { get; set; }

        public IEnumerable<UnfinalizedGameFlag> StartingGameFlags { get; set; } = Enumerable.Empty<UnfinalizedGameFlag>();

        public IEnumerable<UnfinalizedNodeLock> StartingOpenLocks { get; set; } = Enumerable.Empty<UnfinalizedNodeLock>();

        public IEnumerable<UnfinalizedRoomNode> StartingTakenItemLocations { get; set; } = Enumerable.Empty<UnfinalizedRoomNode>();
    }
}
