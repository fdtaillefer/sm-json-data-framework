using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class StratObstacle : InitializablePostDeserializeInRoom
    {
        [JsonPropertyName("id")]
        public string ObstacleId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The RoomObstacle that this StratObstacle indicates must be passed through</para>
        /// </summary>
        [JsonIgnore]
        public RoomObstacle Obstacle { get; set; }

        public LogicalRequirements Requires { get; set; } = new LogicalRequirements();

        /// <summary>
        /// LogicalRequirements to bypass this obstcle without destroying it when doing the associated strat. If this is null, the obstacle cannot be bypassed.
        /// </summary>
        public LogicalRequirements Bypass { get; set; }

        [JsonPropertyName("additionalObstacles")]
        public IEnumerable<string> AdditionalObstacleIds { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The additional RoomObstacles that are destroyed alongside this StratObstacle</para>
        /// </summary>
        [JsonIgnore]
        public IEnumerable<RoomObstacle> AdditionalObstacles { get; set; }

        public void Initialize(SuperMetroidModel model, Room room)
        {
            // Initialize Obstacle
            Obstacle = room.Obstacles[ObstacleId];

            // Initialize AdditionalObstacles
            AdditionalObstacles = AdditionalObstacleIds.Select(id => room.Obstacles[id]);
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Requires.InitializeReferencedLogicalElementProperties(model, room));

            if (Bypass != null)
            {
                unhandled.AddRange(Bypass.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }

        /// <summary>
        /// Attemps to execute the bypass of this obstacle without destroying it, and returns a new InGameState describing the state after success (or null).
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="times">The number of consecutive times that this should be fulfilled. Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// <returns>A new InGameState describing the state after succeeding, or null in case of failure</returns>
        public InGameState AttemptBypass(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            // The bypass attempt fails if there's no way to bypass
            if (Bypass == null)
            {
                return null;
            }
            else
            {
                return Bypass.AttemptFulfill(model, inGameState, times: times, usePreviousRoom: usePreviousRoom);
            }
        }

        /// <summary>
        /// Attemps to execute the destruction of this obstacle, and returns a new InGameState describing the state after success (or null).
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="times">The number of consecutive times that this should be fulfilled. Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// <returns>A new InGameState describing the state after succeeding, or null in case of failure</returns>
        public InGameState AttemptDestroy(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            // There may be up to 2 requirements. This StratObstacle may have some, and the RoomObstacle may also have some general requirements that apply to any strat.

            // Start with the RoomObstacle's requirements
            InGameState resultingState = Obstacle.Requires.AttemptFulfill(model, inGameState, times: times, usePreviousRoom: usePreviousRoom);
            // If we couldn't fulfill the RoomObstacle's requirements, give up
            if (resultingState == null)
            {
                return null;
            }

            resultingState = Requires.AttemptFulfill(model, resultingState, times: times, usePreviousRoom: usePreviousRoom);
            // If we couldn't fulfill this StratObstatcle's requirements, give up
            if(resultingState == null)
            {
                return null;
            }

            // We have succeeded, but we must update the InGameState to reflect any destroyed obstacles
            resultingState.ApplyDestroyedObstacle(Obstacle);
            foreach(RoomObstacle obstacle in AdditionalObstacles)
            {
                resultingState.ApplyDestroyedObstacle(obstacle);
            }

            return resultingState;
        }
    }
}
