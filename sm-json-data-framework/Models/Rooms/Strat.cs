using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Strings;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
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
    public class Strat : AbstractModelElement<UnfinalizedStrat, Strat>, IExecutable
    {

        /// <summary>
        /// Number of tries the player is expected to take to execute the strat, as per applied logical options.
        /// </summary>
        public int Tries => AppliedLogicalOptions.NumberOfTries(this);

        private UnfinalizedStrat InnerElement { get; set; }

        public Strat(UnfinalizedStrat innerElement, Action<Strat> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            Requires = InnerElement.Requires.Finalize(mappings);
            Obstacles = InnerElement.Obstacles.Values.Select(obstacle => obstacle.Finalize(mappings)).ToDictionary(obstacle => obstacle.Obstacle.Id).AsReadOnly();
            Failures = InnerElement.Failures.Values.Select(failure => failure.Finalize(mappings)).ToDictionary(failure => failure.Name).AsReadOnly();
            StratProperties = InnerElement.StratProperties.AsReadOnly();
        }

        /// <summary>
        /// The name of this Strat. This is only unique for strats that are notable.
        /// </summary>
        public string Name => InnerElement.Name;

        /// <summary>
        /// Whether this strat is notable. A strat being notable usually means it requires specific knowledge beyond the ability to fulfill the logical requirements.
        /// </summary>
        public bool Notable => InnerElement.Notable;

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
            if (UselessByLogicalOptions)
            {
                return null;
            }

            times = times * Tries;

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

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            Requires.ApplyLogicalOptions(logicalOptions);

            foreach (StratFailure failure in Failures.Values)
            {
                failure.ApplyLogicalOptions(logicalOptions);
            }

            // A StratObstacle can make this strat useless if it's impossible to bypass it here, and impossible to destroy it at all
            bool impossibleObstacle = false;
            foreach (StratObstacle stratObstacle in Obstacles.Values)
            {
                stratObstacle.ApplyLogicalOptions(logicalOptions);
                stratObstacle.Obstacle.ApplyLogicalOptions(logicalOptions);
                if (stratObstacle.UselessFromHereByLogicalOptions && stratObstacle.Obstacle.IndestructibleByLogicalOptions)
                {
                    impossibleObstacle = true;
                }
            }
            return !logicalOptions.IsStratEnabled(this) || Requires.UselessByLogicalOptions || impossibleObstacle;
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
