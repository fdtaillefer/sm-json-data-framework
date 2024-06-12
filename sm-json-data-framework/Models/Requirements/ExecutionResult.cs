using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
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
        public InGameState ResultingState { get; protected set; }

        /// <summary>
        /// The runways that were used (possibly retroactively) along with the accompanying runway strat, mapped by name.
        /// </summary>
        public IDictionary<string, (Runway runwayUsed, Strat stratUsed)> RunwaysUsed { get; protected set; } = new Dictionary<string, (Runway, Strat)>();

        /// <summary>
        /// A sequence of canLeaveCharged that were executed (possibly retroactively) along with the accompanying canLeaveCharged strat.
        /// </summary>
        public ICollection<(CanLeaveCharged canLeaveChargedUsed, Strat stratUsed)> CanLeaveChargedExecuted { get; protected set; } = new List<(CanLeaveCharged, Strat)>();

        /// <summary>
        /// The node locks that were opened along with the open strat used to opem them, mapped by lock name.
        /// </summary>
        public IDictionary<string, (NodeLock openedLock, Strat stratUsed)> OpenedLocks { get; protected set; } = new Dictionary<string, (NodeLock openedLock, Strat stratUsed)>();

        /// <summary>
        /// A sequence of node locks that were bypassed along with the bypass strat used to opem them, mapped by lock name.
        /// </summary>
        public IDictionary<string, (NodeLock bypassedLock, Strat stratUsed)> BypassedLocks { get; protected set; } = new Dictionary<string, (NodeLock bypassedLock, Strat stratUsed)>();

        /// <summary>
        /// A sequence of game flags that were activated, mapped by name.
        /// </summary>
        public IDictionary<string, GameFlag> ActivatedGameFlags { get; protected set; } = new Dictionary<string, GameFlag>();

        /// <summary>
        /// A sequence of enemies that were killed, along with the weapon and number of shots used.
        /// </summary>
        public ICollection<IndividualEnemyKillResult> KilledEnemies { get; protected set; } = new List<IndividualEnemyKillResult>();

        /// <summary>
        /// A sequence of items that were involved in some way, mapped by name. 
        /// This excludes items needed to operate weapons described in <see cref="KilledEnemies"/>.
        /// This also excludes items whose only contribution was reducing damage (they are in <see cref="DamageReducingItemsInvolved"/>).
        /// </summary>
        public IDictionary<string, Item> ItemsInvolved { get; protected set; } = new Dictionary<string, Item>();

        /// <summary>
        /// A sequence of items that were involved in reducing incoming damage, mapped by name.
        /// </summary>
        public IDictionary<string, Item> DamageReducingItemsInvolved { get; protected set; } = new Dictionary<string, Item>();

        /// <summary>
        /// Given the result of an execution done on this result's resulting state, updates this result to represent
        /// the result of both executions done back-to-back. Then, returns itself.
        /// </summary>
        /// <param name="subsequentResult">The result of a subsequent execution, done on this result's resultingState.</param>
        /// <returns>This ExecutionResult</returns>
        public ExecutionResult ApplySubsequentResult(ExecutionResult subsequentResult)
        {
            ResultingState = subsequentResult.ResultingState;
            RunwaysUsed = RunwaysUsed.Values.Concat(subsequentResult.RunwaysUsed.Values).ToDictionary(pair => pair.runwayUsed.Name);
            CanLeaveChargedExecuted = CanLeaveChargedExecuted.Concat(subsequentResult.CanLeaveChargedExecuted).ToList();
            OpenedLocks = OpenedLocks.Values.Concat(subsequentResult.OpenedLocks.Values)
                .ToDictionary(pair => pair.openedLock.Name);
            BypassedLocks = BypassedLocks.Values.Concat(subsequentResult.BypassedLocks.Values)
                .ToDictionary(pair => pair.bypassedLock.Name);
            ActivatedGameFlags = ActivatedGameFlags.Values.Concat(subsequentResult.ActivatedGameFlags.Values)
                .Distinct(ObjectReferenceEqualityComparer<GameFlag>.Default)
                .ToDictionary(flag => flag.Name);
            KilledEnemies = KilledEnemies.Concat(subsequentResult.KilledEnemies).ToList();
            ItemsInvolved = ItemsInvolved.Values.Concat(subsequentResult.ItemsInvolved.Values)
                .Distinct(ObjectReferenceEqualityComparer<Item>.Default)
                .ToDictionary(item => item.Name);
            DamageReducingItemsInvolved = DamageReducingItemsInvolved.Values.Concat(subsequentResult.DamageReducingItemsInvolved.Values)
                .Distinct(ObjectReferenceEqualityComparer<Item>.Default)
                .ToDictionary(item => item.Name);

            return this;
        }

        /// <summary>
        /// Applies the destruction of the provided obstacles both in this ExecutionResult's resulting InGameState.
        /// </summary>
        /// <param name="obstacles">The obstacles to mark as destroyed.</param>
        /// <param name="usePreviousRoom">Indicates whether the obstacles were destroyed in the context of the previous room.</param>
        /// <param name="previousRoomCount">Indicates in which previous playable room to destroy the obstacles, if any.
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        public void ApplyDestroyObstacles(IEnumerable<RoomObstacle> obstacles, int previousRoomCount = 0)
        {
            // While we can retroactively do some things in previous rooms, we will not retroactively alter the room state.
            if (previousRoomCount == 0)
            {
                foreach (var obstacle in obstacles)
                {
                    ResultingState.ApplyDestroyObstacle(obstacle);
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
            OpenedLocks = OpenedLocks.Values.Append((openedLock: nodeLock, stratUsed: strat)).ToDictionary(pair => pair.openedLock.Name);
            ResultingState.ApplyOpenLock(nodeLock);
        }

        /// <summary>
        /// Applies the bypass of the provided nodeLock both in this ExecutionResult and in its resulting InGameState.
        /// </summary>
        /// <param name="nodeLock">The lock to bypass</param>
        /// <param name="strat">The strat used to bypass the lock</param>
        public void ApplyBypassedLock(NodeLock nodeLock, Strat strat)
        {
            BypassedLocks = BypassedLocks.Values.Append((bypassedLock: nodeLock, stratUsed: strat)).ToDictionary(pair => pair.bypassedLock.Name);
            ResultingState.ApplyBypassLock(nodeLock);
        }

        /// <summary>
        /// Applies the activation of the provided gameFlag both in this ExecutionResult and in its resulting InGameState.
        /// </summary>
        /// <param name="gameFlag"></param>
        public void ApplyActivatedGameFlag(GameFlag gameFlag)
        {
            if (!ResultingState.ActiveGameFlags.ContainsFlag(gameFlag))
            {
                ActivatedGameFlags = ActivatedGameFlags.Values.Append(gameFlag).ToDictionary(gameFlag => gameFlag.Name);
                ResultingState.ApplyAddGameFlag(gameFlag);
            }
        }

        /// <summary>
        /// Adds the to this ExecutionResult a record of using the provided strat to use the provided runway.
        /// </summary>
        /// <param name="runway">The used runway</param>
        /// <param name="strat">The strat used on that runway</param>
        public void AddUsedRunway(Runway runway, Strat strat)
        {
            RunwaysUsed = RunwaysUsed.Values.Append((runwayUsed: runway, stratUsed: strat)).ToDictionary(pair => pair.runwayUsed.Name);
        }

        /// <summary>
        /// Adds the to this ExecutionResult a record of using the provided strat to execute the provided canLeaveCharged.
        /// </summary>
        /// <param name="canLeaveCharged">The executed canLeaveCharged</param>
        /// <param name="strat">The strat used to execute that canLeaveCharged</param>
        public void AddExecutedCanLeaveCharged(CanLeaveCharged canLeaveCharged, Strat strat)
        {
            CanLeaveChargedExecuted = CanLeaveChargedExecuted.Append((canLeaveCharged, strat)).ToList();
        }

        /// <summary>
        /// Adds to this ExecutionResult a record of using the provided number of shots of the provided weapon
        /// to kill the provided enemy.
        /// </summary>
        /// <param name="enemy">The killed enemy</param>
        /// <param name="weapon">The weapon that was used to kill</param>
        /// <param name="shots">The number of shots the kill took</param>
        public void AddKilledEnemy(Enemy enemy, Weapon weapon, int shots)
        {
            KilledEnemies = KilledEnemies.Append(new IndividualEnemyKillResult(enemy, new[] { (weapon, shots) })).ToList();
        }

        /// <summary>
        /// Adds to this ExecutionResult a record of using the provided combination of weapons and shots
        /// to kill the provided enemy.
        /// </summary>
        /// <param name="enemy">The killed enemy</param>
        /// <param name="killMethod">An enumeration of weapons alongside their number of shots fired</param>
        public void AddKilledEnemy(Enemy enemy, IEnumerable<(Weapon weapon, int shots)> killMethod)
        {
            KilledEnemies = KilledEnemies.Append(new IndividualEnemyKillResult(enemy, killMethod)).ToList();
        }

        /// <summary>
        /// Adds to this ExecutionResult a record of getting execution some benefit out of having the provided item.
        /// This should exclude damage reduction and the operation of weapons recorded in <see cref="KilledEnemies"/>.
        /// </summary>
        /// <param name="items">The items</param>
        public void AddItemInvolved(Item item)
        {
            ItemsInvolved = ItemsInvolved.Values.Append(item).ToDictionary(item => item.Name);
        }

        /// <summary>
        /// Adds to this ExecutionResult a record of getting execution some benefit out of having the provided items.
        /// This should exclude damage reduction and the operation of weapons recorded in <see cref="KilledEnemies"/>.
        /// </summary>
        /// <param name="items">The items</param>
        public void AddItemsInvolved(IEnumerable<Item> items)
        {
            ItemsInvolved = ItemsInvolved.Values.Concat(items).ToDictionary(item => item.Name);
        }

        /// <summary>
        /// Adds to this ExecutionResult a record of taking less damage due to having the provided items.
        /// </summary>
        /// <param name="items">The items</param>
        public void AddDamageReducingItemsInvolved(IEnumerable<Item> items)
        {
            DamageReducingItemsInvolved = DamageReducingItemsInvolved.Values.Concat(items).ToDictionary(item => item.Name);
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
        /// <param name="previousRoomCount">The number of playable rooms to go back by (whenever in-room state is relevant). 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns>The new state of this if execution succeeds, or null in case of failure.</returns>
        public ExecutionResult AndThen(IExecutable executable, SuperMetroidModel model, int times = 1, int previousRoomCount = 0)
        {
            ExecutionResult result = executable.Execute(model, ResultingState, times: times, previousRoomCount: previousRoomCount);
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

        public override bool Equals(object obj)
        {
            if(obj is IndividualEnemyKillResult result)
            {
                if(Enemy != result.Enemy || KillMethod.Count() != result.KillMethod.Count())
                {
                    return false;
                }

                List<(Weapon weapon, int shots)> otherKillMethod = new List<(Weapon weapon, int shots)>(result.KillMethod);
                foreach ((Weapon weapon, int shots) in KillMethod)
                {
                    int index = otherKillMethod.FindIndex(method => method.weapon == weapon && method.shots == shots);
                    if (index == -1)
                    {
                        return false;
                    }
                    otherKillMethod.RemoveAt(index);
                }

                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Enemy);
            foreach ((Weapon weapon, int shots) in KillMethod)
            {
                hash.Add(weapon);
                hash.Add(shots);
            }
            return hash.ToHashCode();
        }
    }
}
