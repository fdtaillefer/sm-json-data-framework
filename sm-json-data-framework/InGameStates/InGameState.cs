﻿using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options.ResourceValues;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.InGameStates
{
    /// <summary>
    /// Contains the logically-relevant attributes of a given in-game state.
    /// </summary>
    public class InGameState : ReadOnlyInGameState
    {
        /// <summary>
        /// Indicates how many previous rooms to keep.
        /// </summary>
        public const int MaxPreviousRooms = 2;

        private SuperMetroidModel Model { get; }

        // STITCHME It might be valuable to eventually have InGameState be able to say which nodes are reachable?

        // STITCHME It could be nice to keep track of all canResets in the room and evaluate them as you move around?
        // Another option would be to have something in an initialization phase that converts canResets into just names,
        // and adds information on nodes and strats that they invalidate the canReset.
        // We'll see when we get to the step of reducing logical elements *shrug*

        /// <summary>
        /// Creates a new InGameState based on a copy of the provided StartConditions. 
        /// Note that an InGameState created in this way should only be used with the <see cref="SuperMetroidModel"/> that contains
        /// the node instance found in the StartConditions.
        /// </summary>
        /// <param name="startConditions">StartConditions on which to base the new InGame State</param>
        public InGameState(StartConditions startConditions)
        {
            Model = startConditions.Model;

            // Initialize starting inventory
            InternalInventory = startConditions.StartingInventory.Clone();

            // Start the player's resources at the specified values
            InternalResources = startConditions.StartingResources.Clone();

            // Initialize starting game flags
            foreach (GameFlag gameFlag in startConditions.StartingGameFlags.Values)
            {
                ApplyAddGameFlag(gameFlag);
            }

            // Initialize starting opened locks
            foreach (NodeLock openLock in startConditions.StartingOpenLocks.Values)
            {
                ApplyOpenLock(openLock, applyToRoomState: false);
            }

            // Initialize starting taken item locations
            foreach (RoomNode itemNode in startConditions.StartingTakenItemLocations.Values)
            {
                ApplyTakeLocation(itemNode);
            }

            InternalInRoomState = new InRoomState(startConditions.StartingNode);
        }

        /// <summary>
        /// A copy constructor that creates a new InGameState based on the provided one.
        /// This is a somewhat shallow copy; referenced objects whose inner state does not change with a game state (such as Room, GameFlag, etc.) will not be copied.
        /// The inner InRoomState and anything else that fully belongs to the InGameState does get copied.
        /// </summary>
        /// <param name="other">The InGameState to copy</param>
        public InGameState(InGameState other)
        {
            Model = other.Model;

            InternalActiveGameFlags = new Dictionary<string, GameFlag>(other.InternalActiveGameFlags);

            InternalTakenItemLocations = new Dictionary<string, RoomNode>(other.InternalTakenItemLocations);

            InternalOpenedLocks = new Dictionary<string, NodeLock>(other.InternalOpenedLocks);

            InternalInventory = other.InternalInventory.Clone();

            InternalResources = other.InternalResources.Clone();

            InternalInRoomState = new InRoomState(other.InternalInRoomState);

            foreach (InRoomState previousRoomState in other.InternalPreviousRoomStates)
            {
                InternalPreviousRoomStates.Add(new InRoomState(previousRoomState));
            }
        }

        public InGameState Clone()
        {
            return new InGameState(this);
        }

        public ReadOnlyInGameState AsReadOnly()
        {
            return this;
        }

        protected ResourceCount InternalResources { get; set; }

        public ReadOnlyResourceCount Resources => InternalResources.AsReadOnly();

        public bool IsResourceAvailable(ConsumableResourceEnum resource, int quantity)
        {
            return InternalResources.IsResourceAvailable(resource, quantity);
        }

        /// <summary>
        /// Adds the provided quantity of the provided consumable resource. Will not go beyond the maximum
        /// </summary>
        /// <param name="resource">The resource to increase</param>
        /// <param name="quantity">The amount to increase by</param>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyAddResource(RechargeableResourceEnum resource, int quantity)
        {
            int max = ResourceMaximums.GetAmount(resource);
            int currentAmount = InternalResources.GetAmount(resource);

            // We're already at max (or greater, somehow). Don't add anything
            if (currentAmount >= max)
            {
                return this;
            }
            int newAmount = currentAmount + quantity;

            InternalResources.ApplyAmount(resource, Math.Min(max, currentAmount + quantity));

            return this;
        }

        /// <summary>
        /// Consumes the provided quantity of the provided consumable resource. When consuming energy, regular energy is used up first (down to 1) 
        /// then reserves are used (down to 0), then regular energy is consumed into negatives if necessary.
        /// </summary>
        /// <param name="resource">The resource to consume</param>
        /// <param name="quantity">The amount to consume</param>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyConsumeResource(ConsumableResourceEnum resource, int quantity)
        {
            InternalResources.ApplyAmountReduction(resource, quantity);
            return this;
        }

        /// <summary>
        /// Sets current value for the provided resource to the current maximum
        /// </summary>
        /// <param name="resource">The resource to refill</param>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyRefillResource(RechargeableResourceEnum resource)
        {
            InternalResources.ApplyAmount(resource, ResourceMaximums.GetAmount(resource));
            return this;
        }

        /// <summary>
        /// Sets current value for the provided consumable resource to the current maximum.
        /// This is almost the same as refilling a rechargeable resource,
        /// except both types of energy are grouped together.
        /// </summary>
        /// <param name="resource">The resource to refill</param>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyRefillResource(ConsumableResourceEnum resource)
        {
            foreach (RechargeableResourceEnum rechargeableResource in resource.ToRechargeableResources())
            {
                ApplyRefillResource(rechargeableResource);
            }
            return this;
        }

        /// <summary>
        /// Sets current value for the all resources to their current maximum.
        /// </summary>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyRefillResources()
        {
            foreach (RechargeableResourceEnum rechargeableResource in Enum.GetValues<RechargeableResourceEnum>())
            {
                ApplyRefillResource(rechargeableResource);
            }
            return this;
        }

        public ResourceCount GetResourceVariationWith(ReadOnlyInGameState other)
        {
            return InternalResources.GetVariationWith(other.Resources);
        }

        public IEnumerable<RechargeableResourceEnum> GetFullRechargeableResources()
        {
            return Enum.GetValues<RechargeableResourceEnum>()
                .Cast<RechargeableResourceEnum>()
                .Where(resource => InternalResources.GetAmount(resource) >= ResourceMaximums.GetAmount(resource));
        }

        public IEnumerable<ConsumableResourceEnum> GetFullConsumableResources()
        {
            return Enum.GetValues<ConsumableResourceEnum>()
                .Cast<ConsumableResourceEnum>()
                .Where(resource => InternalResources.GetAmount(resource) >= ResourceMaximums.GetAmount(resource));
        }

        public ISet<EnemyDropEnum> GetUnneededDrops()
        {
            return Model.Rules.GetUnneededDrops(GetFullRechargeableResources());
        }

        protected Dictionary<string, GameFlag> InternalActiveGameFlags { get; set; } = new Dictionary<string, GameFlag>();
        public ReadOnlyDictionary<string, GameFlag> ActiveGameFlags => InternalActiveGameFlags.AsReadOnly();

        public Dictionary<string, GameFlag> GetActiveGameFlagsExceptIn(ReadOnlyInGameState other)
        {
            Dictionary<string, GameFlag> returnFlags = new Dictionary<string, GameFlag>();

            // For each flag, just check for absence in other
            foreach (KeyValuePair<string, GameFlag> kvp in InternalActiveGameFlags)
            {
                if (!other.ActiveGameFlags.ContainsFlag(kvp.Key))
                {
                    returnFlags.Add(kvp.Key, kvp.Value);
                }
            }

            return returnFlags;
        }

        /// <summary>
        /// Adds the provided game flag to the activated game flags in this InGameState.
        /// </summary>
        /// <param name="flag">Flag to add</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="ModelElementMismatchException">Thrown if the game flag instance found in the model is different from the provided one</exception>
        public InGameState ApplyAddGameFlag(GameFlag flag)
        {
            if (flag != Model.GameFlags[flag.Name])
            {
                throw new ModelElementMismatchException(flag);
            }
            if (!InternalActiveGameFlags.ContainsFlag(flag))
            {
                InternalActiveGameFlags.Add(flag.Name, flag);
            }
            return this;
        }

        /// <summary>
        /// Adds the game flag with the provided name to the activated game flags in this InGameState.
        /// </summary>
        /// <param name="flagName">Name of flag to add</param>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyAddGameFlag(string flagName)
        {
            if (!InternalActiveGameFlags.ContainsFlag(flagName))
            {
                InternalActiveGameFlags.Add(flagName, Model.GameFlags[flagName]);
            }
            return this;
        }

        protected Dictionary<string, NodeLock> InternalOpenedLocks { get; set; } = new Dictionary<string, NodeLock>();
        public ReadOnlyDictionary<string, NodeLock> OpenedLocks => InternalOpenedLocks.AsReadOnly();

        public Dictionary<string, NodeLock> GetOpenedNodeLocksExceptIn(ReadOnlyInGameState other)
        {

            Dictionary<string, NodeLock> returnLocks = new Dictionary<string, NodeLock>();

            // For each lock, just check for absence in other
            foreach (KeyValuePair<string, NodeLock> kvp in InternalOpenedLocks)
            {
                if (!other.OpenedLocks.ContainsLock(kvp.Key))
                {
                    returnLocks.Add(kvp.Key, kvp.Value);
                }
            }

            return returnLocks;
        }

        /// <summary>
        /// Applies the opening of the provided lock in this InGameState.
        /// </summary>
        /// <param name="nodeLock">Lock to open</param>
        /// <param name="applyToRoomState">If true, will also remember the lock as being opened in the current room visit.
        /// This can only be done if Samus is at the node that has the lock. Set this to false to unlock a lock remotely.</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="ModelElementMismatchException">Thrown if the lock instance found in the model is different from the provided one</exception>
        public InGameState ApplyOpenLock(NodeLock nodeLock, bool applyToRoomState = true)
        {
            if (nodeLock != Model.Locks[nodeLock.Name])
            {
                throw new ModelElementMismatchException(nodeLock);
            }
            if (!InternalOpenedLocks.ContainsLock(nodeLock))
            {
                InternalOpenedLocks.Add(nodeLock.Name, nodeLock);
                if (applyToRoomState)
                {
                    InternalInRoomState.ApplyOpenLock(nodeLock);
                }
            }
            return this;
        }

        /// <summary>
        /// Applies the opening of the lock with the provided name in this InGameState.
        /// </summary>
        /// <param name="nodeLockName">Name of lock to open</param>
        /// <param name="applyToRoomState">If true, will also remember the lock as being opened in the current room visit.
        /// This can only be done if Samus is at the node that has the lock. Set this to false to unlock a lock remotely.</param>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyOpenLock(string nodeLockName, bool applyToRoomState = true)
        {
            if (!InternalOpenedLocks.ContainsLock(nodeLockName))
            {
                NodeLock nodeLock = Model.Locks[nodeLockName];
                InternalOpenedLocks.Add(nodeLockName, nodeLock);
                if (applyToRoomState)
                {
                    InternalInRoomState.ApplyOpenLock(nodeLock);
                }
            }
            return this;
        }

        /// <summary>
        /// Applies the bypassing of the provided lock in this InGameState. Expects that samus is at the node that has that lock.
        /// </summary>
        /// <param name="nodeLock">Lock to bypass</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="ModelElementMismatchException">Thrown if the lock instance found in the model is different from the provided one</exception>
        public InGameState ApplyBypassLock(NodeLock nodeLock)
        {
            if (nodeLock != Model.Locks[nodeLock.Name])
            {
                throw new ModelElementMismatchException(nodeLock);
            }
            if (!InternalOpenedLocks.ContainsLock(nodeLock))
            {
                InternalInRoomState.ApplyBypassLock(nodeLock);
            }
            return this;
        }

        /// <summary>
        /// Applies the bypassing of the lock with the provded name in this InGameState. Expects that samus is at the node that has that lock.
        /// </summary>
        /// <param name="lockName">Name of the lock to bypass</param>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyBypassLock(string lockName)
        {
            if (!InternalOpenedLocks.ContainsLock(lockName))
            {
                InternalInRoomState.ApplyBypassLock(Model.Locks[lockName]);
            }
            return this;
        }

        /// <summary>
        /// Returns the locks bypassed by Samus at the current node.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (using the last known state in the resulting room if so).
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public IEnumerable<NodeLock> GetBypassedExitLocks(int previousRoomCount = 0)
        {
            return GetInternalInRoomState(previousRoomCount).BypassedExitLocks;
        }

        protected Dictionary<string, RoomNode> InternalTakenItemLocations { get; set; } = new Dictionary<string, RoomNode>();

        public ReadOnlyDictionary<string, RoomNode> TakenItemLocations => InternalTakenItemLocations.AsReadOnly();

        public Dictionary<string, RoomNode> GetTakenItemLocationsExceptIn(ReadOnlyInGameState other)
        {
            Dictionary<string, RoomNode> returnLocations = new Dictionary<string, RoomNode>();

            // For each location, just check for absence in other
            foreach (KeyValuePair<string, RoomNode> kvp in InternalTakenItemLocations)
            {
                if (!other.TakenItemLocations.ContainsNode(kvp.Key))
                {
                    returnLocations.Add(kvp.Key, kvp.Value);
                }
            }

            return returnLocations;
        }

        /// <summary>
        /// Adds the provided location to the taken locations in this InGameState.
        /// Does not modify the inventory.
        /// </summary>
        /// <param name="location">Node of the location to add</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="ModelElementMismatchException">Thrown if the node instance found in the model is different from the provided one</exception>
        public InGameState ApplyTakeLocation(RoomNode location)
        {
            if (Model.Nodes[location.Name] != location)
            {
                throw new ModelElementMismatchException(location);
            }
            if (!TakenItemLocations.ContainsNode(location))
            {
                InternalTakenItemLocations.Add(location.Name, location);
            }
            return this;
        }

        /// <summary>
        /// Adds the location identified by the provided room name and in-room ID to the taken locations in this InGameState.
        /// Does not modify the inventory.
        /// </summary>
        /// <param name="roomName">Name of the room the location is in</param>
        /// <param name="nodeId">In-room ID of the location</param>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyTakeLocation(string roomName, int nodeId)
        {
            RoomNode location = Model.Rooms[roomName].Nodes[nodeId];
            if (!TakenItemLocations.ContainsNode(location))
            {
                InternalTakenItemLocations.Add(location.Name, location);
            }
            return this;
        }

        protected ItemInventory InternalInventory { get; set; }

        public ReadOnlyItemInventory Inventory => InternalInventory.AsReadOnly();

        public ReadOnlyResourceCount BaseResourceMaximums => InternalInventory.BaseResourceMaximums;

        public ReadOnlyResourceCount ResourceMaximums => InternalInventory.ResourceMaximums;

        public ItemInventory GetInventoryExceptIn(ReadOnlyInGameState other)
        {
            return InternalInventory.ExceptIn(other.Inventory);
        }

        /// <summary>
        /// Adds the provided item to the player's inventory for this InGameState.
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="ModelElementMismatchException">Thrown if the item instance found in the model is different from the provided one</exception>
        public InGameState ApplyAddItem(Item item)
        {
            if (Model.Items[item.Name] != item)
            {
                throw new ModelElementMismatchException(item);
            }
            InternalInventory.ApplyAddItem(item);
            return this;
        }

        /// <summary>
        /// Adds the item with the provided name to the player's inventory for this InGameState.
        /// </summary>
        /// <param name="item">Name of the item to add</param>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyAddItem(string itemName)
        {
            InternalInventory.ApplyAddItem(Model.Items[itemName]);
            return this;
        }

        /// <summary>
        ///  Disables the provided non-consumable if it's in this InGameState.
        ///  Does nothing otherwise.
        /// </summary>
        /// <param name="itemName">Name of the item to disable</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="ModelElementMismatchException">Thrown if the item instance found in the model is different from the provided one</exception>
        public InGameState ApplyDisableItem(Item item)
        {
            if (Model.Items[item.Name] != item)
            {
                throw new ModelElementMismatchException(item);
            }
            InternalInventory.ApplyDisableItem(item);
            return this;
        }

        /// <summary>
        ///  Disables the non-consumable item with the provided name if it's in this InGameState.
        ///  Does nothing otherwise.
        /// </summary>
        /// <param name="itemName">Name of the item to disable</param>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyDisableItem(string itemName)
        {
            InternalInventory.ApplyDisableItem(Model.Items[itemName]);
            return this;
        }

        /// <summary>
        ///  Re-enables the provided non-consumable if it's in this InGameState and disabled.
        ///  Does nothing otherwise.
        /// </summary>
        /// <param name="itemName">Name of the item to enable</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="ModelElementMismatchException">Thrown if the item instance found in the model is different from the provided one</exception>
        public InGameState ApplyEnableItem(Item item)
        {
            if (Model.Items[item.Name] != item)
            {
                throw new ModelElementMismatchException(item);
            }
            InternalInventory.ApplyEnableItem(item);
            return this;
        }

        /// <summary>
        ///  Re-enables the non-consumable item with the provided name if it's in this InGameState and disabled.
        ///  Does nothing otherwise.
        /// </summary>
        /// <param name="itemName">Name of the item to enable</param>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyEnableItem(string itemName)
        {
            InternalInventory.ApplyEnableItem(itemName);
            return this;
        }

        /// <summary>
        /// Updates the in-room state to contain a mention of the destruction of the provided obstacle.
        /// This obstacle should be in the current room.
        /// </summary>
        /// <param name="obstacle">The obstacle to destroy.</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="ModelElementMismatchException">Thrown if the obstacle instance found in the current room is different from the provided one</exception>
        public InGameState ApplyDestroyObstacle(RoomObstacle obstacle)
        {
            InternalInRoomState.ApplyDestroyObstacle(obstacle);
            return this;
        }

        /// <summary>
        /// Updates the in-room state to contain a mention of the destruction of the obstacle with the provided in-room ID.
        /// </summary>
        /// <param name="obstacleId">The in-room ID of the obstacle to destroy.</param>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyDestroyObstacle(string obstacleId)
        {
            InternalInRoomState.ApplyDestroyObstacle(obstacleId);
            return this;
        }

        /// <summary>
        /// In-room state of the current room.
        /// </summary>
        protected InRoomState InternalInRoomState { get; set; }

        public ReadOnlyInRoomState InRoomState => InternalInRoomState.AsReadOnly();

        /// <summary>
        /// In-room state of the last few rooms when they were left. This list remembers no more room states than the PreviousRooms constant.
        /// The closer to the start of the list a state is, the more recently Samus was in it.
        /// </summary>
        protected List<InRoomState> InternalPreviousRoomStates { get; } = new List<InRoomState>();

        public IReadOnlyList<ReadOnlyInRoomState> PreviousRoomStates => InternalPreviousRoomStates.Select(state => state.AsReadOnly()).ToList().AsReadOnly();

        /// <summary>
        /// Returns the in-room state that corresponds to the provided previousRoomCount, for this in-game state.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (using the last known state in the resulting room if so).
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        protected InRoomState GetInternalInRoomState(int previousRoomCount)
        {
            if (previousRoomCount == 0)
            {
                return InternalInRoomState;
            }
            else if (previousRoomCount > 0)
            {
                if (previousRoomCount > InternalPreviousRoomStates.Count)
                {
                    return null;
                }
                return InternalPreviousRoomStates[previousRoomCount - 1];
            }
            else
            {
                throw new ArgumentException("previousRoomCount must not be negative");
            }
        }

        public ReadOnlyInRoomState GetInRoomState(int previousRoomCount)
        {
            return GetInternalInRoomState(previousRoomCount)?.AsReadOnly();
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

            if (InternalPreviousRoomStates.Count >= MaxPreviousRooms)
            {
                InternalPreviousRoomStates.RemoveAt(InternalPreviousRoomStates.Count - 1);
            }
            InternalPreviousRoomStates.Insert(0, previousState);
        }

        public RoomNode GetCurrentNode(int previousRoomCount = 0)
        {
            InRoomState roomState = GetInternalInRoomState(previousRoomCount);
            return roomState?.CurrentNode;
        }

        public RoomNode CurrentNode => GetCurrentNode(0);

        public bool BypassingExitLock(int previousRoomCount = 0)
        {
            InRoomState roomState = GetInternalInRoomState(previousRoomCount);
            return roomState?.BypassedExitLock ?? false;
        }

        public bool OpeningExitLock(int previousRoomCount = 0)
        {
            InRoomState roomState = GetInternalInRoomState(previousRoomCount);
            return roomState?.OpenedExitLock ?? false;
        }

        public Room CurrentRoom => GetCurrentOrPreviousRoom(0);

        public Room GetCurrentOrPreviousRoom(int previousRoomCount)
        {
            InRoomState roomState = GetInternalInRoomState(previousRoomCount);
            return roomState?.CurrentRoom;
        }

        public RoomEnvironment GetCurrentOrPreviousRoomEnvironment(int previousRoomCount = 0)
        {
            Room currentRoom = GetCurrentOrPreviousRoom(previousRoomCount);
            if (currentRoom == null)
            {
                return null;
            }

            RoomNode entranceNode = GetVisitedPath(previousRoomCount)[0].nodeState.Node;
            return currentRoom.RoomEnvironments
                .Where(environment => environment.EntranceNodes == null || environment.EntranceNodes.ContainsKey(entranceNode.Id)).FirstOrDefault();
        }

        public bool IsHeatedRoom(int previousRoomCount = 0)
        {
            RoomEnvironment environment = GetCurrentOrPreviousRoomEnvironment(previousRoomCount);
            return environment != null && environment.Heated;
        }

        public DoorEnvironment GetCurrentDoorEnvironment(int previousRoomCount = 0)
        {
            RoomNode currentNode = GetCurrentNode(previousRoomCount);
            if (currentNode == null || !currentNode.DoorEnvironments.Any())
            {
                return null;
            }

            RoomNode entranceNode = GetVisitedPath(previousRoomCount)[0].nodeState.Node;
            return currentNode.DoorEnvironments
                .Where(environment => environment.EntranceNodes == null || environment.EntranceNodes.ContainsKey(entranceNode.Id)).First();
        }

        public PhysicsEnum? GetCurrentDoorPhysics(int previousRoomCount = 0)
        {
            DoorEnvironment environment = GetCurrentDoorEnvironment(previousRoomCount);
            return environment?.Physics;
        }

        public Strat LastLinkStrat => GetLastLinkStrat();

        public Strat GetLastLinkStrat(int previousRoomCount = 0)
        {
            return GetInternalInRoomState(previousRoomCount)?.LastStrat;
        }

        public IReadOnlyList<int> GetVisitedNodeIds(int previousRoomCount = 0)
        {
            InRoomState roomState = GetInternalInRoomState(previousRoomCount);
            IReadOnlyList<int> returnValue = roomState?.VisitedRoomPath?.Select(pathNodeState => pathNodeState.nodeState.Node.Id).ToList().AsReadOnly();
            return returnValue ?? new List<int>().AsReadOnly();
        }

        public IReadOnlyList<(ReadOnlyInNodeState nodeState, Strat strat)> GetVisitedPath(int previousRoomCount = 0)
        {
            InRoomState roomState = GetInternalInRoomState(previousRoomCount);
            var returnValue = roomState?.VisitedRoomPath;
            return returnValue ?? new List<(ReadOnlyInNodeState, Strat)>().AsReadOnly();
        }

        public IEnumerable<string> GetDestroyedObstacleIds(int previousRoomCount = 0)
        {
            InRoomState roomState = GetInternalInRoomState(previousRoomCount);
            IEnumerable<string> returnValue = roomState?.DestroyedObstacleIds;
            return returnValue ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// <para>Positions the in-game state as it would be after entering a room via the provided node. This may place the player at a different node immediately
        /// if the node calls for it.</para>
        /// <para>This will be executed regardless of the current in-room state. To exit with more applied intelligence, see <see cref="ApplyExitRoom()"/></para>
        /// </para>
        /// </summary>
        /// <param name="entryNode">The node (in the next room) through which the next room will be entered.</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="ModelElementMismatchException">Thrown if the node instance found in the model is different from the provided one</exception>
        public InGameState ApplyEnterRoom(RoomNode entryNode)
        {
            if (Model.Nodes[entryNode.Name] != entryNode)
            {
                throw new ModelElementMismatchException(entryNode);
            }
            // Copy current room state and remember it as previous
            RegisterPreviousRoom(InternalInRoomState.Clone());

            // Enter next room
            InternalInRoomState.ApplyEnterRoom(entryNode);

            return this;
        }

        /// <summary>
        /// <para>Positions the in-game state as it would be after entering a room via the node with the provided in-room ID within the room with the provided name.
        /// This may place the player at a different node immediately if the node calls for it.</para>
        /// <para>This will be executed regardless of the current in-room state. To exit with more applied intelligence, see <see cref="ApplyExitRoom()"/></para>
        /// </para>
        /// </summary>
        /// <param name="roomName">Name of the room to enter</param>
        /// <param name="entryNodeId">In-room ID of the node (in the next room) through which the next room will be entered.</param>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyEnterRoom(string roomName, int entryNodeId)
        {
            RoomNode entryNode = Model.Rooms[roomName].Nodes[entryNodeId];
            // Copy current room state and remember it as previous
            RegisterPreviousRoom(InternalInRoomState.Clone());

            // Enter next room
            InternalInRoomState.ApplyEnterRoom(entryNode);

            return this;
        }

        /// <summary>
        /// Attends to exit the current room at the current node, going to the adjacent room this node is connected to.
        /// This is only possible if the current node allows exiting the room, and if no locks are active on the node.
        /// </summary>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyExitRoom()
        {
            RoomNode outNode = CurrentNode.OutNode;
            if (outNode == null)
            {
                throw new InvalidOperationException($"Cannot exit '{CurrentRoom.Name}' via node {CurrentNode.Id} " +
                    $"because it does not have an out connection to another room.");
            }

            IEnumerable<NodeLock> activeLocks = CurrentNode.GetActiveLocks(Model, this).Values
                .Except(GetBypassedExitLocks(), ObjectReferenceEqualityComparer<NodeLock>.Default);
            if (activeLocks.Any())
            {
                throw new InvalidOperationException($"Cannot exit '{CurrentRoom.Name}' via node {CurrentNode.Id} " +
                    $"because the following locks are active on that node: {string.Join(", ", activeLocks.Select(nodeLock => nodeLock.Name))}.");
            }

            // Exiting is legal, so enter the next room
            ApplyEnterRoom(outNode);

            return this;
        }

        /// <summary>
        /// Positions the in-game state at the provided node. This node should be inside the current room.
        /// </summary>
        /// <param name="nodeToVisit">The node to go to</param>
        /// <param name="strat">The strat through which the node is being reached. Can be null. If not null, only makes sense if 
        /// it's on a link that connects previous node to new node.</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="ModelElementMismatchException">Thrown if the node instance found in the model is different from the provided one,
        /// or if the strat instance found on the link from current node to target node is different from the provided one.</exception>
        public InGameState ApplyVisitNode(RoomNode nodeToVisit, Strat strat)
        {
            if (Model.Nodes[nodeToVisit.Name] != nodeToVisit)
            {
                throw new ModelElementMismatchException(nodeToVisit);
            }
            InternalInRoomState.ApplyVisitNode(nodeToVisit, strat);

            return this;
        }

        /// <summary>
        /// Positions the in-game state at the node with the provided in-room ID. Note that this method is not responsible for actually executing the strat.
        /// </summary>
        /// <param name="nodeToVisitId">In-room ID of the node to go to</param>
        /// <param name="stratName">The name of the strat through which the node is being reached. Can be null. If not null, only makes sense if 
        /// it's on a link that connects previous node to new node. The strat must be on a link from the current node to the ndoe to visit</param>
        /// <returns>This, for chaining</returns>
        public InGameState ApplyVisitNode(int nodeToVisitId, string stratName)
        {
            InternalInRoomState.ApplyVisitNode(nodeToVisitId, stratName);
            return this;
        }

        public IEnumerable<Runway> GetRetroactiveRunways(IEnumerable<int> requiredInRoomPath, IReadOnlySet<PhysicsEnum> acceptablePhysics, int previousRoomCount = 0)
        {
            // Since this is a retroactive check, we already have to look at the room prior to the "current" room for this check
            // If that "current" room is the last remembered one, we have no way to obtain the state of the room before that so just return
            if (previousRoomCount >= InternalPreviousRoomStates.Count)
            {
                return Enumerable.Empty<Runway>();
            }

            // Apply physics restriction if applicable
            if (acceptablePhysics != null && acceptablePhysics.Any())
            {
                PhysicsEnum? activePhysics = GetCurrentDoorPhysics(previousRoomCount + 1);
                if (acceptablePhysics == null || !acceptablePhysics.Contains(activePhysics.Value))
                {
                    return Enumerable.Empty<Runway>();
                }
            }

            // We will need to know what nodes were visited in the current room. If this info is missing, we can't do anything retroactively.
            IReadOnlyList<int> visitedNodeIds = GetVisitedNodeIds(previousRoomCount);
            // If we don't know at what node we entered, we can't identify any usable runways
            if (!visitedNodeIds.Any())
            {
                return Enumerable.Empty<Runway>();
            }

            // If the current in-room state doesn't respect the in-room path necessary to use retroactive runways, we can't use any
            if (!visitedNodeIds.SequenceEqual(requiredInRoomPath))
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
            RoomNode entryNode = GetCurrentOrPreviousRoom(previousRoomCount).Nodes[visitedNodeIds[0]];
            if (previousRoomExitNode.OutNode != entryNode)
            {
                return Enumerable.Empty<Runway>();
            }

            // We've confirmed we can use retroactive runways. Return all runways of the previous room's exit node (excluding logically irrelevant ones)
            return previousRoomExitNode.Runways.Values.WhereLogicallyRelevant();
        }

        public IEnumerable<CanLeaveCharged> GetRetroactiveCanLeaveChargeds(IEnumerable<int> requiredInRoomPath, int previousRoomCount = 0)
        {
            // Since this is a retroactive check, we already have to look at the room prior to the "current" room for this check
            // If that "current" room is the last remembered one, we have no way to obtain the state of the room before that so just return
            if (previousRoomCount >= InternalPreviousRoomStates.Count)
            {
                return Enumerable.Empty<CanLeaveCharged>();
            }

            // We will need to know what nodes were visited in the current room. If this info is missing, we can't do anything retroactively.
            IReadOnlyList<int> visitedNodeIds = GetVisitedNodeIds(previousRoomCount);
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

            RoomNode entryNode = GetCurrentOrPreviousRoom(previousRoomCount).Nodes[visitedNodeIds[0]];

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
            return previousRoomExitNode.CanLeaveCharged.WhereLogicallyRelevant().Where(clc =>
            {
                // If the CanLeaveCharged is initiated remotely, we must take a closer look at what happened in that room
                if (clc.IsInitiatedRemotely)
                {
                    // This CanLeaveCharged is initiated at a specific node,
                    // and is executed by following a specific path through the room, with a subset of allowed strats.
                    // We will check whether last room's exit is compatible with this CanLeaveCharged,
                    // to see if it can possibly be executed retroactively.

                    var lastRoomPath = GetVisitedPath(previousRoomCount + 1);
                    // If we haven't visited as many nodes as the prescribed path to door (+ at least 1 more node to visit the initiateAtNode),
                    // we know for sure our exit isn't compatible with this CanLeaveCharged. Reject it.
                    if (lastRoomPath.Count < clc.InitiateRemotely.PathToDoor.Count() + 1)
                    {
                        return false;
                    }

                    // Check the last n visited nodes to make sure they correspond to the prescribed path.
                    // But before this, check the node we were at immediately before those, to make sure we're starting at the right node.
                    IEnumerable<(ReadOnlyInNodeState nodeState, Strat strat)> lastRoomFinalPath
                        = lastRoomPath.TakeLast(clc.InitiateRemotely.PathToDoor.Count() + 1);
                    if (lastRoomFinalPath.First().nodeState.Node != clc.InitiateRemotely.InitiateAtNode)
                    {
                        return false;
                    }

                    if (!clc.InitiateRemotely.PathToDoor.Zip
                        (lastRoomFinalPath.Skip(1), (expectedPath, actualPath) =>
                        {
                            // These two path nodes match up if they go to the same room node, and if the actual path uses one of the valid strats
                            return expectedPath.linkTo.TargetNode == actualPath.nodeState.Node
                                && expectedPath.strats.ContainsKey(actualPath.strat.Name);
                        })
                        // If any node in the actual path does not respect the expected path, 
                        // then last room's exit is incompatible with this CanLeaveCharged
                        .All(valid => valid))
                    {
                        return false;
                    }

                    // If there is a requirement that the door be opened first, 
                    // there are two additional requirements to execute the CanLeaveCharged retroactively.
                    if (clc.InitiateRemotely.MustOpenDoorFirst)
                    {
                        // The exit node must not have any active locks when the CanLeaveCharged execution begins.
                        // This means we can't have opened the lock on the way out
                        if (OpeningExitLock(previousRoomCount + 1))
                        {
                            return false;
                        }

                        // The exit node must have been visited before the CanLeaveCharge execution would begin.
                        // The node where execution begins is hence the last one where we could have opened the door.
                        if (!lastRoomPath.SkipLast(clc.InitiateRemotely.PathToDoor.Count()).Where(pathNode => pathNode.nodeState.Node == previousRoomExitNode).Any())
                        {
                            return false;
                        }

                        // If there's any active locks on the exit door, then the door can't possibly have been opened first.
                        if (previousRoomExitNode.GetActiveLocks(Model, this).Any())
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

    /// <summary>
    /// Exposes the read-only portion of an <see cref="InGameState"/>.
    /// </summary>
    public interface ReadOnlyInGameState
    {
        /// <summary>
        /// Creates and returns a copy of this InGameState, as a full-fledged modifiable one.
        /// </summary>
        /// <returns>The clone</returns>
        public InGameState Clone();

        /// <summary>
        /// A read-only view of the resources the player currently has in this InGameState.
        /// </summary>
        public ReadOnlyResourceCount Resources { get; }

        /// <summary>
        /// Returns whether it's possible to spend the provided amount of the provided resource.
        /// </summary>
        /// <param name="resource">The resource to check for availability</param>
        /// <param name="quantity">The amount of the resource to check for availability</param>
        /// <returns></returns>
        public bool IsResourceAvailable(ConsumableResourceEnum resource, int quantity);

        /// <summary>
        /// Creates and returns a ResourceCount that expresses how many rechargeable resources this in-game state has,
        /// relative to the provided in-game state. Negative values mean this state has less.
        /// </summary>
        /// <param name="other">The other in-game state to compare with.</param>
        /// <returns></returns>
        public ResourceCount GetResourceVariationWith(ReadOnlyInGameState other);

        /// <summary>
        /// Returns the enumeration of rechargeable resources that are currently full.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RechargeableResourceEnum> GetFullRechargeableResources();

        /// <summary>
        /// Returns the enumeration of consumable resources that are currently full.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ConsumableResourceEnum> GetFullConsumableResources();

        /// <summary>
        /// Returns the enumeration of enemy drops that aren't needed by this in-game state because the associated resources are full.
        /// </summary>
        /// <returns></returns>
        public ISet<EnemyDropEnum> GetUnneededDrops();

        /// <summary>
        /// The read-only dictionary of game flags that are active in this InGameState, mapped by their name.
        /// </summary>
        public ReadOnlyDictionary<string, GameFlag> ActiveGameFlags { get; }

        /// <summary>
        /// Creates and returns a new dictionary containing all active game flags from this in-game state
        /// that aren't active in the provided other in-game state.
        /// </summary>
        /// <param name="other">The other in-game state</param>
        /// <returns></returns>
        public Dictionary<string, GameFlag> GetActiveGameFlagsExceptIn(ReadOnlyInGameState other);

        /// <summary>
        /// The read-only dictionary of locks that are opened in this InGameState, mapped by their name.
        /// </summary>
        public ReadOnlyDictionary<string, NodeLock> OpenedLocks { get; }

        /// <summary>
        /// Creates and returns a new dictionary containing all OPENED NODE LOCKS from this in-game state
        /// that aren't OPENED in the provided other in-game state.
        /// </summary>
        /// <param name="other">The other in-game state</param>
        /// <returns></returns>
        public Dictionary<string, NodeLock> GetOpenedNodeLocksExceptIn(ReadOnlyInGameState other);

        /// <summary>
        /// Returns the locks bypassed by Samus at the current node.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (using the last known state in the resulting room if so).
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public IEnumerable<NodeLock> GetBypassedExitLocks(int previousRoomCount = 0);

        /// <summary>
        /// The read-only dictionary of item locations that have been taken in this InGameState, mapped by their name.
        /// Take note that while node names are not guaranteed to be unique, all item node names are.
        /// </summary>
        public ReadOnlyDictionary<string, RoomNode> TakenItemLocations { get; }

        /// <summary>
        /// Creates and returns a new dictionary containing all taken item locations from this in-game state
        /// that aren't taken in the provided other in-game state.
        /// </summary>
        /// <param name="other">The other in-game state</param>
        /// <returns></returns>
        public Dictionary<string, RoomNode> GetTakenItemLocationsExceptIn(ReadOnlyInGameState other);

        /// <summary>
        /// The read-only inventory of items collected by Samus according to this in-game state.
        /// </summary>
        public ReadOnlyItemInventory Inventory { get; }

        // We won't do delegate properties for inventory properties that more obviously belong to the inventory,
        // but BaseResourceMaximums and ResourceMaximums aren't that obvious so those two could be helpful.
        /// <summary>
        /// The resource maximums that the player would have in this InGameState if inventory were empty.
        /// </summary>
        public ReadOnlyResourceCount BaseResourceMaximums { get; }

        /// <summary>
        /// The resource maximums that the player currently has in this InGameState.
        /// </summary>
        public ReadOnlyResourceCount ResourceMaximums { get; }

        /// <summary>
        /// Creates and returns a new ItemInventory containing all items from this in-game state
        /// that aren't found in the provided other in-game state.
        /// </summary>
        /// <param name="other">The other in-game state</param>
        /// <returns></returns>
        public ItemInventory GetInventoryExceptIn(ReadOnlyInGameState other);

        /// <summary>
        /// Read-only portion of the in-room state of the current room.
        /// </summary>
        public ReadOnlyInRoomState InRoomState { get; }

        /// <summary>
        /// Read-only in-room state of the last few rooms when they were left. This list remembers no more room states than the PreviousRooms constant.
        /// The closer to the start of the list a state is, the more recently Samus was in it.
        /// </summary>
        public IReadOnlyList<ReadOnlyInRoomState> PreviousRoomStates { get; }

        /// <summary>
        /// Returns the read-only in-room state that corresponds to the provided previousRoomCount, for this in-game state.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (using the last known state in the resulting room if so).
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public ReadOnlyInRoomState GetInRoomState(int previousRoomCount);

        /// <summary>
        /// Returns the node the player is currently at or was last at, in the room indicated by previousRoomCount. This can be null if in-room state isn't being tracked.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (using the last known state in the resulting room if so).
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public RoomNode GetCurrentNode(int previousRoomCount = 0);

        /// <summary>
        /// The node the player is currently at. This can be null if in-room state isn't being tracked.
        /// </summary>
        /// <returns></returns>
        public RoomNode CurrentNode { get; }

        /// <summary>
        /// Returns whether the player is exiting the room by bypassing a lock on the node they are exiting by.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by.
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public bool BypassingExitLock(int previousRoomCount = 0);

        /// <summary>
        /// Returns whether the player is exiting the room by opening a lock on the node they are exiting by.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by.
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public bool OpeningExitLock(int previousRoomCount = 0);

        /// <summary>
        /// The room the player is currently in.
        /// </summary>
        public Room CurrentRoom { get; }

        /// <summary>
        /// Returns the room the player is currently in or was previously in.
        /// This can be null if in-room state isn't being tracked.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by. 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public Room GetCurrentOrPreviousRoom(int previousRoomCount);

        /// <summary>
        /// Returns the RoomEnvironment applicable to the room the player is currently in, or was previously in. This can be null if in-room state isn't being tracked.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by.
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public RoomEnvironment GetCurrentOrPreviousRoomEnvironment(int previousRoomCount = 0);

        /// <summary>
        /// Returns whether the room the player currently in is heated. Defaults to false if in-room state isn't being tracked.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by.
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public bool IsHeatedRoom(int previousRoomCount = 0);

        /// <summary>
        /// Returns the DoorEnvironment applicable to the node the player is currently in (if it has one). This can also be null if in-room state isn't being tracked.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (using the last known state in the resulting room if so).
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public DoorEnvironment GetCurrentDoorEnvironment(int previousRoomCount = 0);

        /// <summary>
        /// Returns the door physics (if any) at the current node.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (using the last known state in the resulting room if so).
        /// 0 means current room, 3 means go back 3 rooms, negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public PhysicsEnum? GetCurrentDoorPhysics(int previousRoomCount = 0);

        /// <summary>
        /// The strat that was used to reach the current node, if any. Otherwise, is null.
        /// </summary>
        public Strat LastLinkStrat { get; }

        /// <summary>
        /// Returns the strat that was used to reach the current node, if any. Otherwise, returns null.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (using the last known state in the resulting room if so).
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public Strat GetLastLinkStrat(int previousRoomCount = 0);

        /// <summary>
        /// Returns a list of IDs of nodes that have been visited in the specified room since entering, in order, 
        /// starting with the node through which the room was entered. May be empty if the in-room state is not being tracked.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by.
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public IReadOnlyList<int> GetVisitedNodeIds(int previousRoomCount = 0);

        /// <summary>
        /// Returns a list of nodes (represented as an InNodeState) that have been visited in the specified room since entering, in order,
        /// starting with the node through which the room was entered. May be empty if the in-room state is not being tracked.
        /// Note that this list may not stay in sync with future changes to this InGameState.
        /// Each node state is accompanied by the strat that was used to reach the node, when applicable.
        /// This strat can be null for nodes visited during the process of spawning in the room (always the first node, and sometimes the second).
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by. 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public IReadOnlyList<(ReadOnlyInNodeState nodeState, Strat strat)> GetVisitedPath(int previousRoomCount = 0);

        /// <summary>
        /// Returns a sequence of IDs of obstacles that have been destroyed in the current room since entering.
        /// May be empty if the in-room state is not being tracked.
        /// </summary>
        /// <param name="previousRoomCount">The number of playable rooms to go back by. 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public IEnumerable<string> GetDestroyedObstacleIds(int previousRoomCount = 0);

        /// <summary>
        /// <para>Returns all runways that the player could possibly be able to retroactively use, according to the pathing in this in-game state.
        /// Does not check whether the player is also able to use any strats on those runways.
        /// Also does not check whether charging is currently doable.</para>
        /// <para>"Retroactive use" is meant to be done right after entering a room, and aims to retroactively decide how the last room was exited.</para>
        /// <para>A runway would typically be used retroactively to satisfy an adjacentRunway or canComeInCharged
        /// that is being executed soon after entry of the new room.</para>
        /// </summary>
        /// <param name="requiredInRoomPath">The path that must have been followed in the current room (as successive node IDs) in order to be able 
        /// to use retroactive runways in the current context. The first node in this path also dictates the node to which the retroactive runways must lead.</param>
        /// <param name="usePreviousRoom">If true, indicates that the "new" room is already the previous room in this InGameState.</param>
        /// <param name="acceptablePhysics">An optional set of physics, one of which must be active at the runway's door for any runway to be available.
        /// If null, all physics are acceptable.</param>
        /// <param name="previousRoomCount">The number of playable rooms to go back by, *before* looking for retroactive runways in the room before that. 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public IEnumerable<Runway> GetRetroactiveRunways(IEnumerable<int> requiredInRoomPath, IReadOnlySet<PhysicsEnum> acceptablePhysics, int previousRoomCount = 0);

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
        /// <param name="previousRoomCount">The number of playable rooms to go back by, *before* looking for retroactive canLeaveChargeds in the room before that.
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public IEnumerable<CanLeaveCharged> GetRetroactiveCanLeaveChargeds(IEnumerable<int> requiredInRoomPath, int previousRoomCount = 0);
    }
}
