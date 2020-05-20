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

        public ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            times = times * model.LogicalOptions.NumberOfTries(this);

            ExecutionResult result = Requires.Execute(model, inGameState, times: times, usePreviousRoom: usePreviousRoom);

            if (result == null)
            {
                return null;
            }

            // Iterate over intact obstacles that need to be dealt with
            foreach (StratObstacle obstacle in Obstacles.Where(o => !inGameState.GetDestroyedObstacleIds(usePreviousRoom).Contains(o.ObstacleId)))
            {
                // Try destroying the obstacle first
                ExecutionResult destroyResult = result.AndThen(obstacle.DestroyExecution, model, times: times, usePreviousRoom: usePreviousRoom);

                // If destruction fails, try to bypass instead
                if (destroyResult == null)
                {
                    result = result.AndThen(obstacle.BypassExecution, model, times: times, usePreviousRoom: usePreviousRoom);
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

        public IEnumerable<Action> Initialize(SuperMetroidModel model, Room room)
        {
            List<Action> postRoomInitializeCallbacks = new List<Action>();
            foreach (StratFailure failure in Failures)
            {
                postRoomInitializeCallbacks.AddRange(failure.Initialize(model, room));
            }

            foreach(StratObstacle obstacle in Obstacles)
            {
                postRoomInitializeCallbacks.AddRange(obstacle.Initialize(model, room));
            }

            return postRoomInitializeCallbacks;
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
