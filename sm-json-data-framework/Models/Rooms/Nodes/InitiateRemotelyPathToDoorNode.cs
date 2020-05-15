using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class InitiateRemotelyPathToDoorNode
    {
        [JsonPropertyName("destinationNode")]
        public int DestinationNodeId { get; set; }

        [JsonPropertyName("strats")]
        public IEnumerable<string> StratNames { get; set; } = Enumerable.Empty<string>();
    }
}
