using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options.ResourceValues;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
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
        /// <summary>
        /// Indicates how many previous rooms to keep.
        /// </summary>
        private const int PreviousRooms = 2;

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

            OpenedLocks = new Dictionary<string, NodeLock>(other.OpenedLocks);

            Inventory = other.Inventory.Clone();

            Resources = other.Resources.Clone();

            InRoomState = new InRoomState(other.InRoomState);

            foreach(InRoomState previousRoomState in other.PreviousRoomStates)
            {
                PreviousRoomStates.Add(new InRoomState(previousRoomState));
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
        /// Uses the provided resource evaluator to evaluate the current in-game resources of this state.
        /// </summary>
        /// <param name="evaluator"></param>
        /// <returns></returns>
        public int EvaluateCurrentResources(IInGameResourceEvaluator evaluator)
        {
            return evaluator.CalculateValue(Resources);
        }

        /// <summary>
        /// Returns the maximum possible amount of the provided resource in this InGameState.
        /// </summary>
        /// <param name="resource">The resource to get the max amount of.</param>
        /// <returns></returns>
        public int GetMaxAmount(RechargeableResourceEnum resource)
        {
            return Inventory.GetMaxAmount(resource);
        }

        /// <summary>
        /// Returns the maximum possible amount of the provided consumable resource.
        /// This is almost the same as getting the max amount of a rechargeable resource,
        /// except both types of energy are grouped together.
        /// </summary>
        /// <param name="resource">The resource to get the max amount of.</param>
        /// <returns></returns>
        public int GetMaxAmount(ConsumableResourceEnum resource)
        {
            return resource.ToRechargeableResources().Select(resource => GetMaxAmount(resource)).Sum();
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
            return resource.ToRechargeableResources().Select(resource => GetCurrentAmount(resource)).Sum();
        }

        public bool IsResourceAvailable(SuperMetroidModel model, ConsumableResourceEnum resource, int quantity)
        {
            if(quantity == 0)
            {
                return true;
            }

            // If resource tracking is enabled, look at current resource amounts
            if (model.LogicalOptions.ResourceTrackingEnabled)
            {
                // The other resources can be fully spent, but for energy we don't want to go below 1
                if (resource == ConsumableResourceEnum.ENERGY)
                {
                    return GetCurrentAmount(resource) > quantity;
                }
                else
                {
                    return GetCurrentAmount(resource) >= quantity;
                }
            }
            // If resource tracking is not enabled, use max resource amounts instead of current amounts
            else
            {
                // The other resources can be fully spent, but for energy we don't want to go below 1
                if (resource == ConsumableResourceEnum.ENERGY)
                {
                    return GetMaxAmount(resource) > quantity;
                }
                else
                {
                    return GetMaxAmount(resource) >= quantity;
                }
            }
        }

        /// <summary>
        /// Adds the provided quantity of the provided consumable resource. Will not go beyond the maximum
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="resource">The resource to increase</param>
        /// <param name="quantity">The amount to increase by</param>
        public void ApplyAddResource(SuperMetroidModel model, RechargeableResourceEnum resource, int quantity)
        {
            // Don't bother with current resource count if resource tracking is disabled
            if (model.LogicalOptions.ResourceTrackingEnabled)
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
        }

        /// <summary>
        /// Consumes the provided quantity of the provided consumable resource. When consuming energy, regular energy is used up first (down to 1) 
        /// then reserves are used.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="resource">The resource to consume</param>
        /// <param name="quantity">The amount to consume</param>
        public void ApplyConsumeResource(SuperMetroidModel model, ConsumableResourceEnum resource, int quantity)
        {
            // Don't bother with current resource count if resource tracking is disabled
            if(model.LogicalOptions.ResourceTrackingEnabled)
            {
                Resources.ApplyAmountReduction(resource, quantity);
            }
        }

        /// <summary>
        /// Sets current value for the provided resource to the current maximum
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="resource">The resource to refill</param>
        public void ApplyRefillResource(SuperMetroidModel model, RechargeableResourceEnum resource)
        {
            // Don't bother with current resource count if resource tracking is disabled
            if (model.LogicalOptions.ResourceTrackingEnabled)
            {
                Resources.ApplyAmount(resource, Inventory.GetMaxAmount(resource));
            }
        }

        /// <summary>
        /// Sets current value for the provided consumable resource to the current maximum.
        /// This is almost the same as refilling a rechargeable resource,
        /// except both types of energy are grouped together.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="resource">The resource to refill</param>
        public void ApplyRefillResource(SuperMetroidModel model, ConsumableResourceEnum resource)
        {
            // Don't bother with current resource count if resource tracking is disabled
            if (model.LogicalOptions.ResourceTrackingEnabled)
            {
                foreach (RechargeableResourceEnum rechargeableResource in resource.ToRechargeableResources())
                {
                    ApplyRefillResource(model, rechargeableResource);
                }
            }
        }

        /// <summary>
        /// Creates and returns a ResourceCount that expresses how many rechargeable resources this in-game state has,
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
        /// Returns the enumeration of rechargeable resources that are currently full.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RechargeableResourceEnum> GetFullRechargeableResources()
        {
            return Enum.GetValues(typeof(RechargeableResourceEnum))
                .Cast<RechargeableResourceEnum>()
                .Where(resource => Resources.GetAmount(resource) >= GetMaxAmount(resource));
        }

        /// <summary>
        /// Returns the enumeration of consumable resources that are currently full.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ConsumableResourceEnum> GetFullConsumableResources()
        {
            return Enum.GetValues(typeof(ConsumableResourceEnum))
                .Cast<ConsumableResourceEnum>()
                .Where(resource => Resources.GetAmount(resource) >= GetMaxAmount(resource));
        }

        /// <summary>
        /// Returns the enumeration of enemy drops that aren't needed by this in-game state because the associated resources are full.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <returns></returns>
        public IEnumerable<EnemyDropEnum> GetUnneededDrops(SuperMetroidModel model)
        {
            return model.Rules.GetUnneededDrops(GetFullRechargeableResources());
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
        /// Applies the opening of the provided lock in this InGameState. Expects that samus is at the node that has that lock.
        /// </summary>
        /// <param name="nodeLock">Lock to open</param>
        public void ApplyOpenLock(NodeLock nodeLock)
        {
            if (!IsLockOpen(nodeLock))
            {
                OpenedLocks.Add(nodeLock.Name, nodeLock);
                InRoomState.ApplyOpenLock(nodeLock);
            }
        }

        /// <summary>
        /// Applies the bypassing of the provided lock in this InGameState. Expects that samus is at the node that has that lock.
        /// </summary>
        /// <param name="nodeLock">Lock to bypass</param>
        public void ApplyBypassLock(NodeLock nodeLock) {
            if (!IsLockOpen(nodeLock))
            {
                InRoomState.ApplyBypassLock(nodeLock);
            }
        }

        /// <summary>
        /// Returns the locks bypassed by Samus at the current node.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (using the last known state in the resulting room if so).
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public IEnumerable<NodeLock> GetBypassedLocks(int previousRoomCount = 0)
        {
            return GetInRoomState(previousRoomCount).GetBypassedLocks();
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
        /// In-room state of the last few rooms when they were left. This list remembers no more room states than the PreviousRooms constant.
        /// The closer to the start of the list a state is, the more recently Samus was in it.
        /// </summary>
        protected List<InRoomState> PreviousRoomStates { get; } = new List<InRoomState>();

        /// <summary>
        /// Returns the in-room state that corresponds to the provided previousRoomCount, for this in-game state.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (using the last known state in the resulting room if so).
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        protected InRoomState GetInRoomState(int previousRoomCount)
        {
            if (previousRoomCount == 0)
            {
                return InRoomState;
            } else if (previousRoomCount > 0)
            {
                return PreviousRoomStates[previousRoomCount - 1];
            }
            else
            {
                throw new ArgumentException("previousRoomCount must not be negative");
            }
        }

        /// <summary>
        /// <para>
        /// Registers the provided previousState as the most recent previously-visited room state.
        /// If this would cause the number of previous rooms to be over the maximum amount, also forgets the oldest remembered previous state.
        /// </para>
        /// <para>
        /// Note: If the provided previous state is not a playable room, it will not be registered and this method will do nothing
        /// </para>
        /// </summary>
        /// <param name="previousState"></param>
        protected void RegisterPreviousRoom(InRoomState previousState)
        {
            // Ignore non-playable rooms in our previous states.
            if (!previousState.CurrentRoom.Playable)
            {
                return;
            }

            if(PreviousRoomStates.Count >= PreviousRooms)
            {
                PreviousRoomStates.RemoveAt(PreviousRoomStates.Count - 1);
                PreviousRoomStates.Insert(0, previousState);
            }
        }

        /// <summary>
        /// Returns the node the player is currently at. This can be null if in-room state isn't being tracked.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (using the last known state in the resulting room if so).
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public RoomNode GetCurrentNode(int previousRoomCount = 0)
        {
            InRoomState roomState = GetInRoomState(previousRoomCount);
            return roomState?.CurrentNode;
        }

        /// <summary>
        /// Returns whether the player is exiting the room by bypassing a lock on the node they are exiting by.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by.
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public bool BypassingExitLock(int previousRoomCount = 0)
        {
            InRoomState roomState = GetInRoomState(previousRoomCount);
            return roomState?.BypassedExitLock ?? false;
        }

        /// <summary>
        /// Returns whether the player is exiting the room by opening a lock on the node they are exiting by.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by.
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public bool OpeningExitLock(int previousRoomCount = 0)
        {
            InRoomState roomState = GetInRoomState(previousRoomCount);
            return roomState?.OpenedExitLock ?? false;
        }

        /// <summary>
        /// Returns the room the player is currently in. This can be null if in-room state isn't being tracked.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by. 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public Room GetCurrentRoom(int previousRoomCount = 0)
        {
            InRoomState roomState = GetInRoomState(previousRoomCount);
            return roomState?.CurrentRoom;
        }

        /// <summary>
        /// Returns the RoomEnvironment applicable to the room the player is currently in. This can be null if in-room state isn't being tracked.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by.
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public RoomEnvironment GetCurrentRoomEnvironment(int previousRoomCount = 0)
        {
            Room currentRoom = GetCurrentRoom(previousRoomCount);
            if(currentRoom == null)
            {
                return null;
            }

            RoomNode entranceNode = GetVisitedPath(previousRoomCount).First().nodeState.Node;
            return currentRoom.RoomEnvironments
                .Where(environment => environment.EntranceNodes == null || environment.EntranceNodes.Contains(entranceNode, ObjectReferenceEqualityComparer<RoomNode>.Default)).FirstOrDefault();
        }

        /// <summary>
        /// Returns whether the room the player currently in is heated. Defaults to false if in-room state isn't being tracked.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by.
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public bool IsHeatedRoom(int previousRoomCount = 0)
        {
            RoomEnvironment environment = GetCurrentRoomEnvironment(previousRoomCount);
            return environment != null && environment.Heated;
        }

        /// <summary>
        /// Returns the DoorEnvironment applicable to the node the player is currently in (if it has one). This can also be null if in-room state isn't being tracked.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (using the last known state in the resulting room if so).
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public DoorEnvironment GetCurrentDoorEnvironment(int previousRoomCount = 0)
        {
            RoomNode currentNode = GetCurrentNode(previousRoomCount);
            if (currentNode == null || !currentNode.DoorEnvironments.Any())
            {
                return null;
            }

            RoomNode entranceNode = GetVisitedPath(previousRoomCount).First().nodeState.Node;
            return currentNode.DoorEnvironments
                .Where(environment => environment.EntranceNodes == null || environment.EntranceNodes.Contains(entranceNode, ObjectReferenceEqualityComparer<RoomNode>.Default)).First();
        }

        /// <summary>
        /// Returns the door physics (if any) at the current node.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (using the last known state in the resulting room if so).
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public PhysicsEnum? GetCurrentDoorPhysics(int previousRoomCount = 0) {
            DoorEnvironment environment = GetCurrentDoorEnvironment(previousRoomCount);
            return environment?.Physics;
        }

        /// <summary>
        /// Returns the strat that was used to reach the current node, if any. Otherwise, returns null.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (using the last known state in the resulting room if so).
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public Strat GetLastStrat(int previousRoomCount = 0)
        {
            InRoomState roomState = GetInRoomState(previousRoomCount);
            return roomState?.LastStrat;
        }

        /// <summary>
        /// Returns a sequence of IDs of nodes that have been visited in the current room since entering, in order, 
        /// starting with the node through which the room was entered. May be empty if the in-room state is not being tracked.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by.
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public IEnumerable<int> GetVisitedNodeIds(int previousRoomCount = 0)
        {
            InRoomState roomState = GetInRoomState(previousRoomCount);
            IEnumerable<int> returnValue = roomState?.VisitedRoomPath?.Select(pathNodeState => pathNodeState.nodeState.Node.Id);
            return returnValue == null? Enumerable.Empty<int>() : returnValue;
        }

        /// <summary>
        /// Returns a sequence of nodes (represented as an InNodeState) that have been visited in this room since entering, in order,
        /// starting with the node through which the room was entered. May be empty if the in-room state is not being tracked.
        /// Each node state is accompanied by the strat that was used to reach the node, when applicable.
        /// This strat can be null since nodes are reached without using a strat when entering.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by. 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public IEnumerable<(InNodeState nodeState, Strat strat)> GetVisitedPath(int previousRoomCount = 0)
        {
            InRoomState roomState = GetInRoomState(previousRoomCount);
            var returnValue = roomState?.VisitedRoomPath;
            return returnValue == null ? Enumerable.Empty<(InNodeState, Strat)>() : returnValue;
        }

        /// <summary>
        /// Returns a sequence of IDs of obstacles that have been destroyed in the current room since entering.
        /// May be empty if the in-room state is not being tracked.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by. 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public IEnumerable<string> GetDestroyedObstacleIds(int previousRoomCount = 0)
        {
            InRoomState roomState = GetInRoomState(previousRoomCount);
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
        /// <param name="entryNode">The node (in the next room) through which the next room will be entered.</param>
        public void ApplyEnterRoom(RoomNode entryNode)
        {
            // Copy current room state and remember it as previous
            RegisterPreviousRoom(new InRoomState(InRoomState));

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
            PreviousRoomStates.ForEach(state => InRoomState.ClearRoomState());
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
        /// <param name="previousRoomCount">The number of playable rooms to go back by, *before* looking for retroactive runways in the room before that. 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public IEnumerable<Runway> GetRetroactiveRunways(IEnumerable<int> requiredInRoomPath, int previousRoomCount = 0)
        {
            // Since this is a retroactive check, we already have to look at the room prior to the "current" room for this check
            // If that "current" room is the last remembered one, we have no way to obtain the state of the room before that so just return
            if (previousRoomCount >= PreviousRoomStates.Count)
            {
                return Enumerable.Empty<Runway>();
            }

            // We will need to know what nodes were visited in the current room. If this info is missing, we can't do anything retroactively.
            IEnumerable<int> visitedNodeIds = GetVisitedNodeIds(previousRoomCount);
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
            RoomNode previousRoomExitNode = GetCurrentNode(previousRoomCount + 1);

            // If we can't figure out how we left previous room, we can't return any runways
            if (previousRoomExitNode == null)
            {
                return Enumerable.Empty<Runway>();
            }

            // If the last room was exited by bypassing a lock, runways can't be used
            if (BypassingExitLock(previousRoomCount + 1))
            {
                return Enumerable.Empty<Runway>();
            }

            // If we didn't leave the previous room via a node that led to the node by which we entered current room, no runways are usable
            RoomNode entryNode = GetCurrentRoom(previousRoomCount).Nodes[visitedNodeIds.First()];
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
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="requiredInRoomPath">The path that must have been followed in the current room (as successive node IDs) in order to be able to use 
        /// retroactive canLeavechargeds in the current context. The first node in this path also dictates the node to which 
        /// the retroactive charged exit must lead.</param>
        /// <param name="previousRoomCount">The number of playable rooms to go back by, *before* looking for retroactive canLeaveChargeds in the room before that.
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public IEnumerable<CanLeaveCharged> GetRetroactiveCanLeaveChargeds(SuperMetroidModel model, IEnumerable<int> requiredInRoomPath, int previousRoomCount = 0)
        {
            // Since this is a retroactive check, we already have to look at the room prior to the "current" room for this check
            // If that "current" room is the last remembered one, we have no way to obtain the state of the room before that so just return
            if (previousRoomCount >= PreviousRoomStates.Count)
            {
                return Enumerable.Empty<CanLeaveCharged>();
            }

            // We will need to know what nodes were visited in the current room. If this info is missing, we can't do anything retroactively.
            IEnumerable<int> visitedNodeIds = GetVisitedNodeIds(previousRoomCount);
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

            RoomNode entryNode = GetCurrentRoom(previousRoomCount).Nodes[visitedNodeIds.First()];

            // At this point we know our behavior in the current room respects the provided requirements for retroactively using a CanLeaveCharged.

            // Figure out through what node we left the previous room...
            RoomNode previousRoomExitNode = GetCurrentNode(previousRoomCount + 1);

            // If we can't figure out how we left previous room, we can't return any canLeaveChargeds
            if (previousRoomExitNode == null)
            {
                return Enumerable.Empty<CanLeaveCharged>();
            }

            // If the last room was exited by bypassing a lock, canLeaveChargeds can't be used
            if (BypassingExitLock(previousRoomCount + 1))
            {
                return Enumerable.Empty<CanLeaveCharged>();
            }

            // If we didn't leave the previous room via a node that led to the node by which we entered current room, no canLeavechargeds are usable
            if (previousRoomExitNode.OutNode != entryNode)
            {
                return Enumerable.Empty<CanLeaveCharged>();
            }

            // Return all CanLeaveCharged that are valid to retroactively execute
            return previousRoomExitNode.CanLeaveCharged.Where(clc => {
                // If the CanLeaveCharged is initiated remotely, we must take a closer look at what happened in that room
                if(clc.IsInitiatedRemotely)
                {
                    // This CanLeaveCharged is initiated at a specific node,
                    // and is executed by following a specific path through the room, with a subset of allowed strats.
                    // We will check whether last room's exit is compatible with this CanLeaveCharged,
                    // to see if it can possibly be executed retroactively.

                    var lastRoomPath = GetVisitedPath(previousRoomCount + 1);
                    // If we haven't visited as many nodes as the prescribed path to door (+ 1 more node to enter the room),
                    // we know for sure our exit isn't compatible with this CanLeaveCharged. Reject it.
                    if (lastRoomPath.Count() < clc.InitiateRemotely.PathToDoor.Count() + 1)
                    {
                        return false;
                    }

                    // Check the last n visited nodes to make sure they correspond to the prescribed path.
                    // But before this, check the node we were at immediately before those, to make sure we're starting at the right place.
                    IEnumerable<(InNodeState nodeState, Strat strat)> lastRoomFinalPath
                        = lastRoomPath.TakeLast(clc.InitiateRemotely.PathToDoor.Count() + 1);
                    if (lastRoomFinalPath.First().nodeState.Node != clc.InitiateRemotely.InitiateAtNode)
                    {
                        return false;
                    }

                    if (!clc.InitiateRemotely.PathToDoor.Zip
                        (lastRoomFinalPath.Skip(1), (expectedPath, actualPath) => {
                            // These two path nodes match up if they go to the same room node, and if the actual path uses one of the valid strats
                            return expectedPath.link.TargetNode == actualPath.nodeState.Node
                                && expectedPath.strats.Contains(actualPath.strat, ObjectReferenceEqualityComparer<Strat>.Default);
                        })
                        // If any node in the actual path does not respect the expected path, 
                        // then last room's exit is incompatible with this CanLeaveCharged
                        .All(valid => valid))
                    {
                        return false;
                    }

                    // If there is a requirement that the door be opened first, 
                    // there are two additional requirements to execute the CanLeaveCharged retroactively.
                    if(clc.InitiateRemotely.MustOpenDoorFirst)
                    {
                        // The exit node must not have any active locks when the CanLeaveCharged execution begins.
                        // This means we can't have opened the lock on the way out
                        if(OpeningExitLock(previousRoomCount + 1))
                        {
                            return false;
                        }

                        // The exit node must have been visited before the CanLeaveCharge execution would begin.
                        // The node where execution begins is hence the last one where we could have opened the door.
                        if (!lastRoomPath.SkipLast(clc.InitiateRemotely.PathToDoor.Count()).Where(pathNode => pathNode.nodeState.Node == previousRoomExitNode).Any())
                        {
                            return false;
                        }
                    }
                    // We found no reason to invalidate the retroactive execution. This is valid to retroactively use.
                    return true;
                }
                // If the CanLeaveCharged is initiated at the exit node, we have no more conditions to check. This is valid to retroactively use.
                else
                {
                    return true;
                }
            });
            
        }
    }
    // End of InGameState class

    /// <summary>
    /// A Comparer that can compare two in-game states by their consumable resource count, based on an internal in-game resource evaluator.
    /// The "greater" in-game state is the one whose resource total is deemed more valuable according to that evaluator.
    /// </summary>
    public class InGameStateComparer : IComparer<InGameState>
    {
        private IInGameResourceEvaluator ResourceEvaluator { get; set; }

        public InGameStateComparer(IInGameResourceEvaluator resourceEvaluator)
        {
            ResourceEvaluator = resourceEvaluator;
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

            return inGameState.EvaluateCurrentResources(ResourceEvaluator);
        }
    }
}
