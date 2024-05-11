﻿using sm_json_data_framework.Models.GameFlags;
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
    /// <summary>
    /// Represents a lock on a node. When active, a lock prevents interaction with a node until unlocked or unless bypassed.
    /// </summary>
    public class NodeLock : AbstractModelElement<UnfinalizedNodeLock, NodeLock>
    {
        private UnfinalizedNodeLock InnerElement { get; set; }

        public NodeLock(UnfinalizedNodeLock innerElement, Action<NodeLock> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            Lock = InnerElement.Lock.Finalize(mappings);
            UnlockStrats = InnerElement.UnlockStrats.Values.Select(strat => strat.Finalize(mappings)).ToDictionary(strat => strat.Name).AsReadOnly();
            BypassStrats = InnerElement.BypassStrats.Values.Select(strat => strat.Finalize(mappings)).ToDictionary(strat => strat.Name).AsReadOnly();
            YieldsStrings = InnerElement.YieldsStrings.AsReadOnly();
            Yields = InnerElement.Yields.Select(flag => flag.Finalize(mappings)).ToList().AsReadOnly();
            Node = InnerElement.Node.Finalize(mappings);
        }

        /// <summary>
        /// The type of this lock.
        /// </summary>
        public LockTypeEnum LockType { get { return InnerElement.LockType; } }

        /// <summary>
        /// Logical requirements that must be met for this lock to be active.
        /// If not met, the lock is not yet active and does not need to be unlocked or bypassed.
        /// </summary>
        public LogicalRequirements Lock { get; }

        /// <summary>
        /// A name that identifies this lock. Unique across the entire model.
        /// </summary>
        public string Name { get { return InnerElement.Name; } }

        /// <summary>
        /// Strats that can be executed to unlock this lock, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Strat> UnlockStrats { get; }

        /// <summary>
        /// Strats that can be executed to bypass this lock, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Strat> BypassStrats { get; }

        public IReadOnlySet<string> YieldsStrings { get; }

        /// <summary>
        /// The game flags that are activated by unlocking this lock.
        /// </summary>
        public IReadOnlyList<GameFlag> Yields { get; }

        /// <summary>
        /// The RoomNode on which this lock is.
        /// </summary>
        public RoomNode Node { get; }

        /// <summary>
        /// Returns specifically whether this lock has been opened. If it's not yet active, this will return false.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <returns></returns>
        public bool IsOpen(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return InnerElement.IsOpen(model, inGameState);
        }

        /// <summary>
        /// Returns whether this lock is currently active in the provided InGameState.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <returns></returns>
        public bool IsActive(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return InnerElement.IsActive(model, inGameState);
        }

        /// <summary>
        /// An IExecutable that corresponds to opening this lock.
        /// </summary>
        public IExecutable OpenExecution { get { return InnerElement.OpenExecution; } }

        /// <summary>
        /// An IExecutable that corresponds to bypassing this lock.
        /// </summary>
        public IExecutable BypassExecution { get { return InnerElement.BypassExecution; } }
    }

    public class UnfinalizedNodeLock : AbstractUnfinalizedModelElement<UnfinalizedNodeLock, NodeLock>, InitializablePostDeserializeInNode
    {
        public LockTypeEnum LockType { get; set; }

        /// <summary>
        /// Logical requirements that must be met for this lock to be active
        /// </summary>
        public UnfinalizedLogicalRequirements Lock { get; set; } = new UnfinalizedLogicalRequirements();

        public string Name { get; set; }

        /// <summary>
        /// Strats that can be executed to unlock this lock, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedStrat> UnlockStrats { get; set; } = new Dictionary<string, UnfinalizedStrat>();

        /// <summary>
        /// Strats that can be executed to bypass this lock, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedStrat> BypassStrats { get; set; } = new Dictionary<string, UnfinalizedStrat>();

        [JsonPropertyName("yields")]
        public ISet<string> YieldsStrings { get; set; } = new HashSet<string>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, UnfinalizedRoom, UnfinalizedRoomNode)"/> has been called.</para>
        /// <para>The game flags that are activated by unlocking this lock.</para>
        /// </summary>
        [JsonIgnore]
        public IList<UnfinalizedGameFlag> Yields { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, UnfinalizedRoom, UnfinalizedRoomNode)"/> has been called.</para>
        /// <para>The RoomNode on which this lock is.</para>
        /// </summary>
        [JsonIgnore]
        public UnfinalizedRoomNode Node { get; set; }

        public UnfinalizedNodeLock()
        {

        }

        public UnfinalizedNodeLock(RawNodeLock rawNodeLock, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            LockType = rawNodeLock.LockType;
            Lock = rawNodeLock.Lock.ToLogicalRequirements(knowledgeBase);
            Name = rawNodeLock.Name;
            UnlockStrats = rawNodeLock.UnlockStrats.Select(strat => new UnfinalizedStrat(strat, knowledgeBase)).ToDictionary(strat => strat.Name);
            BypassStrats = rawNodeLock.BypassStrats.Select(strat => new UnfinalizedStrat(strat, knowledgeBase)).ToDictionary(strat => strat.Name);
            YieldsStrings = new HashSet<string>(rawNodeLock.Yields);
        }

        protected override NodeLock CreateFinalizedElement(UnfinalizedNodeLock sourceElement, Action<NodeLock> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new NodeLock(sourceElement, mappingsInsertionCallback, mappings);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            Lock.ApplyLogicalOptions(logicalOptions);

            foreach (UnfinalizedStrat strat in UnlockStrats.Values)
            {
                strat.ApplyLogicalOptions(logicalOptions);
            }

            foreach (UnfinalizedStrat strat in BypassStrats.Values)
            {
                strat.ApplyLogicalOptions(logicalOptions);
            }

            // A lock remains useful even if it's impolssible to unlock or bypass as it plays the role of blocking the way.
            // It does become useless if its activation conditions become impossible though
            return Lock.UselessByLogicalOptions;
        }

        public void InitializeProperties(SuperMetroidModel model, UnfinalizedRoom room, UnfinalizedRoomNode node)
        {
            Node = node;

            // Initialize unlock strats
            foreach (UnfinalizedStrat strat in UnlockStrats.Values)
            {
                strat.InitializeProperties(model, room);
            }

            // Initialize bypass strats
            foreach (UnfinalizedStrat strat in BypassStrats.Values)
            {
                strat.InitializeProperties(model, room);
            }

            // Initialize Yielded game flags
            Yields = YieldsStrings.Select(s => model.GameFlags[s]).ToList();
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, UnfinalizedRoom room, UnfinalizedRoomNode node)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Lock.InitializeReferencedLogicalElementProperties(model, room));

            foreach(UnfinalizedStrat strat in UnlockStrats.Values)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            foreach (UnfinalizedStrat strat in BypassStrats.Values)
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
        private UnfinalizedNodeLock NodeLock { get; set; }

        public OpenExecution(UnfinalizedNodeLock nodeLock)
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
            (UnfinalizedStrat bestStrat, ExecutionResult result) = model.ExecuteBest(NodeLock.UnlockStrats.Values.WhereUseful(), inGameState, times: times, previousRoomCount: previousRoomCount);
            if (result != null)
            {
                result.ApplyOpenedLock(NodeLock, bestStrat);
                foreach (UnfinalizedGameFlag gameFlag in NodeLock.Yields)
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
        private UnfinalizedNodeLock NodeLock { get; set; }

        public BypassExecution(UnfinalizedNodeLock nodeLock)
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
            (UnfinalizedStrat bestStrat, ExecutionResult result) = model.ExecuteBest(NodeLock.BypassStrats.Values.WhereUseful(), inGameState, times: times, previousRoomCount: previousRoomCount);
            if(result != null)
            {
                result.ApplyBypassedLock(NodeLock, bestStrat);
            }
            return result;
        }
    }
}
