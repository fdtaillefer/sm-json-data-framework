using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Node;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace sm_json_data_framework.Models.Requirements
{
    /// <summary>
    /// A model that contains the result of executing an IExecutable.
    /// </summary>
    public class ExecutionResult
    {
        public ExecutionResult(InGameState resultingState)
        {
            ResultingState = resultingState;
        }

        /// <summary>
        /// Returns a shallow copy of this ExecutionResult. There is no need for a deep copy since an ExecutionResult
        /// references its contents but does not own it.
        /// </summary>
        /// <returns></returns>
        public ExecutionResult Clone()
        {
            ExecutionResult clone = new ExecutionResult(ResultingState);
            clone.ApplySubsequentResult(this);

            return clone;
        }

        /// <summary>
        /// The in-game state that resulted from the execution.
        /// </summary>
        public InGameState ResultingState { get; set; }

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
        /// A sequence of items that were involved in some way. 
        /// This excludes items needed to operate weapons described in <see cref="KilledEnemies"/>.
        /// This also excludes items whose only contribution was reducing damage (they are in <see cref="DamageReducingItemsInvolved"/>).
        /// </summary>
        public ISet<Item> ItemsInvolved { get; set; } = new HashSet<Item>(ObjectReferenceEqualityComparer<Item>.Default);

        /// <summary>
        /// A sequence of items that were involved in reducing incoming damage.
        /// </summary>
        public ISet<Item> DamageReducingItemsInvolved { get; set; } = new HashSet<Item>(ObjectReferenceEqualityComparer<Item>.Default);

        /// <summary>
        /// Given the result of an execution done on this result's resulting state, updates this result to represent
        /// the result of both executions done back-to-back. Then, returns itself.
        /// </summary>
        /// <param name="subsequentResult">The result of a subsequent execution, done on this result's resultingState.</param>
        /// <returns>This ExecutionResult</returns>
        public ExecutionResult ApplySubsequentResult(ExecutionResult subsequentResult)
        {
            ResultingState = subsequentResult.ResultingState;
            RunwaysUsed = RunwaysUsed.Concat(subsequentResult.RunwaysUsed);
            CanLeaveChargedExecuted = CanLeaveChargedExecuted.Concat(subsequentResult.CanLeaveChargedExecuted);
            OpenedLocks = OpenedLocks.Concat(subsequentResult.OpenedLocks);
            BypassedLocks = BypassedLocks.Concat(subsequentResult.BypassedLocks);
            KilledEnemies = KilledEnemies.Concat(subsequentResult.KilledEnemies);
            ItemsInvolved.UnionWith(subsequentResult.ItemsInvolved);
            DamageReducingItemsInvolved.UnionWith(subsequentResult.DamageReducingItemsInvolved);

            return this;
        }

        /// <summary>
        /// Applies the destruction of the provided obstacles both in this ExecutionResult's resulting InGameState.
        /// </summary>
        /// <param name="obstacles">The obstacles to mark as destroyed.</param>
        /// <param name="usePreviousRoom">Indicates whether the obstacles were destroyed in the context of the previous room.</param>
        public void ApplyDestroyedObstacles(IEnumerable<RoomObstacle> obstacles, bool usePreviousRoom)
        {
            // While we can retroactively do some things in previous rooms, we will not retroactively alter the room state.
            if (!usePreviousRoom)
            {
                foreach(var obstacle in obstacles)
                {
                    ResultingState.ApplyDestroyedObstacle(obstacle);
                }
            }
        }

        /// <summary>
        /// Applies the opening of the provided nodeLock both in this ExecutionResult and in its resulting InGameState.
        /// </summary>
        /// <param name="nodeLock">The lock to mark as open</param>
        /// <param name="strat">The strat used to open the lock</param>
        public void ApplyOpenedLock(NodeLock nodeLock, Strat strat)
        {
            OpenedLocks = OpenedLocks.Append((nodeLock, strat));
            ResultingState.ApplyOpenLock(nodeLock);
        }

        /// <summary>
        /// Adds to this ExecutionResult a record of using the provided strat to bypass the provided lock.
        /// </summary>
        /// <param name="nodeLock">The lock that was bypassed</param>
        /// <param name="strat">The strat used to bypass the lock</param>
        public void AddBypassedLock(NodeLock nodeLock, Strat strat)
        {
            BypassedLocks = BypassedLocks.Append((nodeLock, strat));
        }

        /// <summary>
        /// Adds the to this ExecutionResult a record of using the provided strat to use the provided runway.
        /// </summary>
        /// <param name="runway">The used runway</param>
        /// <param name="strat">The strat used on that runway</param>
        public void AddUsedRunway(Runway runway, Strat strat)
        {
            RunwaysUsed = RunwaysUsed.Append((runway, strat));
        }

        /// <summary>
        /// Adds the to this ExecutionResult a record of using the provided strat to execute the provided canLeaveCharged.
        /// </summary>
        /// <param name="canLeaveCharged">The executed canLeaveCharged</param>
        /// <param name="strat">The strat used to execute that canLeaveCharged</param>
        public void AddExecutedCanLeaveCharged(CanLeaveCharged canLeaveCharged, Strat strat)
        {
            CanLeaveChargedExecuted = CanLeaveChargedExecuted.Append((canLeaveCharged, strat));
        }

        /// <summary>
        /// Adds to this ExecutionResult a record of using the provided number of shots of the provided weapon
        /// to kill the provided enemy.
        /// </summary>
        /// <param name="enemy">The killed enemy</param>
        /// <param name="weapon">The weapon that was used to kill</param>
        /// <param name="shots">The number of shots the kill took</param>
        public void ApplyKilledEnemy(Enemy enemy, Weapon weapon, int shots)
        {
            KilledEnemies = KilledEnemies.Append(new IndividualEnemyKillResult(enemy, new[] { (weapon, shots) }));
        }

        /// <summary>
        /// Adds to this ExecutionResult a record of using the provided combination of weapons and shots
        /// to kill the provided enemy.
        /// </summary>
        /// <param name="enemy">The killed enemy</param>
        /// <param name="killMethod">An enumeration of weapons alongside their number of shots fired</param>
        public void ApplyKilledEnemy(Enemy enemy, IEnumerable<(Weapon weapon, int shots)> killMethod)
        {
            KilledEnemies = KilledEnemies.Append(new IndividualEnemyKillResult(enemy, killMethod));
        }

        /// <summary>
        /// Adds to this ExecutionResult a record of getting execution some benefit out of having the provided items.
        /// This should exclude damage reduction and the operation of weapons recorded in <see cref="KilledEnemies"/>.
        /// </summary>
        /// <param name="items">The items</param>
        public void ApplyItemsInvolved(IEnumerable<Item> items)
        {
            ItemsInvolved.UnionWith(items);
        }

        /// <summary>
        /// Adds to this ExecutionResult a record of taking less damage due to having the provided items.
        /// </summary>
        /// <param name="items">The items</param>
        public void ApplyDamageReducingItemsInvolved(IEnumerable<Item> items)
        {
            DamageReducingItemsInvolved.UnionWith(items);
        }

        /// <summary>
        /// <para>Attempts to execute the provided IExecutable on top of this ExecutionResult.
        /// If successful, applies the result on top of the current state and returns this.
        /// If not successful, returns null.</para>
        /// <para>Be aware that this method may alter the state of this execution result.</para>
        /// </summary>
        /// <param name="executable">The IExecutable to execute</param>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="times">The number of consecutive times that this should be executed.
        /// Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// (whenever in-room state is relevant).</param>
        /// <returns>The new state of this if execution succeeds, or null in case of failure.</returns>
        public ExecutionResult AndThen(IExecutable executable, SuperMetroidModel model, int times = 1, bool usePreviousRoom = false)
        {
            ExecutionResult result = executable.Execute(model, ResultingState, times: times, usePreviousRoom: usePreviousRoom);
            if (result == null)
            {
                return null;
            }
            else
            {
                ApplySubsequentResult(result);
                return this;
            }
        }
    }

    /// <summary>
    /// Expresses the killing of an enemy, along with the combination of weapons used (with the number of shots per weapon).
    /// </summary>
    public class IndividualEnemyKillResult
    {
        public IndividualEnemyKillResult(Enemy enemy, IEnumerable<(Weapon weapon, int shots)> killMethod)
        {
            Enemy = enemy;
            KillMethod = killMethod;
        }

        public Enemy Enemy { get; private set; }

        public IEnumerable<(Weapon weapon, int shots)> KillMethod { get; private set; }
    }
}
