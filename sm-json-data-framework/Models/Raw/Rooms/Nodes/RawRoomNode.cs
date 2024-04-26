using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Raw.Requirements;

namespace sm_json_data_framework.Models.Raw.Rooms.Nodes
{
    public class RawRoomNode
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public NodeTypeEnum NodeType { get; set; }

        public NodeSubTypeEnum NodeSubType { get; set; }

        public string NodeItem { get; set; }

        public string NodeAddress { get; set; }

        public IEnumerable<RawDoorEnvironment> DoorEnvironments { get; set; } = Enumerable.Empty<RawDoorEnvironment>();

        public RawLogicalRequirements InteractionRequires { get; set; } = new RawLogicalRequirements();

        public IEnumerable<RawRunway> Runways { get; set; } = Enumerable.Empty<RawRunway>();

        public IEnumerable<RawCanLeaveCharged> CanLeaveCharged { get; set; } = Enumerable.Empty<RawCanLeaveCharged>();

        public int? SpawnAt { get; set; }

        public IEnumerable<RawNodeLock> Locks { get; set; } = Enumerable.Empty<RawNodeLock>();

        public IEnumerable<UtilityEnum> Utility { get; set; } = Enumerable.Empty<UtilityEnum>();

        public IEnumerable<RawViewableNode> ViewableNodes { get; set; } = Enumerable.Empty<RawViewableNode>();

        public IEnumerable<string> Yields { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<RawTwinDoorAddress> TwinDoorAddresses { get; set; } = Enumerable.Empty<RawTwinDoorAddress>();
    }
}
