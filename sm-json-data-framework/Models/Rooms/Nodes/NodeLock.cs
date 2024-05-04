using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Options;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class NodeLock : AbstractModelElement, InitializablePostDeserializeInNode
    {
        public LockTypeEnum LockType { get; set; }

        /// <summary>
        /// Logical requirements that must be met for this lock to be active
        /// </summary>
        public LogicalRequirements Lock { get; set; } = new LogicalRequirements();

        public string Name { get; set; }

        /// <summary>
        /// Strats that can be executed to unlock this lock, mapped by name.
        /// </summary>
        public IDictionary<string, Strat> UnlockStrats { get; set; } = new Dictionary<string, Strat>();

        /// <summary>
        /// Strats that can be executed to bypass this lock, mapped by name.
        /// </summary>
        public IDictionary<string, Strat> BypassStrats { get; set; } = new Dictionary<string, Strat>();

        [JsonPropertyName("yields")]
        public ISet<string> YieldsStrings { get; set; } = new HashSet<string>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room, RoomNode)"/> has been called.</para>
        /// <para>The game flags that are activated by unlocking this lock.</para>
        /// </summary>
        [JsonIgnore]
        public IList<GameFlag> Yields { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room, RoomNode)"/> has been called.</para>
        /// <para>The RoomNode on which this lock is.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode Node { get; set; }

        public NodeLock()
        {

        }

        public NodeLock(RawNodeLock rawNodeLock, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            LockType = rawNodeLock.LockType;
            Lock = rawNodeLock.Lock.ToLogicalRequirements(knowledgeBase);
            Name = rawNodeLock.Name;
            UnlockStrats = rawNodeLock.UnlockStrats.Select(strat => new Strat(strat, knowledgeBase)).ToDictionary(strat => strat.Name);
            BypassStrats = rawNodeLock.BypassStrats.Select(strat => new Strat(strat, knowledgeBase)).ToDictionary(strat => strat.Name);
            YieldsStrings = new HashSet<string>(rawNodeLock.Yields);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            Lock.ApplyLogicalOptions(logicalOptions);

            foreach (Strat strat in UnlockStrats.Values)
            {
                strat.ApplyLogicalOptions(logicalOptions);
            }

            foreach (Strat strat in BypassStrats.Values)
            {
                strat.ApplyLogicalOptions(logicalOptions);
            }

            // A lock remains useful even if it's impolssible to unlock or bypass as it plays the role of blocking the way.
            // It does become useless if its activation conditions become impossible though
            return Lock.UselessByLogicalOptions;
        }

        public void InitializeProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            Node = node;

            // Initialize unlock strats
            foreach (Strat strat in UnlockStrats.Values)
            {
                strat.InitializeProperties(model, room);
            }

            // Initialize bypass strats
            foreach (Strat strat in BypassStrats.Values)
            {
                strat.InitializeProperties(model, room);
            }

            // Initialize Yielded game flags
            Yields = YieldsStrings.Select(s => model.GameFlags[s]).ToList();
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Lock.InitializeReferencedLogicalElementProperties(model, room));

            foreach(Strat strat in UnlockStrats.Values)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            foreach (Strat strat in BypassStrats.Values)
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
        public bool IsOpen(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return inGameState.OpenedLocks.ContainsLock(this);
        }

        /// <summary>
        /// Returns whether this lock is currently active in the provided InGameState.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <returns></returns>
        public bool IsActive(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            // This lock cannot be active if it's been opened
            if (inGameState.OpenedLocks.ContainsLock(this))
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

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // Can't open a lock that isn't active
            if (!NodeLock.IsActive(model, inGameState))
            {
                return null;
            }

            // Look for the best unlock strat
            (Strat bestStrat, ExecutionResult result) = model.ExecuteBest(NodeLock.UnlockStrats.Values.WhereUseful(), inGameState, times: times, previousRoomCount: previousRoomCount);
            if (result != null)
            {
                result.ApplyOpenedLock(NodeLock, bestStrat);
                foreach (GameFlag gameFlag in NodeLock.Yields)
                {
                    result.ApplyActivatedGameFlag(gameFlag);
                }
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

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // If there are no bypass strats, bypassing fails
            if (!NodeLock.BypassStrats.Any())
            {
                return null;
            }

            // Look for the best bypass strat
            (Strat bestStrat, ExecutionResult result) = model.ExecuteBest(NodeLock.BypassStrats.Values.WhereUseful(), inGameState, times: times, previousRoomCount: previousRoomCount);
            if(result != null)
            {
                result.ApplyBypassedLock(NodeLock, bestStrat);
            }
            return result;
        }
    }
}
