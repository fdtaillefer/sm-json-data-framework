using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class StratObstacle
    {
        [JsonPropertyName("id")]
        public string ObstacleId { get; set; }

        public LogicalRequirements Requires { get; set; } = new LogicalRequirements();

        public LogicalRequirements Bypass { get; set; } = new LogicalRequirements();

        [JsonPropertyName("additionalObstacles")]
        public IEnumerable<string> AdditionalObstacleIds { get; set; } = Enumerable.Empty<string>();

        // STITCHME Note?
    }
}
