using sm_json_data_framework.Models;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Rules
{
    /// <summary>
    /// An IStartConditionsFactory that creates the start conditions based solely on the contents of items.json.
    /// If overriding this, it's recommended to call its default behavior and alter the start conditions afterwards.
    /// </summary>
    public class DefaultStartConditionsFactory: IStartConditionsFactory
    {
        public virtual StartConditions CreateStartConditions(SuperMetroidModel model, ItemContainer itemContainer)
        {
            List<GameFlag> startingFlags = new List<GameFlag>();
            foreach (string flagName in itemContainer.StartingGameFlagNames)
            {
                if (!model.GameFlags.TryGetValue(flagName, out GameFlag flag))
                {
                    throw new Exception($"Starting game flag {flagName} not found.");
                }
                startingFlags.Add(flag);
            }

            List<NodeLock> startingLocks = new List<NodeLock>();
            foreach (string lockName in itemContainer.StartingNodeLockNames)
            {
                if (!model.Locks.TryGetValue(lockName, out NodeLock nodeLock))
                {
                    throw new Exception($"Starting node lock {lockName} not found.");
                }
                startingLocks.Add(nodeLock);
            }

            ResourceCount startingResources = new ResourceCount();
            foreach (ResourceCapacity capacity in itemContainer.StartingResources)
            {
                startingResources.ApplyAmount(capacity.Resource, capacity.MaxAmount);
            }

            ItemInventory startingInventory = new ItemInventory(startingResources);
            foreach (string itemName in itemContainer.StartingItemNames)
            {
                if (!model.Items.TryGetValue(itemName, out Item item))
                {
                    throw new Exception($"Starting item {itemName} not found.");
                }
                startingInventory.ApplyAddItem(item);
            }

            StartConditions startConditions = new StartConditions
            {
                StartingNode = model.GetNodeInRoom(itemContainer.StartingRoomName, itemContainer.StartingNodeId),
                StartingGameFlags = startingFlags,
                StartingOpenLocks = startingLocks,
                // Default starting resource counts to the starting maximum
                StartingResources = startingResources.Clone(),
                StartingInventory = startingInventory
            };

            return startConditions;
        }
    }
}
