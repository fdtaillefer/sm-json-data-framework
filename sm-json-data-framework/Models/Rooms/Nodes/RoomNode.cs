using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Node.NodeSparking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class RoomNode
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public NodeTypeEnum NodeType { get; set; }

        public NodeSubTypeEnum NodeSubType { get; set; }

        public string NodeItem { get; set; }

        public string NodeAddress { get; set; }

        public LogicalRequirements InteractionRequires { get; set; }

        public Sparking Sparking { get; set; }

        // Decide who should handle null here
        [JsonPropertyName("spawnAt")]
        public int? SpawnAtNodeId { get; set; }

        public IEnumerable<NodeLock> Locks { get; set; } = Enumerable.Empty<NodeLock>();

        public IEnumerable<UtilityEnum> Utility { get; set; } = Enumerable.Empty<UtilityEnum>();

        public IEnumerable<ViewableNode> ViewableNodes { get; set; } = Enumerable.Empty<ViewableNode>();

        public IEnumerable<string> Yields { get; set; } = Enumerable.Empty<string>();

        // STITCHME Note?
    }
}
