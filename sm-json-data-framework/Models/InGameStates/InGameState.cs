using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Node;
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
        // STITCHME We oughta find a way to put unlocked locks in there. I imagine they would work quite exactly like game flags

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
            if(quantity == 0)
            {
                return true;
            }
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

        public bool HasGameFlag(GameFlag flag)
        {
            return GameFlags.ContainsKey(flag.Name);
        }

        public bool HasGameFlag(string flagName)
        {
            return GameFlags.ContainsKey(flagName);
        }

        public void AddGameFlag(GameFlag flag)
        {
            if(!HasGameFlag(flag))
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

        /// <summary>
        /// Returns specifically whether the Varia Suit is available in this in-game state.
        /// </summary>
        /// <returns></returns>
        public bool HasVariaSuit()
        {
            return HasItem("Varia");
        }

        /// <summary>
        /// Returns specifically whether the Gravity Suit is available in this in-game state.
        /// </summary>
        /// <returns></returns>
        public bool HasGravitySuit()
        {
            return HasItem("Gravity");
        }

        /// <summary>
        /// Returns specifically whether the Speed Booster is available in this in-game state.
        /// </summary>
        /// <returns></returns>
        public bool HasSpeedBooster()
        {
            return HasItem("SpeedBooster");
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

        /// <summary>
        /// In-room state of the current room.
        /// </summary>
        protected InRoomState InRoomState { get; set; }

        /// <summary>
        /// In-room state of the last room when it was left.
        /// </summary>
        protected InRoomState PreviousRoomState { get; set; }

        /// <summary>
        /// Returns the node the player is currently at. This can be null if in-room state isn't being tracked.
        /// </summary>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer.</param>
        /// <returns></returns>
        public RoomNode GetCurrentNode(bool usePreviousRoom = false)
        {
            InRoomState roomState = usePreviousRoom ? PreviousRoomState : InRoomState;
            return roomState?.CurrentNode;
        }

        /// <summary>
        /// Returns the room the player is currently in. This can be null if in-room state isn't being tracked.
        /// </summary>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer.</param>
        /// <returns></returns>
        public Room GetCurrentRoom(bool usePreviousRoom = false)
        {
            InRoomState roomState = usePreviousRoom ? PreviousRoomState : InRoomState;
            return roomState?.CurrentRoom;
        }

        /// <summary>
        /// Returns the strat that was used to reach the current node, if any. Otherwise, returns null.
        /// </summary>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer.</param>
        /// <returns></returns>
        public Strat GetLastStrat(bool usePreviousRoom = false)
        {
            InRoomState roomState = usePreviousRoom ? PreviousRoomState : InRoomState;
            return roomState?.LastStrat;
        }

        /// <summary>
        /// Returns a sequence of IDs of nodes that have been visited in the current room since entering, in order, 
        /// starting with the node through which the room was entered. May be empty if the in-room state is not being tracked.
        /// </summary>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer.</param>
        /// <returns></returns>
        public IEnumerable<int> GetVisitedNodeIds(bool usePreviousRoom = false)
        {
            InRoomState roomState = usePreviousRoom ? PreviousRoomState : InRoomState;
            IEnumerable<int> returnValue = roomState?.VisitedNodeIds;
            return returnValue == null? Enumerable.Empty<int>() : returnValue;
        }

        /// <summary>
        /// Returns a sequence of IDs of obstacles that have been destroyed in the current room since entering.
        /// May be empty if the in-room state is not being tracked.
        /// </summary>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer.</param>
        /// <returns></returns>
        public IEnumerable<string> GetDestroyedObstacleIds(bool usePreviousRoom = false)
        {
            InRoomState roomState = usePreviousRoom ? PreviousRoomState : InRoomState;
            IEnumerable<string> returnValue = roomState?.DestroyedObstacleIds;
            return returnValue == null ? Enumerable.Empty<string>() : returnValue;
        }

        /// <summary>
        /// Positions the in-game state as it would be after entering a room via the provided node. This may place the player at a different node immediately
        /// if the node calls for it.
        /// </summary>
        /// <param name="entryNode"></param>
        public void EnterRoom(RoomNode entryNode)
        {
            // Copy current room state and remember it as previous
            PreviousRoomState = new InRoomState(InRoomState);
            InRoomState.EnterRoom(entryNode);
        }

        /// <summary>
        /// Positions the in-game state at the provided node. This node should be inside the current room.
        /// </summary>
        /// <param name="nodeToVisit">The node to go to</param>
        /// <param name="strat">The strat through which the node is being reached. Can be null. If not null, only makes sense if 
        /// it's on a link that connects previous node to new node.</param>
        public void VisitNode(RoomNode nodeToVisit, Strat strat)
        {
            InRoomState.VisitNode(nodeToVisit, strat);
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
            PreviousRoomState?.ClearRoomState();
        }

        /// <summary>
        /// <para>Returns all runways that the player would be in a state to retroactively use, according to this in-game state.
        /// Does not check whether the player is able to use any strats on those runways.</para>
        /// <para>"Retroactive use" is meant to be done right after entering a room, and aims to retroactively decide how the last room was exited.</para>
        /// <para>A runway would typically be used retroactively to satisfy and adjacentRunway or canComeInCharged
        /// that is being executed at the first node of the new room.</para>
        /// </summary>
        /// <param name="inRoomPath">The path that must have been followed in the current room (as successive node IDs) in order to be able 
        /// to use retroactive runways in the current context. The first node in this path also dictates the node to which the retroactive runways must lead.</param>
        /// <param name="usePreviousRoom">If true, indicates that the "new" room is already the previous room in this InGamestate.</param>
        /// <returns></returns>
        public IEnumerable<Runway> GetRetroactiveRunways(IEnumerable<int> inRoomPath, bool usePreviousRoom = false)
        {
            // Since this is a retroactive check, we already have to look at the room prior to the one we're asked to evaluate
            // If we were already evaluating the previous room, we have no way to obtain the state of the room before that so just return
            if (usePreviousRoom)
            {
                return Enumerable.Empty<Runway>();
            }

            RoomNode currentNode = GetCurrentNode(usePreviousRoom);

            // The only runways that are usable are the ones on the node through which we left the previous room,
            // and they are only usable if that node led to the node through which we entered the current room.
            IEnumerable<int> visitedNodeIds = GetVisitedNodeIds(usePreviousRoom);
            // If we don't know at what node we entered, we can't identify any usable runways
            if (!visitedNodeIds.Any())
            {
                return Enumerable.Empty<Runway>();
            }

            // If the current in-room state doesn't respect the in-room path necessary to use retroactive runways, we can't use any
            if(!visitedNodeIds.SequenceEqual(inRoomPath))
            {
                return Enumerable.Empty<Runway>();
            }

            // If we didn't leave the previous room via a node that led to the node by which we entered current room, no runways are usable
            RoomNode entryNode = GetCurrentRoom(usePreviousRoom).Nodes[visitedNodeIds.First()];
            RoomNode previousRoomExitNode = GetCurrentNode(true);
            if (previousRoomExitNode?.OutNode != entryNode)
            {
                return Enumerable.Empty<Runway>();
            }

            // We've confirmed we can use retroactive runways. Return all runways of the previous room's exit node
            return previousRoomExitNode?.Runways;
        }
    }
}
