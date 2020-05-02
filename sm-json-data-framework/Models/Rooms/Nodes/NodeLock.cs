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
            return Lock.Execute(model, inGameState) != null;
        }

        IExecutable _openExecution = null;
        /// <summary>
        /// An IExecutable that corresponds to opening this lock.
        /// </summary>
        public IExecutable OpenExecution
        {
            get
            {
                if (_openExecution == null)
                {
                    _openExecution = new OpenExecution(this);
                }
                return _openExecution;
            }
        }

        IExecutable _bypassExecution = null;
        /// <summary>
        /// An IExecutable that corresponds to bypassing this lock.
        /// </summary>
        public IExecutable BypassExecution
        {
            get
            {
                if (_bypassExecution == null)
                {
                    _bypassExecution = new BypassExecution(this);
                }
                return _bypassExecution;
            }
        }
    }

    /// <summary>
    /// A class that encloses the opening of a NodeLock in an IExecutable interface.
    /// </summary>
    internal class OpenExecution : IExecutable
    {
        private NodeLock NodeLock { get; set; }

        public OpenExecution(NodeLock nodeLock)
        {
            NodeLock = nodeLock;
        }

        public ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            // Can't open a lock that isn't active
            if (!NodeLock.IsActive(model, inGameState))
            {
                return null;
            }

            // Look for the best unlock strat
            (Strat bestStrat, ExecutionResult result) = model.ExecuteBest(NodeLock.UnlockStrats, inGameState, times: times, usePreviousRoom: usePreviousRoom);
            if (result != null)
            {
                result.ApplyOpenedLock(NodeLock, bestStrat);
            }
            return result;
        }
    }

    /// <summary>
    /// A class that encloses the bypassing of a NodeLock in an IExecutable interface.
    /// </summary>
    internal class BypassExecution : IExecutable
    {
        private NodeLock NodeLock { get; set; }

        public BypassExecution(NodeLock nodeLock)
        {
            NodeLock = nodeLock;
        }

        public ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            // Look for the best bypass strat
            (Strat bestStrat, ExecutionResult result) = model.ExecuteBest(NodeLock.BypassStrats, inGameState, times: times, usePreviousRoom: usePreviousRoom);
            if(result != null)
            {
                result.AddBypassedLock(NodeLock, bestStrat);
            }
            return result;
        }
    }
}
