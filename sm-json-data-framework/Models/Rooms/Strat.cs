using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Rooms
{
    public class Strat : AbstractModelElement, InitializablePostDeserializeInRoom, IExecutable
    {
        public string Name { get; set; }

        public bool Notable { get; set; }

        public LogicalRequirements Requires { get; set; }

        /// <summary>
        /// Obstacles that must be broken before or during the strat execution (or bypassed), mapped by their in-room ID.
        /// </summary>
        public IDictionary<string, StratObstacle> Obstacles { get; set; } = new Dictionary<string, StratObstacle>();

        /// <summary>
        /// Different ways the strat can be failed, mapped by name.
        /// </summary>
        public IDictionary<string, StratFailure> Failures { get; set; } = new Dictionary<string, StratFailure>();

        public ISet<string> StratProperties { get; set; } = new HashSet<string>();

        private int Tries { get; set; } = LogicalOptions.DefaultNumberOfTries;

        public Strat() { 

        }

        public Strat (RawStrat rawStrat, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            Name = rawStrat.Name;
            Notable = rawStrat.Notable;
            Requires = rawStrat.Requires.ToLogicalRequirements(knowledgeBase);
            Obstacles = rawStrat.Obstacles.Select(obstacle => new StratObstacle(obstacle, knowledgeBase)).ToDictionary(obstacle => obstacle.ObstacleId);
            Failures = rawStrat.Failures.Select(failure => new StratFailure(failure, knowledgeBase)).ToDictionary(failure => failure.Name);
            StratProperties = new HashSet<string>(rawStrat.StratProperties);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            Tries = logicalOptions?.NumberOfTries(this) ?? LogicalOptions.DefaultNumberOfTries;

            Requires.ApplyLogicalOptions(logicalOptions);

            foreach (StratFailure failure in Failures.Values)
            {
                failure.ApplyLogicalOptions(logicalOptions);
            }

            // Note that StratObstacles becoming impossible doesn't make this strat impossible,
            // as maybe there's other places where those obstacles can be destroyed.
            // If the obstacle has absolute requirements that become impossible though, then so do we
            bool impossibleObstacle = false;
            foreach (StratObstacle obstacle in Obstacles.Values)
            {
                obstacle.ApplyLogicalOptions(logicalOptions);
                obstacle.Obstacle.ApplyLogicalOptions(logicalOptions);
                if (obstacle.Obstacle.UselessByLogicalOptions)
                {
                    impossibleObstacle = true;
                }
            }
            return logicalOptions.IsStratEnabled(this) || Requires.UselessByLogicalOptions || impossibleObstacle;
        }

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            if(UselessByLogicalOptions)
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
            foreach (StratObstacle obstacle in Obstacles.Values.Where(o => !inGameState.GetDestroyedObstacleIds(previousRoomCount).Contains(o.ObstacleId)))
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

        public void InitializeProperties(SuperMetroidModel model, Room room)
        {
            foreach (StratFailure failure in Failures.Values)
            {
                failure.InitializeProperties(model, room);
            }

            foreach (StratObstacle obstacle in Obstacles.Values)
            {
                obstacle.InitializeProperties(model, room);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Requires.InitializeReferencedLogicalElementProperties(model, room));

            foreach(StratObstacle obstacle in Obstacles.Values)
            {
                unhandled.AddRange(obstacle.InitializeReferencedLogicalElementProperties(model, room));
            }

            foreach(StratFailure failure in Failures.Values)
            {
                unhandled.AddRange(failure.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
