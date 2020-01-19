using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.InGameStates
{
    /// <summary>
    /// contains the logically-relevant attributes of a given in-game state.
    /// </summary>
    public class InGameState
    {
        /// <summary>
        /// Creates a new InGameState
        /// </summary>
        /// <param name="model">A SuperMetroidModel. Its rooms must have both been set and initialized. 
        /// Its items and game flags must also have been set.</param>
        /// <param name="itemContainer">The result of reading the items.json file.</param>
        public InGameState(SuperMetroidModel model, ItemContainer itemContainer)
        {
            IEnumerable<ResourceCapacity> startingResources = itemContainer.StartingResources;

            // Initialize max and current resource counts using starting resources
            foreach (ResourceCapacity capacity in startingResources)
            {
                ResourceMaximums.Add(capacity.Resource, capacity.MaxAmount);
                Resources.Add(capacity.Resource, capacity.MaxAmount);
            }

            // Initialize missing resources at current and max of 0
            foreach (RechargeableResourceEnum resource in ((RechargeableResourceEnum[])Enum.GetValues(typeof(RechargeableResourceEnum)))
                .Where(r => !ResourceMaximums.ContainsKey(r)))
            {
                ResourceMaximums.Add(resource, 0);
                Resources.Add(resource, 0);
            }

            // Initialize starting game flags
            foreach(string gameFlagName in itemContainer.StartingGameFlagNames)
            {
                AddGameFlag(model.GameFlags[gameFlagName]);
            }

            // Initialize starting items
            foreach (string itemName in itemContainer.StartingItemNames)
            {
                AddItem(model.Items[itemName]);
            }

            RoomNode startingNode = model.Rooms[itemContainer.StartingRoomName].Nodes[itemContainer.StartingNodeId];
            InRoomState = new InRoomState(startingNode);
        }

        public InGameState(InGameState other)
        {
            GameFlags = new Dictionary<string, GameFlag>(other.GameFlags);

            NonConsumableItems = new Dictionary<string, Item>(other.NonConsumableItems);
            ExpansionItems = new Dictionary<string, (ExpansionItem item, int count)>(other.ExpansionItems);

            ResourceMaximums = new Dictionary<RechargeableResourceEnum, int>(other.ResourceMaximums);

            Resources = new Dictionary<RechargeableResourceEnum, int>(other.Resources);

            InRoomState = new InRoomState(other.InRoomState);
        }

        protected IDictionary<RechargeableResourceEnum, int> ResourceMaximums { get; set; } = new Dictionary<RechargeableResourceEnum, int>();

        protected IDictionary<RechargeableResourceEnum, int> Resources { get; set; } = new Dictionary<RechargeableResourceEnum, int>();

        public int GetMaxAmount(RechargeableResourceEnum resource)
        {
            return ResourceMaximums[resource];
        }

        public int GetCurrentAmount(RechargeableResourceEnum resource)
        {
            return Resources[resource];
        }

        public bool IsResourceAvailable(ConsumableResourceEnum resource, int quantity)
        {
            return resource switch
            {
                // The other resources can be fully spent, but for energy we don't want to go below 1
                ConsumableResourceEnum.ENERGY => (GetCurrentAmount(RechargeableResourceEnum.RegularEnergy) + GetCurrentAmount(RechargeableResourceEnum.ReserveEnergy)) > quantity,
                ConsumableResourceEnum.MISSILE => GetCurrentAmount(RechargeableResourceEnum.Missile) >= quantity,
                ConsumableResourceEnum.SUPER => GetCurrentAmount(RechargeableResourceEnum.Super) >= quantity,
                ConsumableResourceEnum.POWER_BOMB => GetCurrentAmount(RechargeableResourceEnum.PowerBomb) >= quantity
            };
        }

        /// <summary>
        /// Consumes the provided quantity of the provided consumable resource. When consuming energy, regular energy is used up first (down to 1) 
        /// then reserves are used.
        /// </summary>
        /// <param name="resource">The resource to consume</param>
        /// <param name="quantity">The amount to consume</param>
        public void ConsumeResource(ConsumableResourceEnum resource, int quantity)
        {
            switch (resource)
            {
                case ConsumableResourceEnum.ENERGY:
                    // Consume regular energy first, down to 1
                    int regularEnergy = GetCurrentAmount(RechargeableResourceEnum.RegularEnergy);
                    int regularEnergyToConsume = regularEnergy > quantity ? quantity : regularEnergy - 1;
                    Resources[RechargeableResourceEnum.RegularEnergy] -= regularEnergyToConsume;
                    quantity -= regularEnergyToConsume;
                    if(regularEnergyToConsume > 0)
                    {
                        Resources[RechargeableResourceEnum.ReserveEnergy] -= quantity;
                    }
                    break;
                case ConsumableResourceEnum.MISSILE:
                    Resources[RechargeableResourceEnum.Missile] -= quantity;
                    break;
                case ConsumableResourceEnum.SUPER:
                    Resources[RechargeableResourceEnum.Super] -= quantity;
                    break;
                case ConsumableResourceEnum.POWER_BOMB:
                    Resources[RechargeableResourceEnum.PowerBomb] -= quantity;
                    break;
            }
        }

        /// <summary>
        /// Sets current value for the provided resource to the current maximum
        /// </summary>
        /// <param name="resource">The resource to refill</param>
        public void RefillResource(RechargeableResourceEnum resource)
        {
            Resources[resource] = ResourceMaximums[resource];
        }

        protected IDictionary<string, GameFlag> GameFlags { get; set; } = new Dictionary<string, GameFlag>();

        public bool HasGameflag(GameFlag flag)
        {
            return GameFlags.ContainsKey(flag.Name);
        }

        public bool HasGameFlag(string flagName)
        {
            return GameFlags.ContainsKey(flagName);
        }

        public void AddGameFlag(GameFlag flag)
        {
            if(!HasGameflag(flag))
            {
                GameFlags.Add(flag.Name, flag);
            }
        }

        protected IDictionary<string, Item> NonConsumableItems { get; set; } = new Dictionary<string, Item>();
        protected IDictionary<string, (ExpansionItem item, int count)> ExpansionItems { get; set; } = new Dictionary<string, (ExpansionItem item, int count)>();

        public bool HasItem(Item item)
        {
            return HasItem(item.Name);
        }

        public bool HasItem(string itemName)
        {
            return NonConsumableItems.ContainsKey(itemName) || ExpansionItems.ContainsKey(itemName);
        }

        public void AddItem(Item item)
        {
            // Expansion items have a count
            if(item is ExpansionItem expansionItem)
            {
                if (!ExpansionItems.ContainsKey(expansionItem.Name))
                {
                    // Add item with an initial quantity of 1
                    ExpansionItems.Add(expansionItem.Name, (expansionItem, 1));
                }
                else
                {
                    // Increment count
                    var itemWithCount = ExpansionItems[expansionItem.Name];
                    itemWithCount.count++;
                    ExpansionItems[expansionItem.Name] = itemWithCount;
                }
                // In either case, the inner maximum of the proper resource should increase
                ResourceMaximums[expansionItem.Resource] =
                    ResourceMaximums[expansionItem.Resource] + expansionItem.ResourceAmount;

                // Capacity pickups may add current resources as well, but they are not repeatable so by default we don't want logic to rely on them.
                // So we will not alter current resources.
            }
            // Regular items don't have a count
            else
            {
                if (!NonConsumableItems.ContainsKey(item.Name))
                {
                    NonConsumableItems.Add(item.Name, item);
                }
            }
        }

        protected InRoomState InRoomState { get; set; }

        /// <summary>
        /// The node the player is currently at. This can be null if in-room state isn't being tracked.
        /// </summary>
        public RoomNode CurrentNode { get => InRoomState.CurrentNode; }

        /// <summary>
        /// The room the player is currently in. This can be null if in-room state isn't being tracked.
        /// </summary>
        public Room CurrentRoom { get => InRoomState.CurrentRoom; }

        /// <summary>
        /// Positions the in-game state as it would be after entering a room via the provided node. This may place the player at a different node immediately
        /// if the node calls for it.
        /// </summary>
        /// <param name="entryNode"></param>
        public void EnterRoom(RoomNode entryNode)
        {
            InRoomState.EnterRoom(entryNode);
        }

        /// <summary>
        /// Positions the in-game state at the provided node. This node should be inside the current room.
        /// </summary>
        /// <param name="nodeToVisit">The node to go to</param>
        public void VisitNode(RoomNode nodeToVisit)
        {
            InRoomState.VisitNode(nodeToVisit);
        }

        /// <summary>
        /// Updates the in-room state to contain a mention of the destruction of the provided obstacle.
        /// This obstacle should be in the current room.
        /// </summary>
        /// <param name="obstacle">The obstacle to destroy.</param>
        public void DestroyObstacle(RoomObstacle obstacle)
        {
            InRoomState.DestroyObstacle(obstacle);
        }

        /// <summary>
        /// Removes all in-room data from this InGameState. Useful if this has been initialized at a starting node but in-room state is not going to be maintained.
        /// </summary>
        public void ClearRoomState()
        {
            InRoomState.ClearRoomState();
        }
    }
}
