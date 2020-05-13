using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Node;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.InGameStates
{
    /// <summary>
    /// Contains the logically-relevant attributes of a given in-game state.
    /// </summary>
    public class InGameState
    {
        // STITCHME It might be valuable to eventually have InGameState be able to say which nodes are reachable?

        // STITCHME It could be nice to keep track of all canResets in the room and evaluate them as you move around?
        // Another option would be to have something in an initialization phase that converts canResets into just names,
        // and adds information on nodes and strats that they invalidate the canReset.
        // We'll see when we get to the step of reducing logical elements *shrug*

        /// <summary>
        /// Creates a new InGameState
        /// </summary>
        /// <param name="model">A SuperMetroidModel. Its rooms must have both been set and initialized. 
        /// Its items and game flags must also have been set.</param>
        /// <param name="itemContainer">The result of reading the items.json file.</param>
        public InGameState(SuperMetroidModel model, ItemContainer itemContainer)
        {
            IEnumerable<ResourceCapacity> startingResources = itemContainer.StartingResources;

            Inventory = new ItemInventory(startingResources);

            Resources = new ResourceCount();

            // Start the player at full
            foreach (ResourceCapacity capacity in startingResources)
            {
                Resources.ApplyAmountIncrease(capacity.Resource, capacity.MaxAmount);
            }

            // Initialize starting game flags
            foreach(string gameFlagName in itemContainer.StartingGameFlagNames)
            {
                ApplyAddGameFlag(model.GameFlags[gameFlagName]);
            }

            // Initialize starting items
            foreach (string itemName in itemContainer.StartingItemNames)
            {
                ApplyAddItem(model.Items[itemName]);
            }

            RoomNode startingNode = model.Rooms[itemContainer.StartingRoomName].Nodes[itemContainer.StartingNodeId];
            InRoomState = new InRoomState(startingNode);
        }

        /// <summary>
        /// A copy constructor that creates a new InGameState based on the provided one.
        /// This is a somewhat shallow copy; referenced objects whose inner state does not change with a game state (such as Room, GameFlag, etc.) will not be copied.
        /// The inner InRoomState and anything else that fully belongs to the InGameState does get copied.
        /// </summary>
        /// <param name="other">The InGameState to copy</param>
        public InGameState(InGameState other)
        {
            ActiveGameFlags = new Dictionary<string, GameFlag>(other.ActiveGameFlags);

            TakenItemLocations = new Dictionary<string, RoomNode>(other.TakenItemLocations);

            OpenedLocks = new Dictionary<String, NodeLock>(other.OpenedLocks);

            Inventory = other.Inventory.Clone();

            Resources = other.Resources.Clone();

            InRoomState = new InRoomState(other.InRoomState);

            if(other.PreviousRoomState != null)
            {
                PreviousRoomState = new InRoomState(other.PreviousRoomState);
            }
        }

        /// <summary>
        /// Delegates to the copy constructor to return a new InGameState based on this one.
        /// </summary>
        /// <returns></returns>
        public InGameState Clone()
        {
            return new InGameState(this);
        }

        protected ResourceCount Resources { get; set; }

        /// <summary>
        /// Returns the maximum possible amount of the provided resource in this InGameState.
        /// </summary>
        /// <param name="resource">The resource to get the max amount of.19</param>
        /// <returns></returns>
        public int GetMaxAmount(RechargeableResourceEnum resource)
        {
            return Inventory.GetMaxAmount(resource);
        }

        /// <summary>
        /// Returns a copy of the current resource count in this in-game state.
        /// </summary>
        /// <returns></returns>
        public ResourceCount GetCurrentResources()
        {
            return Resources.Clone();
        }

        /// <summary>
        /// Returns the current amount of the provided rechargeable resource.
        /// </summary>
        /// <param name="resource">Resource to get the amount of.</param>
        /// <returns></returns>
        public int GetCurrentAmount(RechargeableResourceEnum resource)
        {
            return Resources.GetAmount(resource);
        }

        /// <summary>
        /// Returns the current amount of the provided consumable resource. This is almost the same as getting the current amount of a rechargeable resource,
        /// except both types of energy are grouped together.
        /// </summary>
        /// <param name="resource">Resource to get the amount of.</param>
        /// <returns></returns>
        public int GetCurrentAmount(ConsumableResourceEnum resource)
        {
            return resource switch
            {
                // The other resources can be fully spent, but for energy we don't want to go below 1
                ConsumableResourceEnum.ENERGY => GetCurrentAmount(RechargeableResourceEnum.RegularEnergy) + GetCurrentAmount(RechargeableResourceEnum.ReserveEnergy),
                ConsumableResourceEnum.MISSILE => GetCurrentAmount(RechargeableResourceEnum.Missile),
                ConsumableResourceEnum.SUPER => GetCurrentAmount(RechargeableResourceEnum.Super),
                ConsumableResourceEnum.POWER_BOMB => GetCurrentAmount(RechargeableResourceEnum.PowerBomb)
            };
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
        /// Adds the provided quantity of the provided consumable resource. Will not go beyond the maximum
        /// </summary>
        /// <param name="resource">The resource to increase</param>
        /// <param name="quantity">The amount to increase by</param>
        public void ApplyAddResource(RechargeableResourceEnum resource, int quantity)
        {
            int max = GetMaxAmount(resource);
            int currentAmount = Resources.GetAmount(resource);

            // We're already at max (or greater, somehow). Don't add anything
            if (currentAmount >= max)
            {
                return;
            }
            int newAmount = currentAmount + quantity;

            Resources.ApplyAmount(resource, Math.Min(max, currentAmount + quantity));
        }

        /// <summary>
        /// Consumes the provided quantity of the provided consumable resource. When consuming energy, regular energy is used up first (down to 1) 
        /// then reserves are used.
        /// </summary>
        /// <param name="resource">The resource to consume</param>
        /// <param name="quantity">The amount to consume</param>
        public void ApplyConsumeResource(ConsumableResourceEnum resource, int quantity)
        {
            Resources.ApplyAmountReduction(resource, quantity);
        }

        /// <summary>
        /// Sets current value for the provided resource to the current maximum
        /// </summary>
        /// <param name="resource">The resource to refill</param>
        public void ApplyRefillResource(RechargeableResourceEnum resource)
        {
            Resources.ApplyAmount(resource, Inventory.GetMaxAmount(resource));
        }

        protected IDictionary<string, GameFlag> ActiveGameFlags { get; set; } = new Dictionary<string, GameFlag>();

        private IReadOnlyDictionary<string, GameFlag> _activeGameFlags;
        /// <summary>
        /// Returns a read-only view of the active game flags, mapped by name.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, GameFlag> GetActiveGameFlagsDictionary()
        {
            if (_activeGameFlags == null)
            {
                _activeGameFlags = new ReadOnlyDictionary<string, GameFlag>(ActiveGameFlags);
            }
            return _activeGameFlags;
        }

        /// <summary>
        /// Creates and returns a new dictionary containing all active game flags from this in-game state
        /// that aren't active in the provided other in-game state.
        /// </summary>
        /// <param name="other">The other in-game state</param>
        /// <returns></returns>
        public IDictionary<string, GameFlag> GetActiveGameFlagsExceptWith(InGameState other)
        {
            IDictionary<string, GameFlag> returnFlags = new Dictionary<string, GameFlag>();

            // For each flag, just check for absence in other
            foreach (KeyValuePair<string, GameFlag> kvp in ActiveGameFlags)
            {
                if (!other.ActiveGameFlags.ContainsKey(kvp.Key))
                {
                    returnFlags.Add(kvp.Key, kvp.Value);
                }
            }

            return returnFlags;
        }

        /// <summary>
        /// Returns whether the provided game flag is activated in this InGameState.
        /// </summary>
        /// <param name="flag">The game flag to check</param>
        /// <returns></returns>
        public bool HasGameFlag(GameFlag flag)
        {
            return ActiveGameFlags.ContainsKey(flag.Name);
        }

        /// <summary>
        /// Returns whether the game flag with the provided name is activated in this InGameState.
        /// </summary>
        /// <param name="flagName">The game flag name to check</param>
        /// <returns></returns>
        public bool HasGameFlag(string flagName)
        {
            return ActiveGameFlags.ContainsKey(flagName);
        }

        /// <summary>
        /// Adds the provided game flag to the activated game flags in this InGameState.
        /// </summary>
        /// <param name="flag">Flag to add</param>
        public void ApplyAddGameFlag(GameFlag flag)
        {
            if(!HasGameFlag(flag))
            {
                ActiveGameFlags.Add(flag.Name, flag);
            }
        }

        protected IDictionary<string, NodeLock> OpenedLocks { get; set; } = new Dictionary<string, NodeLock>();

        private IReadOnlyDictionary<string, NodeLock> _readOnlyOpenedLocks;
        /// <summary>
        /// Returns a read-only view of the opened locks dictionary, mapped by name.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, NodeLock> GetOpenedLocksDictionary()
        {
            if (_readOnlyOpenedLocks == null)
            {
                _readOnlyOpenedLocks = new ReadOnlyDictionary<string, NodeLock>(OpenedLocks);
            }
            return _readOnlyOpenedLocks;
        }

        /// <summary>
        /// Creates and returns a new dictionary containing all OPENED NODE LOCKS from this in-game state
        /// that aren't OPENED in the provided other in-game state.
        /// </summary>
        /// <param name="other">The other in-game state</param>
        /// <returns></returns>
        public IDictionary<string, NodeLock> GetOpenedNodeLocksExceptWith(InGameState other)
        {

            IDictionary<string, NodeLock> returnLocks = new Dictionary<string, NodeLock>();

            // For each lock, just check for absence in other
            foreach (KeyValuePair<string, NodeLock> kvp in OpenedLocks)
            {
                if (!other.OpenedLocks.ContainsKey(kvp.Key))
                {
                    returnLocks.Add(kvp.Key, kvp.Value);
                }
            }

            return returnLocks;
        }

        /// <summary>
        /// Returns whether the provided node lock is open in this InGameState.
        /// </summary>
        /// <param name="nodeLock">The node lock to check</param>
        /// <returns></returns>
        public bool IsLockOpen(NodeLock nodeLock)
        {
            return OpenedLocks.ContainsKey(nodeLock.Name);
        }

        /// <summary>
        /// Returns whether the node lock with the provided name is open in this InGameState.
        /// </summary>
        /// <param name="lockName">The node lock name to check</param>
        /// <returns></returns>
        public bool IsLockOpen(string lockName)
        {
            return OpenedLocks.ContainsKey(lockName);
        }

        /// <summary>
        /// Adds the provided node lock to the opened node locks in this InGameState.
        /// </summary>
        /// <param name="nodeLock">Lock to add</param>
        public void ApplyOpenLock(NodeLock nodeLock)
        {
            if (!IsLockOpen(nodeLock))
            {
                OpenedLocks.Add(nodeLock.Name, nodeLock);
            }
        }

        protected IDictionary<string, RoomNode> TakenItemLocations { get; set; } = new Dictionary<string, RoomNode>();

        private IReadOnlyDictionary<string, RoomNode> _takenItemLocations;
        /// <summary>
        /// Returns a read-only view of the taken item locations, mapped by name.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, RoomNode> GetTakenItemLocationsDictionary()
        {
            if (_takenItemLocations == null)
            {
                _takenItemLocations = new ReadOnlyDictionary<string, RoomNode>(TakenItemLocations);
            }
            return _takenItemLocations;
        }

        /// <summary>
        /// Creates and returns a new dictionary containing all taken item locations from this in-game state
        /// that aren't taken in the provided other in-game state.
        /// </summary>
        /// <param name="other">The other in-game state</param>
        /// <returns></returns>
        public IDictionary<string, RoomNode> GetTakenItemLocationsExceptWith(InGameState other)
        {
            IDictionary<string, RoomNode> returnLocations = new Dictionary<string, RoomNode>();

            // For each location, just check for absence in other
            foreach (KeyValuePair<string, RoomNode> kvp in TakenItemLocations)
            {
                if (!other.TakenItemLocations.ContainsKey(kvp.Key))
                {
                    returnLocations.Add(kvp.Key, kvp.Value);
                }
            }

            return returnLocations;
        }

        /// <summary>
        /// Returns whether the provided item location is taken in this InGameState.
        /// </summary>
        /// <param name="roomNode">The node of the location to check</param>
        /// <returns></returns>
        public bool IsItemLocationTaken(RoomNode location)
        {
            return TakenItemLocations.ContainsKey(location.Name);
        }

        /// <summary>
        /// Returns whether the location with the provided name is taken in this InGameState.
        /// </summary>
        /// <param name="locationName">The location name to check</param>
        /// <returns></returns>
        public bool IsItemLocationTaken(string locationName)
        {
            return TakenItemLocations.ContainsKey(locationName);
        }

        /// <summary>
        /// Adds the provided location to the taken locations in this InGameState.
        /// Does not modify the inventory.
        /// </summary>
        /// <param name="location">Node of the location to add</param>
        public void ApplyTakeLocation(RoomNode location)
        {
            if (!IsItemLocationTaken(location))
            {
                TakenItemLocations.Add(location.Name, location);
            }
        }

        protected ItemInventory Inventory { get; set; }

        /// <summary>
        /// Creates and returns a new ItemInventory containing all items from this in-game state
        /// that aren't found in the provided other in-game state.
        /// </summary>
        /// <param name="other">The other in-game state</param>
        /// <returns></returns>
        public ItemInventory GetInventoryExceptWith(InGameState other)
        {
            return Inventory.ExceptWith(other.Inventory);
        }

        /// <summary>
        /// Creates and returns a ResourceCount that expresses how many resources this in-game state has,
        /// relative to the provided in-game state. Negative values mean this state has less.
        /// </summary>
        /// <param name="other">The other in-game state to compare with.</param>
        /// <returns></returns>
        public ResourceCount GetResourceVariationWith(InGameState other)
        {
            ResourceCount returnValue = new ResourceCount();
            foreach (RechargeableResourceEnum currentResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                returnValue.ApplyAmount(currentResource, GetCurrentAmount(currentResource) - other.GetCurrentAmount(currentResource));
            }

            return returnValue;
        }

        /// <summary>
        /// Returns a read-only view of the inner non-consumable items dictionary, mapped by name.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, Item> GetNonConsumableItemsDictionary()
        {
            return Inventory.GetNonConsumableItemsDictionary();
        }

        /// <summary>
        /// Returns a read-only view of the inner dictionary of expansion items (along with how many of each is present), mapped by name.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, (ExpansionItem item, int count)> GetExpansionItemsDictionary()
        {
            return Inventory.GetExpansionItemsDictionary();
        }

        /// <summary>
        /// Returns whether the player has the provided item in this InGameState.
        /// </summary>
        /// <param name="item">The item to check for</param>
        /// <returns></returns>
        public bool HasItem(Item item)
        {
            return Inventory.HasItem(item);
        }

        /// <summary>
        /// Returns whether the player has the item with the provided name in this InGameState.
        /// </summary>
        /// <param name="item">The item to check for</param>
        /// <returns></returns>
        public bool HasItem(string itemName)
        {
            return Inventory.HasItem(itemName);
        }

        /// <summary>
        /// Returns specifically whether the player has the Varia Suit in this in-game state.
        /// </summary>
        /// <returns></returns>
        public bool HasVariaSuit()
        {
            return Inventory.HasVariaSuit();
        }

        /// <summary>
        /// Returns specifically whether the player has the Gravity Suit in this in-game state.
        /// </summary>
        /// <returns></returns>
        public bool HasGravitySuit()
        {
            return Inventory.HasGravitySuit();
        }

        /// <summary>
        /// Returns specifically whether the player has the Speed Booster in this in-game state.
        /// </summary>
        /// <returns></returns>
        public bool HasSpeedBooster()
        {
            return Inventory.HasSpeedBooster();
        }

        /// <summary>
        /// Adds the provided item to the player's inventory for this InGameState.
        /// </summary>
        /// <param name="item"></param>
        public void ApplyAddItem(Item item)
        {
            Inventory.ApplyAddItem(item);
        }

        /// <summary>
        /// Returns whether the provided item is present and disabled in this InGameState.
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns></returns>
        public bool isItemDisabled(Item item)
        {
            return Inventory.IsItemDisabled(item);
        }

        /// <summary>
        /// Returns whether the item with the provided name is present and disabled in this InGameState.
        /// </summary>
        /// <param name="itemName">Name of the item to check</param>
        /// <returns></returns>
        public bool IsItemDisabled(string itemName)
        {
            return Inventory.IsItemDisabled(itemName);
        }

        /// <summary>
        /// Returns the names of items that are disabled in this InGameState.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetDisabledItemNames()
        {
            return Inventory.GetDisabledItemNames();
        }

        /// <summary>
        ///  Disables the provided non-consumable if it's in this InGameState.
        ///  Does nothing otherwise.
        /// </summary>
        /// <param name="itemName">Name of the item to disable</param>
        public void ApplyDisableItem(Item item)
        {
            Inventory.ApplyDisableItem(item);
        }

        /// <summary>
        ///  Disables the non-consumable item with the provided name if it's in this InGameState.
        ///  Does nothing otherwise.
        /// </summary>
        /// <param name="itemName">Name of the item to disable</param>
        public void ApplyDisableItem(string itemName)
        {
            Inventory.ApplyDisableItem(itemName);
        }

        /// <summary>
        ///  Re-enables the provided non-consumable if it's in this InGameState and disabled.
        ///  Does nothing otherwise.
        /// </summary>
        /// <param name="itemName">Name of the item to enable</param>
        public void ApplyEnableItem(Item item)
        {
            Inventory.ApplyEnableItem(item);
        }

        /// <summary>
        ///  Re-enables the non-consumable item with the provided name if it's in this InGameState and disabled.
        ///  Does nothing otherwise.
        /// </summary>
        /// <param name="itemName">Name of the item to enable</param>
        public void ApplyEnableItem(string itemName)
        {
            Inventory.ApplyEnableItem(itemName);
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
        /// Returns the node the player is remotely exiting the room from. This should be null in all situations except if looking at the previous room,
        /// and if the player also exited that room remotely.
        /// </summary>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer.</param>
        /// <returns></returns>
        public RoomNode GetRemoteExitNode(bool usePreviousRoom = false)
        {
            InRoomState roomState = usePreviousRoom ? PreviousRoomState : InRoomState;
            return roomState?.RemoteExitNode;
        }

        /// <summary>
        /// Returns whether the player is exiting the room by bypassing a lock on the node they are exiting by.
        /// </summary>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer.</param>
        /// <returns></returns>
        public bool BypassingExitLock(bool usePreviousRoom = false)
        {
            InRoomState roomState = usePreviousRoom ? PreviousRoomState : InRoomState;
            return roomState?.BypassedExitLock ?? false;
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
        /// <para>Positions the in-game state as it would be after entering a room via the provided node. This may place the player at a different node immediately
        /// if the node calls for it.</para>
        /// <para>This method allows for leaving "remotely": leaving through a node the player is not at, by performing an action that must be initiated where the player is.
        /// A known example is using a CanLeaveCharged that initiates at a different node than the door it's on.</para>
        /// <para>Be aware that when leaving remotely, the exact action used is only determined retroactively once in the next room, and that leaving remotely is
        /// not considered valid if it's not made use of by a strat in the next room.
        /// </para>
        /// </summary>
        /// <param name="entryNode">The node (in the next room) through which the next room will be enteted.</param>
        /// <param name="remoteExitNode">If the player is leaving remotely, this indicates the node through which the player remotely leaves.</param>
        public void ApplyEnterRoom(RoomNode entryNode, bool bypassExitLock = false, RoomNode remoteExitNode = null)
        {
            // Finalize current room state with exit state
            InRoomState.ApplyExitRoom(bypassExitLock, remoteExitNode);

            // Copy current room state and remember it as previous
            PreviousRoomState = new InRoomState(InRoomState);

            // Enter next room
            InRoomState.ApplyEnterRoom(entryNode);
        }

        /// <summary>
        /// Positions the in-game state at the provided node. This node should be inside the current room.
        /// </summary>
        /// <param name="nodeToVisit">The node to go to</param>
        /// <param name="strat">The strat through which the node is being reached. Can be null. If not null, only makes sense if 
        /// it's on a link that connects previous node to new node.</param>
        public void ApplyVisitNode(RoomNode nodeToVisit, Strat strat)
        {
            InRoomState.ApplyVisitNode(nodeToVisit, strat);
        }

        /// <summary>
        /// Identifies and returns a LinkTo that allows navigation from the current node to the provided node.
        /// </summary>
        /// <param name="targetNodeId">The node to which the LinkTo should lead</param>
        /// <returns>The identified LinkTo, or null if a single LinkTo couldn't be found</returns>
        public LinkTo GetCurrentLinkTo(int targetNodeId)
        {
            Link linkFromCurrent = GetCurrentRoom().Links
                .Where(link => link.FromNodeId == GetCurrentNode().Id)
                .SingleOrDefault();
            // If we don't find exactly one link from current node, can't do anything
            if (linkFromCurrent == null)
            {
                return null;
            }
            else
            {
                return linkFromCurrent.To
                    .Where(to => to.TargetNodeId == targetNodeId)
                    .SingleOrDefault();
            }
        }

        /// <summary>
        /// Updates the in-room state to contain a mention of the destruction of the provided obstacle.
        /// This obstacle should be in the current room.
        /// </summary>
        /// <param name="obstacle">The obstacle to destroy.</param>
        public void ApplyDestroyedObstacle(RoomObstacle obstacle)
        {
            InRoomState.ApplyDestroyedObstacle(obstacle);
        }

        /// <summary>
        /// Removes all in-room data from this InGameState. Useful if this has been initialized at a starting node but in-room state is not going to be maintained.
        /// </summary>
        public void ApplyClearRoomState()
        {
            InRoomState.ClearRoomState();
            PreviousRoomState?.ClearRoomState();
        }

        /// <summary>
        /// <para>Returns all runways that the player could possibly be able to retroactively use, according to the pathing in this in-game state.
        /// Does not check whether the player is also able to use any strats on those runways.</para>
        /// <para>"Retroactive use" is meant to be done right after entering a room, and aims to retroactively decide how the last room was exited.</para>
        /// <para>A runway would typically be used retroactively to satisfy an adjacentRunway or canComeInCharged
        /// that is being executed soon after entry of the new room.</para>
        /// </summary>
        /// <param name="requiredInRoomPath">The path that must have been followed in the current room (as successive node IDs) in order to be able 
        /// to use retroactive runways in the current context. The first node in this path also dictates the node to which the retroactive runways must lead.</param>
        /// <param name="usePreviousRoom">If true, indicates that the "new" room is already the previous room in this InGameState.</param>
        /// <returns></returns>
        public IEnumerable<Runway> GetRetroactiveRunways(IEnumerable<int> requiredInRoomPath, bool usePreviousRoom = false)
        {
            // Since this is a retroactive check, we already have to look at the room prior to the one we're asked to evaluate
            // If we were already evaluating the previous room, we have no way to obtain the state of the room before that so just return
            if (usePreviousRoom)
            {
                return Enumerable.Empty<Runway>();
            }

            // We will need to know what nodes were visited in the current room. If this info is missing, we can't do anything retroactively.
            IEnumerable<int> visitedNodeIds = GetVisitedNodeIds(usePreviousRoom);
            // If we don't know at what node we entered, we can't identify any usable runways
            if (!visitedNodeIds.Any())
            {
                return Enumerable.Empty<Runway>();
            }

            // If the current in-room state doesn't respect the in-room path necessary to use retroactive runways, we can't use any
            if(!visitedNodeIds.SequenceEqual(requiredInRoomPath))
            {
                return Enumerable.Empty<Runway>();
            }

            // At this point we know our behavior in the current room respects the provided requirements for retroactively using a runway.
            // We must now figure out if the previous room also qualifies.

            // Figure out through what node we left the previous room...
            RoomNode previousRoomExitNode = GetCurrentNode(usePreviousRoom: true);

            // If we can't figure out how we left previous room, we can't return any runways
            if (previousRoomExitNode == null)
            {
                return Enumerable.Empty<Runway>();
            }

            // If the last room was exited by bypassing a lock, runways can't be used
            if (BypassingExitLock(usePreviousRoom: true))
            {
                return Enumerable.Empty<Runway>();
            }

            // If we didn't leave the previous room via a node that led to the node by which we entered current room, no runways are usable
            RoomNode entryNode = GetCurrentRoom(usePreviousRoom).Nodes[visitedNodeIds.First()];
            if (previousRoomExitNode.OutNode != entryNode)
            {
                return Enumerable.Empty<Runway>();
            }

            // We've confirmed we can use retroactive runways. Return all runways of the previous room's exit node
            return previousRoomExitNode.Runways;
        }

        /// <summary>
        /// <para>Returns all canLeaveChargeds that the player could possibly be able to retroactively use, according to the pathing in this in-game state.
        /// Does not check whether the player is able to use any strats on those canLeaveChargeds, or whether charging or sparking is currently doable.
        /// To check all of that, see <see cref="CanLeaveCharged.IsUsable(SuperMetroidModel, InGameState, bool)"/>.</para>
        /// <para>"Retroactive use" is meant to be done right after entering a room, and aims to retroactively decide how the last room was exited.</para>
        /// <para>A canLeaveCharged would typically be used retroactively to satisfy a canComeInCharged
        /// that is being executed soon after entry of the new room.</para>
        /// </summary>
        /// <param name="requiredInRoomPath">The path that must have been followed in the current room (as successive node IDs) in order to be able to use 
        /// retroactive canLeavechargeds in the current context. The first node in this path also dictates the node to which 
        /// the retroactive charged exit must lead.</param>
        /// <param name="usePreviousRoom">If true, indicates that the "new" room is already the previous room in this InGameState.</param>
        /// <returns></returns>
        public IEnumerable<CanLeaveCharged> GetRetroactiveCanLeaveChargeds(IEnumerable<int> requiredInRoomPath, bool usePreviousRoom = false)
        {
            // Since this is a retroactive check, we already have to look at the room prior to the one we're asked to evaluate
            // If we were already evaluating the previous room, we have no way to obtain the state of the room before that so just return
            if (usePreviousRoom)
            {
                return Enumerable.Empty<CanLeaveCharged>();
            }

            // We will need to know what nodes were visited in the current room. If this info is missing, we can't do anything retroactively.
            IEnumerable<int> visitedNodeIds = GetVisitedNodeIds(usePreviousRoom);
            // If we don't know at what node we entered, we can't identify any usable runways
            if (!visitedNodeIds.Any())
            {
                return Enumerable.Empty<CanLeaveCharged>();
            }

            // If the current in-room state doesn't respect the in-room path necessary to use retroactive runways, we can't use any
            if (!visitedNodeIds.SequenceEqual(requiredInRoomPath))
            {
                return Enumerable.Empty<CanLeaveCharged>();
            }

            RoomNode entryNode = GetCurrentRoom(usePreviousRoom).Nodes[visitedNodeIds.First()];

            // At this point we know our behavior in the current room respects the provided requirements for retroactively using a CanLeaveCharged.

            // Figure out through what node we left the previous room...
            RoomNode previousRoomNode = GetCurrentNode(usePreviousRoom: true);
            RoomNode previousRoomRemoteExitNode = GetRemoteExitNode(usePreviousRoom: true);
            bool remoteExit = previousRoomRemoteExitNode != null;
            RoomNode previousRoomExitNode = remoteExit?  previousRoomRemoteExitNode : previousRoomNode;

            // If we can't figure out how we left previous room, we can't return any canLeaveChargeds
            if (previousRoomExitNode == null)
            {
                return Enumerable.Empty<CanLeaveCharged>();
            }

            // If the last room was exited by bypassing a lock, canLeaveChargeds can't be used
            if (BypassingExitLock(usePreviousRoom: true))
            {
                return Enumerable.Empty<CanLeaveCharged>();
            }

            // If we didn't leave the previous room via a node that led to the node by which we entered current room, no canLeavechargeds are usable
            if (previousRoomExitNode.OutNode != entryNode)
            {
                return Enumerable.Empty<CanLeaveCharged>();
            }

            // If last room was exited remotely, return all CanLeaveChargeds of that remote exited node which are initiated at the last node
            if (remoteExit)
            {
                return previousRoomExitNode.CanLeaveCharged.Where(clc => clc.InitiateAtNode == previousRoomNode)
                    // If a remote CanLeaveCharged requires opening the door first, make sure the door's node was visited prior to remotely exiting
                    .Where(clc => !clc.MustOpenDoorFirst || GetVisitedNodeIds(true).Contains(previousRoomExitNode.Id));
            }
            // If last room was not exited remotely, return all CanLeaveChargeds that are performed directly at exit node
            else
            {
                return previousRoomExitNode.CanLeaveCharged.Where(clc => clc.InitiateAtNode == null);
            }
        }
    }
    // End of InGameState class

    /// <summary>
    /// A Comparer that can compare two in-game states by their consumable resource count, based on an internal dictionary of relative resource values.
    /// The "greater" in-game state is the one whose resource total is deemed more valuable according to these values.
    /// </summary>
    public class InGameStateComparer : IComparer<InGameState>
    {
        private IDictionary<ConsumableResourceEnum, int> RelativeResourceValues {get; set;}

        public InGameStateComparer(IDictionary<ConsumableResourceEnum, int> relativeResourceValues)
        {
            RelativeResourceValues = relativeResourceValues;
        }

        public int Compare(InGameState x, InGameState y)
        {
            return CalculateValue(x).CompareTo(CalculateValue(y));
        }

        /// <summary>
        /// Calculates a value to attribute to the provided InGameState when using this Comparer to compare InGameStates.
        /// </summary>
        /// <param name="inGameState">The InGameState to assign a value to</param>
        /// <returns></returns>
        private int CalculateValue(InGameState inGameState)
        {
            // Give a negative value to null. It's decidedly less valuable than any existing state.
            if (inGameState == null)
            {
                return -1;
            }

            // We are assuming that dead states (0 energy) won't show up. If we wanted to support that, we'd have to check specifically for it and give it a negative value too.
            // (but greater than the value for null)
            int value = 0;
            foreach (ConsumableResourceEnum currentResource in Enum.GetValues(typeof(ConsumableResourceEnum)))
            {
                value += inGameState.GetCurrentAmount(currentResource) * RelativeResourceValues[currentResource];
            }

            return value;
        }
    }
}
