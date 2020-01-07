using sm_json_data_parser.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_parser.Models.Rooms
{
    public class StratFailure
    {
        public string Name { get; set; }

        [JsonPropertyName("leadsToNode")]
        public int? LeadsToNodeId { get; set; }

        public LogicalRequirements Cost { get; set; } = new LogicalRequirements();

        public bool Softlock { get; set; } = false;

        public bool ClearsPreviousNode { get; set; } = false;

        // STITCHME Note?
    }
}
