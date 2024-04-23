using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Rooms
{
    public class Strat : InitializablePostDeserializeInRoom, IExecutable
    {
        public string Name { get; set; }

        public bool Notable { get; set; }

        public LogicalRequirements Requires { get; set; }

        public IEnumerable<StratObstacle> Obstacles { get; set; } = Enumerable.Empty<StratObstacle>();

        public IEnumerable<StratFailure> Failures { get; set; } = Enumerable.Empty<StratFailure>();

        public IEnumerable<string> StratProperties { get; set; } = Enumerable.Empty<string>();

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            times = times * model.LogicalOptions.NumberOfTries(this);

            ExecutionResult result = Requires.Execute(model, inGameState, times: times, previousRoomCount: previousRoomCount);

            if (result == null)
            {
                return null;
            }

            // Iterate over intact obstacles that need to be dealt with
            foreach (StratObstacle obstacle in Obstacles.Where(o => !inGameState.GetDestroyedObstacleIds(previousRoomCount).Contains(o.ObstacleId)))
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

        public void InitializeForeignProperties(SuperMetroidModel model, Room room)
        {
            foreach (StratFailure failure in Failures)
            {
                failure.InitializeForeignProperties(model, room);
            }

            foreach (StratObstacle obstacle in Obstacles)
            {
                obstacle.InitializeForeignProperties(model, room);
            }
        }

        public void InitializeOtherProperties(SuperMetroidModel model, Room room)
        {
            foreach (StratFailure failure in Failures)
            {
                failure.InitializeOtherProperties(model, room);
            }

            foreach (StratObstacle obstacle in Obstacles)
            {
                obstacle.InitializeOtherProperties(model, room);
            }
        }

        public bool CleanUpUselessValues(SuperMetroidModel model, Room room)
        {
            Failures = Failures.Where(failure => failure.CleanUpUselessValues(model, room));

            Obstacles = Obstacles.Where(obstacle => obstacle.CleanUpUselessValues(model, room));

            // There's nothing being cleaned up here that can make a strat useless by disappearing.
            // However, a strat that is disabled or has requirements that can never be fulfilled is useless
            return model.LogicalOptions.IsStratEnabled(this) && !Requires.IsNever();
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Requires.InitializeReferencedLogicalElementProperties(model, room));

            foreach(StratObstacle obstacle in Obstacles)
            {
                unhandled.AddRange(obstacle.InitializeReferencedLogicalElementProperties(model, room));
            }

            foreach(StratFailure failure in Failures)
            {
                unhandled.AddRange(failure.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
