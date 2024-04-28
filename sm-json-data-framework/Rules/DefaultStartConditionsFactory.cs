using sm_json_data_framework.Models;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Raw.Items;
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
        public virtual StartConditions CreateStartConditions(SuperMetroidModel model, BasicStartConditions basicStartConditions)
        {
            List<GameFlag> startingFlags = new List<GameFlag>();
            foreach (string flagName in basicStartConditions.StartingFlagNames)
            {
                if (!model.GameFlags.TryGetValue(flagName, out GameFlag flag))
                {
                    throw new Exception($"Starting game flag {flagName} not found.");
                }
                startingFlags.Add(flag);
            }

            List<NodeLock> startingLocks = new List<NodeLock>();
            foreach (string lockName in basicStartConditions.StartingLockNames)
            {
                if (!model.Locks.TryGetValue(lockName, out NodeLock nodeLock))
                {
                    throw new Exception($"Starting node lock {lockName} not found.");
                }
                startingLocks.Add(nodeLock);
            }

            ResourceCount startingResources = new ResourceCount();
            foreach (RawResourceCapacity capacity in basicStartConditions.StartingResources)
            {
                startingResources.ApplyAmount(capacity.Resource, capacity.MaxAmount);
            }

            ItemInventory startingInventory = new ItemInventory(startingResources);
            foreach (string itemName in basicStartConditions.StartingItemNames)
            {
                if (!model.Items.TryGetValue(itemName, out Item item))
                {
                    throw new Exception($"Starting item {itemName} not found.");
                }
                startingInventory.ApplyAddItem(item);
            }

            StartConditions startConditions = new StartConditions
            {
                StartingNode = model.GetNodeInRoom(basicStartConditions.StartingRoomName, basicStartConditions.StartingNodeId),
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
