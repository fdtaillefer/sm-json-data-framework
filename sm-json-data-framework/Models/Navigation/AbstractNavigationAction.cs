using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Node;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Navigation
{
    /// <summary>
    /// An abstract superclass for actions that can be performed by a <see cref="GameNavigator"/>.
    /// These actions describe the results of the action after the fact, not the intent before the fact.
    /// </summary>
    public abstract class AbstractNavigationAction
    {
        protected AbstractNavigationAction(string intent)
        {
            IntentDescription = intent;
        }

        public AbstractNavigationAction(string intent, SuperMetroidModel model, InGameState initialInGameState, ExecutionResult executionResult): this(intent)
        {
            Succeeded = true;

            // Initialize position change
            if(initialInGameState.GetCurrentNode() != executionResult.ResultingState.GetCurrentNode())
            {
                PositionChange = (initialInGameState.GetCurrentNode(), executionResult.ResultingState.GetCurrentNode());
            }

            // Initialize gained and lost items
            ItemInventory gainedInventory = executionResult.ResultingState.GetInventoryExceptWith(initialInGameState);
            ItemsGained = gainedInventory;
            // Cannot lose items, just create an empty inventory
            ItemsLost = new ItemInventory(model.StartConditions.BaseResourceMaximums);

            // Initialize enabled and disabled items
            ItemsDisabledNames = executionResult.ResultingState.GetDisabledItemNames().Except(initialInGameState.GetDisabledItemNames());
            ItemsEnabledNames = initialInGameState.GetDisabledItemNames().Except(executionResult.ResultingState.GetDisabledItemNames());

            // Initialize flags gained
            GameFlagsGained = GameFlagsGained.Concat(executionResult.ResultingState.GetActiveGameFlagsExceptWith(initialInGameState).Values);

            // Initialize locks opened
            LocksOpened = LocksOpened.Concat(executionResult.ResultingState.GetOpenedNodeLocksExceptWith(initialInGameState).Values);

            // Initialize item locations taken
            ItemLocationsTaken = ItemLocationsTaken.Concat(executionResult.ResultingState.GetTakenItemLocationsExceptWith(initialInGameState).Values);

            // Initialize resources before and after
            ResourcesBefore = initialInGameState.GetCurrentResources();
            ResourcesAfter = executionResult.ResultingState.GetCurrentResources();

            // Initialize destroyed obstacles, but that's only relevant if we didn't change rooms
            if (executionResult.ResultingState.GetCurrentRoom() == initialInGameState.GetCurrentRoom())
            {
                ObstaclesDestroyed = ObstaclesDestroyed.Concat(
                    executionResult.ResultingState.GetDestroyedObstacleIds()
                        .Except(initialInGameState.GetDestroyedObstacleIds())
                        .Select(obstacleId => executionResult.ResultingState.GetCurrentRoom().Obstacles[obstacleId])
                    );
            }

            // Transfer information data from the ExecutionResult.
            // No need to copy since they are IEnumerable and not supposed to be mutated.
            RunwaysUsed = executionResult.RunwaysUsed;
            CanLeaveChargedExecuted = executionResult.CanLeaveChargedExecuted;
            OpenedLocks = executionResult.OpenedLocks;
            BypassedLocks = executionResult.BypassedLocks;
            KilledEnemies = executionResult.KilledEnemies;

            // Since the set of items is mutable, do not transfer the instance
            ItemsInvolved.UnionWith(executionResult.ItemsInvolved);
            DamageReducingItemsInvolved.UnionWith(executionResult.DamageReducingItemsInvolved);
        }

        /// <summary>
        /// A description of what was being attempted which resulted in this action.
        /// </summary>
        public string IntentDescription { get; set; } = "";

        #region Information about the action's effects
        /// <summary>
        /// Indicates whether this action was a success. A failed attempt is fully ignored by a GameNavigator and doesn't actually change the state.
        /// </summary>
        public bool Succeeded { get; protected set; }

        /// <summary>
        /// If true, this action expresses the reverse effect of an action that was actually performed.
        /// </summary>
        public bool IsReverseAction { get; protected set; } = false;

        /// <summary>
        /// If this action changed the player's position, this property describes that change. Otherwise, this is null.
        /// </summary>
        public (RoomNode fromNode, RoomNode toNode) PositionChange { get; protected set; } = (null, null);

        /// <summary>
        /// The inventory of items that have been gained by the player as a result of this action.
        /// </summary>
        public ItemInventory ItemsGained { get; set; }

        /// <summary>
        /// The inventory of items that have been lost by the player as a result of this action.
        /// This can only really happen by reversing an action.
        /// </summary>
        public ItemInventory ItemsLost { get; set; }

        /// <summary>
        /// The names of items that have been disabled by the player as a result of this action.
        /// </summary>
        public IEnumerable<string> ItemsDisabledNames { get; set; } = new HashSet<string>();

        /// <summary>
        /// The names of items that have been enabled by the player as a result of this action.
        /// </summary>
        public IEnumerable<string> ItemsEnabledNames { get; set; } = new HashSet<string>();

        /// <summary>
        /// The enumeration of game flags that have been obtained by the player as a result of this action.
        /// </summary>
        public IEnumerable<GameFlag> GameFlagsGained { get; protected set; } = Enumerable.Empty<GameFlag>();

        /// <summary>
        /// The enumeration of game flags that have been obtained by the player as a result of this action.
        /// </summary>
        public IEnumerable<GameFlag> GameFlagsLost { get; protected set; } = Enumerable.Empty<GameFlag>();

        /// <summary>
        /// The enumeration of node locks that have been opened as a result of this action.
        /// </summary>
        public IEnumerable<NodeLock> LocksOpened { get; protected set; } = Enumerable.Empty<NodeLock>();

        /// <summary>
        /// The enumeration of node  locks that have been closed as a result of this action.
        /// This can only really happen by reversing an action.
        /// </summary>
        public IEnumerable<NodeLock> LocksClosed { get; protected set; } = Enumerable.Empty<NodeLock>();

        /// <summary>
        /// The enumeration of item locations whose item has been taken by the player as a result of this action.
        /// </summary>
        public IEnumerable<RoomNode> ItemLocationsTaken { get; protected set; } = Enumerable.Empty<RoomNode>();

        /// <summary>
        /// The enumeration of item locations whose item has been put back where it was as a result of this action.
        /// </summary>
        public IEnumerable<RoomNode> ItemLocationsPutBack { get; protected set; } = Enumerable.Empty<RoomNode>();

        /// <summary>
        /// <para>The enumeration of in-room obstacles that have been destroyed as a result of this action.</para>
        /// <para>Note that changes to obstacles are not kept when changing rooms.</para>
        /// </summary>
        public IEnumerable<RoomObstacle> ObstaclesDestroyed { get; protected set; } = Enumerable.Empty<RoomObstacle>();

        /// <summary>
        /// <para>The enumeration of in-room obstacles that have been restored as a result of this action.
        /// This can only really happen by reversing an action, since it's deemed unnecessary to indicate that exiting a room restores obstacles.</para>
        /// <para>Note that changes to obstacles are not kept when changing rooms.</para>
        /// </summary>
        public IEnumerable<RoomObstacle> ObstaclesRestored { get; protected set; } = Enumerable.Empty<RoomObstacle>();

        /// <summary>
        /// A ResourceCount representing the player's resources before this action.
        /// </summary>
        ResourceCount ResourcesBefore { get; set; }

        /// <summary>
        /// A ResourceCount representing the player's resources after this action.
        /// </summary>
        ResourceCount ResourcesAfter { get; set; }
        #endregion

        #region Information about how the action was performed
        /// <summary>
        /// A sequence of runways that were used (possibly retroactively) along with the accompanying runway strat.
        /// </summary>
        public IEnumerable<(Runway runwayUsed, Strat stratUsed)> RunwaysUsed { get; set; } = Enumerable.Empty<(Runway, Strat)>();

        /// <summary>
        /// A sequence of canLeaveCharged that were executed (possibly retroactively) along with the accompanying canLeaveCharged strat.
        /// </summary>
        public IEnumerable<(CanLeaveCharged canLeaveChargedUsed, Strat stratUsed)> CanLeaveChargedExecuted { get; set; } = Enumerable.Empty<(CanLeaveCharged, Strat)>();

        /// <summary>
        /// A sequence of node locks that were opened along with the open strat used to opem them.
        /// </summary>
        public IEnumerable<(NodeLock openedLock, Strat stratUsed)> OpenedLocks { get; set; } = Enumerable.Empty<(NodeLock openedLock, Strat stratUsed)>();

        /// <summary>
        /// A sequence of node locks that were bypassed along with the bypass strat used to opem them.
        /// </summary>
        public IEnumerable<(NodeLock bypassedLock, Strat stratUsed)> BypassedLocks { get; set; } = Enumerable.Empty<(NodeLock bypassedLock, Strat stratUsed)>();

        /// <summary>
        /// A sequence of enemies that were killed, along with the weapon and number of shots used.
        /// </summary>
        public IEnumerable<IndividualEnemyKillResult> KilledEnemies { get; set; } = Enumerable.Empty<IndividualEnemyKillResult>();

        /// <summary>
        /// A sequence of items that were involved in some way, excluding damage reduction
        /// and operation of weapons already present in <see cref="KilledEnemies"/>.
        /// </summary>
        public ISet<Item> ItemsInvolved { get; set; } = new HashSet<Item>(ObjectReferenceEqualityComparer<Item>.Default);

        /// <summary>
        /// A sequence of items that were involved in reducing incoming damage.
        /// </summary>
        public ISet<Item> DamageReducingItemsInvolved { get; set; } = new HashSet<Item>(ObjectReferenceEqualityComparer<Item>.Default);
        #endregion

        /// <summary>
        /// Creates and returns an action representing the reverse of this action.
        /// </summary>
        /// <returns></returns>
        public abstract AbstractNavigationAction Reverse(SuperMetroidModel model);

        /// <summary>
        /// Transfers to the provided action data from an AbstractNavigationAction that corresponds 
        /// to doing the opposite of this action (in other words, the equivalent of undoing it).
        /// </summary>
        /// <param name="reverseAction">The action to which to transfer reverse data</param>
        protected virtual void TransferDataToReverseAbstractAction(AbstractNavigationAction reverseAction)
        {
            // If this action succeeded at doing something, the reverse would succeed at undoing it
            reverseAction.Succeeded = Succeeded;
            reverseAction.IsReverseAction = true;

            // Initialize reversed position change
            reverseAction.PositionChange = (PositionChange.toNode, PositionChange.fromNode);

            // Initialize reversed inventory change
            reverseAction.ItemsGained = ItemsLost.Clone();
            reverseAction.ItemsLost = ItemsGained.Clone();

            // Initialize reversed enabled/disabled items
            reverseAction.ItemsDisabledNames = ItemsEnabledNames;
            reverseAction.ItemsEnabledNames = ItemsDisabledNames;

            // Initialize reversed flags change
            // Don't clone, expect that IEnumerables won't get mutated.
            reverseAction.GameFlagsGained = GameFlagsLost;
            reverseAction.GameFlagsLost = GameFlagsGained;

            // Initialize reversed locks change
            // Don't clone, expect that IEnumerables won't get mutated.
            reverseAction.LocksOpened = LocksClosed;
            reverseAction.LocksClosed = LocksOpened;

            // Initialize reversed item location taken change
            // Don't clone, expect that IEnumerables won't get mutated.
            reverseAction.ItemLocationsTaken = ItemLocationsPutBack;
            reverseAction.ItemLocationsPutBack = ItemLocationsTaken;

            // Initialize reversed resource counts
            reverseAction.ResourcesBefore = ResourcesAfter.Clone();
            reverseAction.ResourcesAfter = ResourcesBefore.Clone();

            // Initialize reversed obstacles change
            // Don't clone, expect that IEnumerables won't get mutated.
            reverseAction.ObstaclesDestroyed = ObstaclesRestored;
            reverseAction.ObstaclesRestored = ObstaclesDestroyed;

            // Don't transfer information data. It makes little sense for a reverse action
        }

        /// <summary>
        /// Outputs to console a summary of this action. Always outputs succedd/failure,
        /// and optionally outputs more depending on provided paramters.
        /// </summary>
        /// <param name="outputEffects">If true, will output how the action impacted the game state.</param>
        /// <param name="outputDetails">If true, will output details about how the action was performed.</param>
        public void OutputToConsole(bool outputEffects, bool outputDetails)
        {
            Console.WriteLine("");
            Console.WriteLine($"Action attempted: {IntentDescription}");
            if (!Succeeded)
            {
                Console.WriteLine("Action failed");
                return;
            }

            if(IsReverseAction)
            {
                Console.WriteLine("Action reversal executed");
            }
            else
            {
                Console.WriteLine(GetSuccessOutputString());
            }

            if (outputDetails)
            {
                foreach(var (runway, strat) in RunwaysUsed)
                {
                    Console.WriteLine($"A runway on node '{runway.Node.Name}' was used, by executing strat '{strat.Name}'");
                }

                foreach(var (canLeaveCharged, strat) in CanLeaveChargedExecuted)
                {
                    Console.WriteLine($"A canLeaveCharged on node '{canLeaveCharged.Node.Name}' was used, by executing strat '{strat.Name}'");
                }

                foreach(var (nodeLock, strat) in OpenedLocks)
                {
                    Console.WriteLine($"Lock '{nodeLock.Name}' was opened, by executing strat '{strat.Name}'");
                }

                foreach(var (nodeLock, strat) in BypassedLocks)
                {
                    Console.WriteLine($"Lock '{nodeLock.Name}' was bypassed, by executing strat '{strat.Name}'");
                }

                foreach(var killResult in KilledEnemies)
                {
                    string killMethodString = String.Join(" and ", killResult.KillMethod.Select(method => $"{method.shots} shots of weapon {method.weapon.Name}"));
                    Console.WriteLine($"Enemy '{killResult.Enemy.Name}' was killed, using {killMethodString}.");
                }

                foreach(var item in ItemsInvolved)
                {
                    Console.WriteLine($"Item '{item.Name}' was used during execution.");
                }

                foreach(var item in DamageReducingItemsInvolved)
                {
                    Console.WriteLine($"Item '{item.Name}' helped reduce incoming damage.");
                }
            }

            if (outputEffects)
            {
                // Items gained and lost
                foreach (Item item in ItemsGained.GetNonConsumableItemsDictionary().Values)
                {
                    Console.WriteLine($"Gained item '{item.Name}'");
                }
                foreach (var (item, count) in ItemsGained.GetExpansionItemsDictionary().Values)
                {
                    Console.WriteLine($"Gained item '{item.Name}' X {count}");
                }

                foreach (Item item in ItemsLost.GetNonConsumableItemsDictionary().Values)
                {
                    Console.WriteLine($"Lost item '{item.Name}'");
                }
                foreach (var (item, count) in ItemsLost.GetExpansionItemsDictionary().Values)
                {
                    Console.WriteLine($"Lost item '{item.Name}' X {count}");
                }

                // Items enabled and disabled
                foreach(string itemName in ItemsDisabledNames)
                {
                    Console.WriteLine($"Disabled item '{itemName}'");
                }
                foreach (string itemName in ItemsEnabledNames)
                {
                    Console.WriteLine($"Enabled item '{itemName}'");
                }

                // Resource variation
                foreach (RechargeableResourceEnum currentResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
                {
                    int countAfter = ResourcesAfter.GetAmount(currentResource);

                    int variation = countAfter - ResourcesBefore.GetAmount(currentResource);
                    if (variation < 0)
                    {
                        Console.WriteLine($"Used up {variation * -1} of resource {currentResource}, current amount now {countAfter}");
                    }
                    else if (variation > 0)
                    {
                        Console.WriteLine($"Gained {variation} of resource {currentResource}, current amount now {countAfter}");
                    }
                }

                // Game flags gained and lost
                foreach (GameFlag flag in GameFlagsGained)
                {
                    Console.WriteLine($"Gained game flag '{flag.Name}'");
                }

                foreach (GameFlag flag in GameFlagsLost)
                {
                    Console.WriteLine($"Lost game flag '{flag.Name}'");
                }

                // Locks opened and closed
                foreach (NodeLock nodeLock in LocksOpened)
                {
                    Console.WriteLine($"Opened lock '{nodeLock.Name}'");
                }

                foreach (NodeLock nodeLock in LocksClosed)
                {
                    Console.WriteLine($"Closed lock '{nodeLock.Name}'");
                }

                // Item locations taken and put back
                foreach (RoomNode itemLocation in ItemLocationsTaken)
                {
                    Console.WriteLine($"Item at location '{itemLocation.Name}' has been taken");
                }

                foreach (RoomNode itemLocation in ItemLocationsPutBack)
                {
                    Console.WriteLine($"Item at location '{itemLocation.Name}' has been put back");
                }

                //Obstacles destroyed and restored
                foreach (RoomObstacle obstacle in ObstaclesDestroyed)
                {
                    Console.WriteLine($"Destroyed obstacle '{obstacle.Name}'");
                }

                foreach (RoomObstacle obstacle in ObstaclesRestored)
                {
                    Console.WriteLine($"Restored obstacle '{obstacle.Name}'");
                }

                // Position change
                if (PositionChange.fromNode != PositionChange.toNode)
                {
                    Console.WriteLine($"Current node changed from '{PositionChange.fromNode.Name}' to '{PositionChange.toNode.Name}'");
                }
            }
        }

        public virtual string GetSuccessOutputString()
        {
            return "Action succeeded";
        }
    }
}
