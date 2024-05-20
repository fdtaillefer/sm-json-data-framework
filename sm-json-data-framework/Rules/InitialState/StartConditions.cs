using sm_json_data_framework.Models;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
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
            StartConditionsBuilder vanillaStartConditions = new StartConditionsBuilder()
                .StartingInventory(startingInventory)
                .StartingResources(startingInventory.BaseResourceMaximums.Clone())
                .StartingNode(model.GetNodeInRoom("Ceres Elevator Room", 1));

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
                startingInventory: startingInventory,
                startingResources: startingInventory.BaseResourceMaximums.Clone(),
                startingNode: model.GetNodeInRoom("Ceres Elevator Room", 1)
                );

            return vanillaStartConditions;
        }

        public RoomNode StartingNode { get; }

        public ReadOnlyItemInventory StartingInventory { get; }

        public ReadOnlyResourceCount BaseResourceMaximums => StartingInventory?.BaseResourceMaximums;

        public ReadOnlyResourceCount StartingResources { get; }

        public IReadOnlyList<GameFlag> StartingGameFlags { get; }

        public IReadOnlyList<NodeLock> StartingOpenLocks { get; }

        public IReadOnlyList<RoomNode> StartingTakenItemLocations { get; }

        public StartConditions(UnfinalizedStartConditions sourceElement, ModelFinalizationMappings mappings)
        {
            StartingNode = sourceElement.StartingNode.Finalize(mappings);
            StartingInventory = new ItemInventory(sourceElement, mappings).AsReadOnly();
            StartingResources = sourceElement.StartingResources.Clone().AsReadOnly();
            StartingGameFlags = sourceElement.StartingGameFlags.Select(flag => flag.Finalize(mappings)).ToList().AsReadOnly();
            StartingOpenLocks = sourceElement.StartingOpenLocks.Select(nodeLock => nodeLock.Finalize(mappings)).ToList().AsReadOnly();
            StartingTakenItemLocations = sourceElement.StartingTakenItemLocations.Select(node => node.Finalize(mappings)).ToList().AsReadOnly();
        }

        public StartConditions(RoomNode startingNode, ReadOnlyItemInventory startingInventory, ReadOnlyResourceCount startingResources,
            ICollection<GameFlag> startingGameFlags = null, ICollection<NodeLock> startingOpenLocks = null, ICollection<RoomNode> startingTakenItemLocations = null)
        {
            StartingNode = startingNode;
            StartingInventory = startingInventory;
            StartingResources = startingResources;
            StartingGameFlags = startingGameFlags?.ToList().AsReadOnly() ?? new List<GameFlag>().AsReadOnly();
            StartingOpenLocks = startingOpenLocks?.ToList().AsReadOnly() ?? new List<NodeLock>().AsReadOnly();
            StartingTakenItemLocations = startingTakenItemLocations?.ToList().AsReadOnly() ?? new List<RoomNode>().AsReadOnly();
        }

        public StartConditions(StartConditions other)
        {
            StartingNode = other.StartingNode;
            StartingInventory = other.StartingInventory.Clone().AsReadOnly();
            StartingResources = other.StartingResources.Clone().AsReadOnly();
            StartingGameFlags = other.StartingGameFlags?.ToList().AsReadOnly() ?? new List<GameFlag>().AsReadOnly();
            StartingOpenLocks = other.StartingOpenLocks?.ToList().AsReadOnly() ?? new List<NodeLock>().AsReadOnly();
            StartingTakenItemLocations = other.StartingTakenItemLocations?.ToList().AsReadOnly() ?? new List<RoomNode>().AsReadOnly();
        }

        public StartConditions Clone()
        {
            return new StartConditions(this);
        }
    }

    public class StartConditionsBuilder
    {
        private RoomNode _startingNode;

        private ItemInventory _startingInventory;

        private ResourceCount _startingResources;

        private IList<GameFlag> _startingGameFlags = new List<GameFlag>();

        private IList<NodeLock> _startingOpenLocks = new List<NodeLock>();

        private IList<RoomNode> _startingTakenItemLocations = new List<RoomNode>();

        public StartConditionsBuilder StartingNode(RoomNode startingNode)
        {
            _startingNode = startingNode;
            return this;
        }

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

        public StartConditionsBuilder StartingOpenLocks(ICollection<NodeLock> startingOpenLocks)
        {
            _startingOpenLocks = new List<NodeLock>(startingOpenLocks);
            return this;
        }

        public StartConditionsBuilder StartingTakenItemLocations(ICollection<RoomNode> startingTakenItemLocation)
        {
            _startingTakenItemLocations = new List<RoomNode>(startingTakenItemLocation);
            return this;
        }

        /// <summary>
        /// Assigns base resource maximums. Because this data is in the inventory, this will do nothing if there is not
        /// inventory in this builder
        /// </summary>
        /// <param name="baseResourceMaximums"></param>
        /// <returns></returns>
        public StartConditionsBuilder BaseResourceMaximums(ResourceCount baseResourceMaximums)
        {
            if (_startingInventory != null)
            {
                _startingInventory = _startingInventory.WithBaseResourceMaximums(baseResourceMaximums);
            }
            return this;
        }

        public StartConditions Build()
        {
            return new StartConditions(
                startingNode: _startingNode,
                startingInventory: _startingInventory.AsReadOnly(),
                startingResources: _startingResources.AsReadOnly(),
                startingGameFlags: _startingGameFlags.AsReadOnly(),
                startingOpenLocks: _startingOpenLocks.AsReadOnly(),
                startingTakenItemLocations: _startingTakenItemLocations.AsReadOnly()
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
