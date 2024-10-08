﻿using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Strings;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Rooms
{
    /// <summary>
    /// Represents a specific way to do something in a specific room, in order to do things like move between nodes or unlock a node.
    /// </summary>
    public class Strat : AbstractModelElement<UnfinalizedStrat, Strat>, IExecutable, ILogicalExecutionPreProcessable
    {

        /// <summary>
        /// Number of tries the player is expected to take to execute the strat, as per applied logical options.
        /// </summary>
        public int Tries => AppliedLogicalOptions.NumberOfTries(this);

        public Strat(UnfinalizedStrat sourceElement, Action<Strat> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Name = sourceElement.Name;
            Notable = sourceElement.Notable;
            Requires = sourceElement.Requires.Finalize(mappings);
            Obstacles = sourceElement.Obstacles.Values.Select(obstacle => obstacle.Finalize(mappings)).ToDictionary(obstacle => obstacle.Obstacle.Id).AsReadOnly();
            Failures = sourceElement.Failures.Values.Select(failure => failure.Finalize(mappings)).ToDictionary(failure => failure.Name).AsReadOnly();
            StratProperties = sourceElement.StratProperties.AsReadOnly();
        }

        /// <summary>
        /// The name of this Strat. This is only unique for strats that are notable.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Whether this strat is notable. A strat being notable usually means it requires specific knowledge beyond the ability to fulfill the logical requirements.
        /// </summary>
        public bool Notable { get; }

        /// <summary>
        /// The logical requirements that must be fulfilled to execute this Strat.
        /// </summary>
        public LogicalRequirements Requires { get; }

        /// <summary>
        /// Obstacles that must be broken before or during the strat execution (or bypassed), mapped by their in-room ID.
        /// </summary>
        public IReadOnlyDictionary<string, StratObstacle> Obstacles { get; }

        /// <summary>
        /// Different ways the strat can be failed, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, StratFailure> Failures { get;  }

        /// <summary>
        /// The set of properties associated with this strat. 
        /// Can be relevant in subsequent navigation to fulfill a <see cref="PreviousStratProperty"/> logical element.
        /// </summary>
        public IReadOnlySet<string> StratProperties { get; }

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            if (LogicallyNever)
            {
                return null;
            }

            times *= Tries;

            ExecutionResult result = Requires.Execute(model, inGameState, times: times, previousRoomCount: previousRoomCount);

            if (result == null)
            {
                return null;
            }

            // Iterate over intact obstacles that need to be dealt with (so ignore obstacles that are already destroyed)
            foreach (StratObstacle obstacle in Obstacles.Values.Where(o => !inGameState.GetDestroyedObstacleIds(previousRoomCount).Contains(o.Obstacle.Id)))
            {
                // Try destroying the obstacle first
                ExecutionResult destroyResult = result.AndThen(obstacle.DestroyExecution, model, times: times, previousRoomCount: previousRoomCount);

                // If destruction fails, try to bypass instead
                if (destroyResult == null)
                {
                    result = result.AndThen(obstacle.BypassExecution, model, times: times, previousRoomCount: previousRoomCount);
                    // If bypass also fails, we cannot get past this obstacle. Give up.
                    if (result == null)
                    {
                        return null;
                    }
                }
                // If destruction succeeded, carry on with the result of that
                else
                {
                    result = destroyResult;
                }
            }

            return result;
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            Requires.ApplyLogicalOptions(logicalOptions, model);

            foreach (StratFailure failure in Failures.Values)
            {
                failure.ApplyLogicalOptions(logicalOptions, model);
            }

            foreach (StratObstacle stratObstacle in Obstacles.Values)
            {
                stratObstacle.ApplyLogicalOptions(logicalOptions, model);
                stratObstacle.Obstacle.ApplyLogicalOptions(logicalOptions, model);
            }
        }

        protected override void UpdateLogicalProperties(SuperMetroidModel model)
        {
            base.UpdateLogicalProperties(model);
            LogicallyNever = CalculateLogicallyNever(model);
            LogicallyAlways = CalculateLogicallyAlways(model);
            LogicallyFree = CalculateLogicallyFree(model);
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidModel model)
        {
            // A strat that can never be executed may as well not exist
            return !CalculateLogicallyNever(model);
        }

        /// <summary>
        /// If true, then this strat is impossible to execute given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyNever(SuperMetroidModel model)
        {
            // A strat is impossible to execute if it has impossible requirements, but also if it has any impossible obstacle
            // or if it's just logically disabled
            return Requires.LogicallyNever || Obstacles.Values.Any(obstacle => obstacle.LogicallyNever)
                || !AppliedLogicalOptions.IsStratEnabled(this);
        }

        public bool LogicallyAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlways"/> should currently be.
        /// </summary>
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyAlways(SuperMetroidModel model)
        {
            // Can always be fulfilled if enabled, and its requirements can always be done, and its obstacles can always be destroyed or bypassed
            return AppliedLogicalOptions.IsStratEnabled(this) && Requires.LogicallyAlways && !Obstacles.Values.Any(obstacle => !obstacle.LogicallyAlways);
        }

        public bool LogicallyFree { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyFree"/> should currently be.
        /// </summary>
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyFree(SuperMetroidModel model)
        {
            // Free if enabled, has free requirements, and its obstacles can always be destroyed or bypassed for free
            return AppliedLogicalOptions.IsStratEnabled(this) && Requires.LogicallyFree && !Obstacles.Values.Any(obstacle => !obstacle.LogicallyFree);
        }
    }

    public class UnfinalizedStrat : AbstractUnfinalizedModelElement<UnfinalizedStrat, Strat>, InitializablePostDeserializeInRoom
    {
        public string Name { get; set; }

        public bool Notable { get; set; }

        public UnfinalizedLogicalRequirements Requires { get; set; }

        /// <summary>
        /// Obstacles that must be broken before or during the strat execution (or bypassed), mapped by their in-room ID.
        /// </summary>
        public IDictionary<string, UnfinalizedStratObstacle> Obstacles { get; set; } = new Dictionary<string, UnfinalizedStratObstacle>();

        /// <summary>
        /// Different ways the strat can be failed, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedStratFailure> Failures { get; set; } = new Dictionary<string, UnfinalizedStratFailure>();

        public ISet<string> StratProperties { get; set; } = new HashSet<string>();

        public UnfinalizedStrat() { 

        }

        public UnfinalizedStrat (RawStrat rawStrat, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            Name = rawStrat.Name;
            Notable = rawStrat.Notable;
            Requires = rawStrat.Requires.ToLogicalRequirements(knowledgeBase);
            Obstacles = rawStrat.Obstacles.Select(obstacle => new UnfinalizedStratObstacle(obstacle, knowledgeBase)).ToDictionary(obstacle => obstacle.ObstacleId);
            Failures = rawStrat.Failures.Select(failure => new UnfinalizedStratFailure(failure, knowledgeBase)).ToDictionary(failure => failure.Name);
            StratProperties = new HashSet<string>(rawStrat.StratProperties);
        }

        protected override Strat CreateFinalizedElement(UnfinalizedStrat sourceElement, Action<Strat> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new Strat(sourceElement, mappingsInsertionCallback, mappings);
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            foreach (UnfinalizedStratFailure failure in Failures.Values)
            {
                failure.InitializeProperties(model, room);
            }

            foreach (UnfinalizedStratObstacle obstacle in Obstacles.Values)
            {
                obstacle.InitializeProperties(model, room);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Requires.InitializeReferencedLogicalElementProperties(model, room));

            foreach(UnfinalizedStratObstacle obstacle in Obstacles.Values)
            {
                unhandled.AddRange(obstacle.InitializeReferencedLogicalElementProperties(model, room));
            }

            foreach(UnfinalizedStratFailure failure in Failures.Values)
            {
                unhandled.AddRange(failure.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
