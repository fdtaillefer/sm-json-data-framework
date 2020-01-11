using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class RoomEnemy
    {
        [JsonPropertyName("name")]
        public string EnemyName { get; set; }

        public int Quantity { get; set; }

        [JsonPropertyName("homeNodes")]
        public IEnumerable<int> HomeNodeIds { get; set; } = Enumerable.Empty<int>();

        [JsonPropertyName("betweenNodes")]
        public IEnumerable<int> BetweenNodeIds { get; set; } = Enumerable.Empty<int>();

        public LogicalRequirements Spawn { get; set; } = new LogicalRequirements();

        public LogicalRequirements StopSpawn { get; set; } = new LogicalRequirements();

        public LogicalRequirements DropRequires { get; set; } = new LogicalRequirements();

        // STITCHME Note?
    }
}
