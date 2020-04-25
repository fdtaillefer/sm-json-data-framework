using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class NodeLock : InitializablePostDeserializeInNode
    {
        public LockTypeEnum LockType { get; set; }

        public LogicalRequirements Lock { get; set; } = new LogicalRequirements();

        public string Name { get; set; }

        public IEnumerable<Strat> UnlockStrats { get; set; } = Enumerable.Empty<Strat>();

        public IEnumerable<Strat> BypassStrats { get; set; } = Enumerable.Empty<Strat>();

        /// <summary>
        /// <para>Not available before <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room, RoomNode)"/> has been called.</para>
        /// <para>The RoomNode on which this lock is.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode Node { get; set; }

        public void Initialize(SuperMetroidModel model, Room room, RoomNode node)
        {
            Node = node;

            model.Locks.Add(Name, this);

            // Eliminate disabled unlock strats
            UnlockStrats = UnlockStrats.WhereEnabled(model);

            foreach (Strat strat in UnlockStrats)
            {
                strat.Initialize(model, room);
            }

            // Eliminate disabled bypass strats
            BypassStrats = BypassStrats.WhereEnabled(model);
            foreach (Strat strat in BypassStrats)
            {
                strat.Initialize(model, room);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Lock.InitializeReferencedLogicalElementProperties(model, room));

            foreach(Strat strat in UnlockStrats)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            foreach (Strat strat in BypassStrats)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }

        /// <summary>
        /// Returns specifically whether this lock has been opened. If it's not yet active, this will return false.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <returns></returns>
        public bool IsOpen(SuperMetroidModel model, InGameState inGameState)
        {
            return inGameState.IsLockOpen(this);
        }

        /// <summary>
        /// Returns whether this lock is currently active.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <returns></returns>
        public bool IsActive(SuperMetroidModel model, InGameState inGameState)
        {
            // This lock cannot be active if it's been opened
            if (inGameState.IsLockOpen(this))
            {
                return false;
            }

            // The lock isn't open, but it's only active if its activation conditions are met.
            // The resulting game state has no value because locks are activated passively.
            return Lock.AttemptFulfill(model, inGameState) != null;
        }

        /// <summary>
        /// Attemps to execute a bypass of this lock without opening it, and returns a new InGameState describing the state after success (or null).
        /// Does not actually check whether the lock is active, strictly attempts to execute a bypass strat.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="times">The number of consecutive times that this should be fulfilled. Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// <returns>A new InGameState describing the state after succeeding, or null in case of failure</returns>
        public InGameState AttemptBypass(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            // Try to find the cheapest doable bypass strat and return the result of that
            return model.ApplyOr(inGameState, BypassStrats, (s, igs) => s.AttemptFulfill(model, igs, times: times, usePreviousRoom: usePreviousRoom));
        }

        /// <summary>
        /// Attempts to execute the opening of this lock, and returns a new InGameState describing the state after success (or null).
        /// If the lock is not currently active, this will be considered a failure.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="times">The number of consecutive times that this should be fulfilled. Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// <returns>A new InGameState describing the state after succeeding, or null in case of failure</returns>
        public InGameState AttemptOpen(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            // Can't open a lock that isn't active
            if (!IsActive(model, inGameState))
            {
                return null;
            }

            // Try to find the cheapest doable unlock strat and return the result of that
            InGameState resultingState = model.ApplyOr(inGameState, UnlockStrats, (s, igs) => s.AttemptFulfill(model, igs, times: times, usePreviousRoom: usePreviousRoom));
            // If lock was successfully opened, alter the resulting state
            if(resultingState != null)
            {
                resultingState.ApplyOpenLock(this);
            }
            return resultingState;
        }
    }
}
