using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    public class ResetRoom : AbstractObjectLogicalElement
    {
        [JsonPropertyName("nodes")]
        public IEnumerable<int> NodeIds { get; set; } = Enumerable.Empty<int>();

        [JsonPropertyName("nodesToAvoid")]
        public IEnumerable<int> NodeIdsToAvoid { get; set; } = Enumerable.Empty<int>();

        [JsonPropertyName("obstaclesToAvoid")]
        public IEnumerable<string> ObstaclesIdsToAvoid { get; set; } = Enumerable.Empty<string>();

        public bool MustStayPut { get; set; } = false;
    }
}
