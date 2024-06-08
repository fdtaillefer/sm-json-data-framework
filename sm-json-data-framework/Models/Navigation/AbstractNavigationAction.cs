using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
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

        public AbstractNavigationAction(string intent, SuperMetroidModel model, ReadOnlyInGameState initialInGameState, ExecutionResult executionResult): this(intent)
        {
            Succeeded = true;

            // Initialize position change
            if(initialInGameState.GetCurrentNode() != executionResult.ResultingState.GetCurrentNode())
            {
                PositionChange = (initialInGameState.GetCurrentNode(), executionResult.ResultingState.GetCurrentNode());
            }

            // Initialize gained and lost items
            ItemInventory gainedInventory = executionResult.ResultingState.GetInventoryExceptIn(initialInGameState);
            ItemsGained = gainedInventory;
            // Cannot lose items, just create an empty inventory
            ItemsLost = new ItemInventory(model.StartConditions.BaseResourceMaximums.Clone());

            // Initialize enabled and disabled items
            ItemsDisabledNames = executionResult.ResultingState.Inventory.DisabledItemNames.Except(initialInGameState.Inventory.DisabledItemNames).ToHashSet();
            ItemsEnabledNames = initialInGameState.Inventory.DisabledItemNames.Except(executionResult.ResultingState.Inventory.DisabledItemNames).ToHashSet();

            // Initialize flags gained
            GameFlagsGained = GameFlagsGained.Values.Concat(executionResult.ResultingState.GetActiveGameFlagsExceptIn(initialInGameState).Values).ToDictionary(flag => flag.Name);

            // Initialize locks opened
            LocksOpened = LocksOpened.Values.Concat(executionResult.ResultingState.GetOpenedNodeLocksExceptIn(initialInGameState).Values).ToDictionary(nodeLock => nodeLock.Name);

            // Initialize item locations taken
            ItemLocationsTaken = ItemLocationsTaken.Concat(executionResult.ResultingState.GetTakenItemLocationsExceptIn(initialInGameState).Values).ToList();

            // Initialize resources before and after
            ResourcesBefore = initialInGameState.Resources.Clone();
            ResourcesAfter = executionResult.ResultingState.Resources.Clone();

            // Initialize destroyed obstacles, but that's only relevant if we didn't change rooms
            if (executionResult.ResultingState.CurrentRoom == initialInGameState.CurrentRoom)
            {
                ObstaclesDestroyed = ObstaclesDestroyed.Concat(
                    executionResult.ResultingState.GetDestroyedObstacleIds()
                        .Except(initialInGameState.GetDestroyedObstacleIds())
                        .Select(obstacleId => executionResult.ResultingState.CurrentRoom.Obstacles[obstacleId])
                    ).ToList();
            }

            // Transfer information data from the ExecutionResult.
            RunwaysUsed = executionResult.RunwaysUsed.Values.ToDictionary(pair => pair.runwayUsed.Name);
            CanLeaveChargedExecuted = executionResult.CanLeaveChargedExecuted.ToList();
            OpenedLocks = executionResult.OpenedLocks.Values.ToDictionary(pair => pair.openedLock.Name);
            BypassedLocks = executionResult.BypassedLocks.Values.ToDictionary(pair => pair.bypassedLock.Name);
            KilledEnemies = executionResult.KilledEnemies.ToList();

            ItemsInvolved = new Dictionary<string, Item>(executionResult.ItemsInvolved);
            DamageReducingItemsInvolved = new Dictionary<string, Item> (executionResult.DamageReducingItemsInvolved);
        }

        /// <summary>
        /// A description of what was being attempted which resulted in this action.
        /// </summary>
        public string IntentDescription { get; protected set; } = "";

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
        public ReadOnlyItemInventory ItemsGained { get; protected set; }

        /// <summary>
        /// The inventory of items that have been lost by the player as a result of this action.
        /// This can only really happen by reversing an action.
        /// </summary>
        public ReadOnlyItemInventory ItemsLost { get; protected set; }

        /// <summary>
        /// The names of items that have been disabled by the player as a result of this action.
        /// </summary>
        public IReadOnlySet<string> ItemsDisabledNames { get; protected set; } = new HashSet<string>();

        /// <summary>
        /// The names of items that have been enabled by the player as a result of this action.
        /// </summary>
        public IReadOnlySet<string> ItemsEnabledNames { get; protected set; } = new HashSet<string>();

        /// <summary>
        /// The game flags that have been obtained by the player as a result of this action, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, GameFlag> GameFlagsGained { get; protected set; } = new Dictionary<string, GameFlag>();

        /// <summary>
        /// The game flags that have been obtained by the player as a result of this action, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, GameFlag> GameFlagsLost { get; protected set; } = new Dictionary<string, GameFlag>();

        /// <summary>
        /// The node locks that have been opened as a result of this action, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, NodeLock> LocksOpened { get; protected set; } = new Dictionary<string, NodeLock>();

        /// <summary>
        /// The node locks that have been closed as a result of this action, mapped by name.
        /// This can only really happen by reversing an action.
        /// </summary>
        public IReadOnlyDictionary<string, NodeLock> LocksClosed { get; protected set; } = new Dictionary<string, NodeLock>();

        /// <summary>
        /// The enumeration of item locations whose item has been taken by the player as a result of this action.
        /// </summary>
        public IReadOnlyCollection<RoomNode> ItemLocationsTaken { get; protected set; } = new List<RoomNode>();

        /// <summary>
        /// The enumeration of item locations whose item has been put back where it was as a result of this action.
        /// </summary>
        public IReadOnlyCollection<RoomNode> ItemLocationsPutBack { get; protected set; } = new List<RoomNode>();

        /// <summary>
        /// <para>The enumeration of in-room obstacles that have been destroyed as a result of this action.</para>
        /// <para>Note that changes to obstacles are not kept when changing rooms.</para>
        /// </summary>
        public IReadOnlyCollection<RoomObstacle> ObstaclesDestroyed { get; protected set; } = new List<RoomObstacle>();

        /// <summary>
        /// <para>The enumeration of in-room obstacles that have been restored as a result of this action.
        /// This can only really happen by reversing an action, since it's deemed unnecessary to indicate that exiting a room restores obstacles.</para>
        /// <para>Note that changes to obstacles are not kept when changing rooms.</para>
        /// </summary>
        public IReadOnlyCollection<RoomObstacle> ObstaclesRestored { get; protected set; } = new List<RoomObstacle>();

        /// <summary>
        /// A ResourceCount representing the player's resources before this action.
        /// </summary>
        public ReadOnlyResourceCount ResourcesBefore { get; protected set; }

        /// <summary>
        /// A ResourceCount representing the player's resources after this action.
        /// </summary>
        public ReadOnlyResourceCount ResourcesAfter { get; protected set; }
        #endregion

        #region Information about how the action was performed
        /// <summary>
        /// The runways that were used (possibly retroactively) along with the accompanying runway strat, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, (Runway runwayUsed, Strat stratUsed)> RunwaysUsed { get; protected set; } = new Dictionary<string, (Runway, Strat)>();

        /// <summary>
        /// A sequence of canLeaveCharged that were executed (possibly retroactively) along with the accompanying canLeaveCharged strat.
        /// </summary>
        public IReadOnlyCollection<(CanLeaveCharged canLeaveChargedUsed, Strat stratUsed)> CanLeaveChargedExecuted { get; protected set; } = new List<(CanLeaveCharged, Strat)>();

        /// <summary>
        /// The node locks that were opened along with the open strat used to opem them, mapped by lock name.
        /// </summary>
        public IReadOnlyDictionary<string, (NodeLock openedLock, Strat stratUsed)> OpenedLocks { get; protected set; } = new Dictionary<string, (NodeLock openedLock, Strat stratUsed)>();

        /// <summary>
        /// The node locks that were bypassed along with the bypass strat used to opem them, mapped by lock name.
        /// </summary>
        public IReadOnlyDictionary<string, (NodeLock bypassedLock, Strat stratUsed)> BypassedLocks { get; protected set; } = new Dictionary<string, (NodeLock bypassedLock, Strat stratUsed)>();

        /// <summary>
        /// A sequence of enemies that were killed, along with the weapon and number of shots used.
        /// </summary>
        public IReadOnlyCollection<IndividualEnemyKillResult> KilledEnemies { get; protected set; } = new List<IndividualEnemyKillResult>();

        /// <summary>
        /// The items that were involved in some way, mapped by name,
        /// excluding damage reduction (found in <see cref="DamageReducingItemsInvolved"/>) and operation of weapons to kill enemies (found in <see cref="KilledEnemies"/>).
        /// </summary>
        public IReadOnlyDictionary<string, Item> ItemsInvolved { get; protected set; } = new Dictionary<string, Item>();

        /// <summary>
        /// A sequence of items that were involved in reducing incoming damage, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Item> DamageReducingItemsInvolved { get; protected set; } = new Dictionary<string, Item>();
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
        /// Outputs to console a summary of this action, based on parameters.
        /// Can independently output succes, failure, effects of the action, and details of the execution.
        /// This method also initially outputs a preamble explaining the attempted action, but only if at least something else will be outputted.
        /// </summary>
        /// <param name="outputEffects">If true, will output how the action impacted the game state.</param>
        /// <param name="outputDetails">If true, will output details about how the action was performed.</param>
        /// <param name="outputSuccess">If true, will output an indication if the action succeeded.</param>
        /// <param name="outputFailure">If true, will output an indication if the action failed.</param>
        public void OutputToConsole(bool outputEffects, bool outputDetails, bool outputSuccess = true, bool outputFailure = true)
        {
            string actionAttemptedOutput = $"Action attempted: {IntentDescription}";

            // Exit fast in case of failure
            if (!Succeeded)
            {
                if (outputFailure)
                {
                    OutputExecutionPreamble();
                    Console.WriteLine("** Action failed **");
                }
                return;
            }

            // Action succeeded, proceed as normal
            if(outputSuccess || outputDetails || outputEffects)
            {
                OutputExecutionPreamble();
            }

            if (outputSuccess)
            {
                if(IsReverseAction)
                {
                    Console.WriteLine("Action reversal executed");
                }
                else
                {
                    Console.WriteLine(GetSuccessOutputString());
                }
            }

            if (outputDetails)
            {
                foreach(var (runway, strat) in RunwaysUsed.Values)
                {
                    Console.WriteLine($"Runway '{runway.Name}' was used, by executing strat '{strat.Name}'");
                }

                foreach(var (canLeaveCharged, strat) in CanLeaveChargedExecuted)
                {
                    Console.WriteLine($"A canLeaveCharged on node '{canLeaveCharged.Node.Name}' was used, by executing strat '{strat.Name}'");
                }

                foreach(var (nodeLock, strat) in OpenedLocks.Values)
                {
                    Console.WriteLine($"Lock '{nodeLock.Name}' was opened, by executing strat '{strat.Name}'");
                }

                foreach(var (nodeLock, strat) in BypassedLocks.Values)
                {
                    Console.WriteLine($"Lock '{nodeLock.Name}' was bypassed, by executing strat '{strat.Name}'");
                }

                foreach(var killResult in KilledEnemies)
                {
                    string killMethodString = String.Join(" and ", killResult.KillMethod.Select(method => $"{method.shots} shots of weapon {method.weapon.Name}"));
                    Console.WriteLine($"Enemy '{killResult.Enemy.Name}' was killed, using {killMethodString}.");
                }

                foreach(var item in ItemsInvolved.Values)
                {
                    Console.WriteLine($"Item '{item.Name}' was used during execution.");
                }

                foreach(var item in DamageReducingItemsInvolved.Values)
                {
                    Console.WriteLine($"Item '{item.Name}' helped reduce incoming damage.");
                }
            }

            if (outputEffects)
            {
                // Items gained and lost
                foreach (Item item in ItemsGained.NonConsumableItems.Values)
                {
                    Console.WriteLine($"Gained item '{item.Name}'");
                }
                foreach (var (item, count) in ItemsGained.ExpansionItems.Values)
                {
                    Console.WriteLine($"Gained item '{item.Name}' X {count}");
                }

                foreach (Item item in ItemsLost.NonConsumableItems.Values)
                {
                    Console.WriteLine($"Lost item '{item.Name}'");
                }
                foreach (var (item, count) in ItemsLost.ExpansionItems.Values)
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
                foreach (GameFlag flag in GameFlagsGained.Values)
                {
                    Console.WriteLine($"Gained game flag '{flag.Name}'");
                }

                foreach (GameFlag flag in GameFlagsLost.Values)
                {
                    Console.WriteLine($"Lost game flag '{flag.Name}'");
                }

                // Locks opened and closed
                foreach (NodeLock nodeLock in LocksOpened.Values)
                {
                    Console.WriteLine($"Opened lock '{nodeLock.Name}'");
                }

                foreach (NodeLock nodeLock in LocksClosed.Values)
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

        /// <summary>
        /// Outputs the preamble of an action execution's output, including an empty line as a spacer.
        /// Should only be called if at least one other portion of the action output will be written out.
        /// </summary>
        protected void OutputExecutionPreamble()
        {
            Console.WriteLine("");
            Console.WriteLine($"Action attempted: {IntentDescription}");
        }

        public virtual string GetSuccessOutputString()
        {
            return "Action succeeded";
        }
    }
}
