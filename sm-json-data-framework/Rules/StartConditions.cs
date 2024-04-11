using sm_json_data_framework.Models;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public StartConditions(ItemContainer itemContainer, SuperMetroidModel model)
        {
            StartingResources = new ResourceCount(itemContainer.StartingResources);

            // Initialize starting inventory. The json file's starting resources are also implicitly the starting maximum.
            StartingInventory = new ItemInventory(StartingResources);
            foreach (string itemName in itemContainer.StartingItemNames)
            {
                StartingInventory.ApplyAddItem(model.Items[itemName]);
            }

            StartingNode = model.Rooms[itemContainer.StartingRoomName].Nodes[itemContainer.StartingNodeId];

            // Initialize starting game flags
            StartingGameFlags = itemContainer.StartingGameFlagNames.Select(flagName => model.GameFlags[flagName]).ToList();

            // Initialize starting open locks
            StartingOpenLocks = itemContainer.StartingNodeLockNames.Select(lockName => model.Locks[lockName]).ToList();

            // items.json doesn't have the ability to express starting taken item locations, so leave it as its default empty list
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
