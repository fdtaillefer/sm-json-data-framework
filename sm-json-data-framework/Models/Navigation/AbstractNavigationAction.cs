using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Node;
using sm_json_data_framework.Models.Rooms.Nodes;
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
        protected AbstractNavigationAction()
        {

        }

        public AbstractNavigationAction(SuperMetroidModel model, InGameState initialInGameState, ExecutionResult executionResult)
        {
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

            // Initialize flags gained
            GameFlagsGained = GameFlagsGained.Concat(executionResult.ResultingState.GetActiveGameFlagsExceptWith(initialInGameState).Values);

            // Initialize locks opened
            LocksOpened = LocksOpened.Concat(executionResult.ResultingState.GetOpenedNodeLocksExceptWith(initialInGameState).Values);

            // Initialize resource variation
            ResourceVariation = executionResult.ResultingState.GetResourceVariationWith(initialInGameState);

            // Initialize destroyed obstacles, but that's only relevant if we didn't change rooms
            if(executionResult.ResultingState.GetCurrentRoom() == initialInGameState.GetCurrentRoom())
            {
                DestroyedObstacles = DestroyedObstacles.Concat(
                    executionResult.ResultingState.GetDestroyedObstacleIds()
                        .Except(initialInGameState.GetDestroyedObstacleIds())
                        .Select(obstacleId => executionResult.ResultingState.GetCurrentRoom().Obstacles[obstacleId])
                    );
            }

            // Transfer information data from the ExecutionResult.
            // No need to copy since they are IEnumerable and not supposed to be mutated.
            RunwaysUsed = executionResult.RunwaysUsed;
            CanLeaveChargedExecuted = executionResult.CanLeaveChargedExecuted;
            DestroyedObstacles = executionResult.DestroyedObstacles;
            OpenedLocks = executionResult.OpenedLocks;
            BypassedLocks = executionResult.BypassedLocks;
            KilledEnemies = executionResult.KilledEnemies;
        }

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
        /// The enumeration of game flags that have been obtained by the player as a result of this action.
        /// </summary>
        public IEnumerable<GameFlag> GameFlagsGained { get; protected set; } = Enumerable.Empty<GameFlag>();

        /// <summary>
        /// The enumeration of game flags that have been obtained by the player as a result of this action.
        /// </summary>
        public IEnumerable<GameFlag> GameFlagsLost { get; protected set; } = Enumerable.Empty<GameFlag>();

        /// <summary>
        /// The enumeration of node  locks that have been opened as a result of this action.
        /// </summary>
        public IEnumerable<NodeLock> LocksOpened { get; protected set; } = Enumerable.Empty<NodeLock>();

        /// <summary>
        /// The enumeration of node  locks that have been closed as a result of this action.
        /// This can only really happen by reversing an action.
        /// </summary>
        public IEnumerable<NodeLock> LocksClosed { get; protected set; } = Enumerable.Empty<NodeLock>();

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
        /// A ResourceCount representing the variation of each resource type that happened as a result of this action.
        /// </summary>
        public ResourceCount ResourceVariation { get; set; }
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
        /// A sequence of room obstacles that were destroyed.
        /// </summary>
        public IEnumerable<RoomObstacle> DestroyedObstacles { get; set; } = Enumerable.Empty<RoomObstacle>();

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

            // Initialize reversed flags change
            // Don't clone, expect that IEnumerables won't get mutated.
            reverseAction.GameFlagsGained = GameFlagsLost;
            reverseAction.GameFlagsLost = GameFlagsGained;

            // Initialize reversed locks change
            // Don't clone, expect that IEnumerables won't get mutated.
            reverseAction.LocksOpened = LocksClosed;
            reverseAction.LocksClosed = LocksOpened;

            // Initialize reversed resource variation
            reverseAction.ResourceVariation = ResourceVariation.CloneNegative();

            // Initialize reversed obstacles change
            // Don't clone, expect that IEnumerables won't get mutated.
            reverseAction.ObstaclesDestroyed = ObstaclesRestored;
            reverseAction.ObstaclesRestored = ObstaclesDestroyed;

            // Don't transfer information data. It makes little sense for a reverse action
        }
    }
}
