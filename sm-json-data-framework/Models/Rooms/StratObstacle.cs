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

        public LogicalRequirements Bypass { get; set; } = new LogicalRequirements();

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

            unhandled.AddRange(Bypass.InitializeReferencedLogicalElementProperties(model, room));

            return unhandled.Distinct();
        }
    }
}
