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

namespace sm_json_data_framework.Rules
{
    /// <summary>
    /// A container for game start configuration.
    /// </summary>
    public class StartConditions
    {
        /// <summary>
        /// Creates and returns StartConditions representing the vanilla starting conditions.
        /// </summary>
        /// <param name="model">Model from which any needed Item/GameFlag/etc. instances will be obtained.</param>
        /// <returns>The StartConditions</returns>
        public static StartConditions CreateVanillaStartConditions(SuperMetroidModel model)
        {
            StartConditions vanillaStartConditions = new StartConditions();
            vanillaStartConditions.StartingInventory = ItemInventory.CreateVanillaStartingInventory(model);
            vanillaStartConditions.StartingResources = vanillaStartConditions.StartingInventory.BaseResourceMaximums.Clone();
            vanillaStartConditions.StartingNode = model.GetNodeInRoom("Ceres Elevator Room", 1);

            // Start with no open locks, taken items, or active game falgs, so we can leave those as their empty defaults

            return vanillaStartConditions;
        }

        public StartConditions()
        {

        }

        /// <summary>
        /// Constructor that constructs StartConditions using the provided model and the <see cref="BasicStartConditions"/> it contains.
        /// </summary>
        /// <param name="model">Model from which to construct StartConditions</param>
        public StartConditions(SuperMetroidModel model): this(model, model.BasicStartConditions)
        {

        }

        /// <summary>
        /// Constructor that constructs StartConditions using the provided model and basicStartConditions.
        /// </summary>
        /// <param name="model">Model from which to reference objects for the start conditions</param>
        /// <param name="overrideBasicStartConditions">The basic start conditions to use, regardless of whether
        /// this is the provided model's basicStartConditions or not.</param>
        public StartConditions(SuperMetroidModel model, BasicStartConditions overrideBasicStartConditions)
        {
            List<GameFlag> startingFlags = new List<GameFlag>();
            foreach (string flagName in overrideBasicStartConditions.StartingFlagNames)
            {
                if (!model.GameFlags.TryGetValue(flagName, out GameFlag flag))
                {
                    throw new Exception($"Starting game flag {flagName} not found.");
                }
                startingFlags.Add(flag);
            }

            List<NodeLock> startingLocks = new List<NodeLock>();
            foreach (string lockName in overrideBasicStartConditions.StartingLockNames)
            {
                if (!model.Locks.TryGetValue(lockName, out NodeLock nodeLock))
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

            ItemInventory startingInventory = new ItemInventory(startingResources);
            foreach (string itemName in overrideBasicStartConditions.StartingItemNames)
            {
                if (!model.Items.TryGetValue(itemName, out Item item))
                {
                    throw new Exception($"Starting item {itemName} not found.");
                }
                startingInventory.ApplyAddItem(item);
            }


            StartingNode = model.GetNodeInRoom(overrideBasicStartConditions.StartingRoomName, overrideBasicStartConditions.StartingNodeId);
            StartingGameFlags = startingFlags;
            StartingOpenLocks = startingLocks;
            // Default starting resource counts to the starting maximum
            StartingResources = startingResources.Clone();
            StartingInventory = startingInventory;
        }

        public StartConditions(StartConditions other)
        {
            StartingGameFlags = new List<GameFlag>(other.StartingGameFlags);
            StartingInventory = other.StartingInventory?.Clone();
            StartingNode = other.StartingNode;
            StartingOpenLocks = new List<NodeLock>(other.StartingOpenLocks);
            StartingResources = other.StartingResources?.Clone();
            StartingTakenItemLocations = new List<RoomNode>(other.StartingTakenItemLocations);
        }

        public StartConditions Clone()
        {
            return new StartConditions(this);
        }

        public RoomNode StartingNode { get; set; }

        private ItemInventory _itemInventory;
        public ItemInventory StartingInventory { get; set; }

        public ReadOnlyResourceCount BaseResourceMaximums { get { return StartingInventory?.BaseResourceMaximums; } }

        public ResourceCount StartingResources { get; set; }

        public IEnumerable<GameFlag> StartingGameFlags { get; set; } = Enumerable.Empty<GameFlag>();

        public IEnumerable<NodeLock> StartingOpenLocks { get; set; } = Enumerable.Empty<NodeLock>();

        public IEnumerable<RoomNode> StartingTakenItemLocations { get; set; } = Enumerable.Empty<RoomNode>();
    }
}
