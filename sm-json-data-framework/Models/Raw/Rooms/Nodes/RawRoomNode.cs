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

        public IList<RawDoorEnvironment> DoorEnvironments { get; set; } = new List<RawDoorEnvironment>();

        public RawLogicalRequirements InteractionRequires { get; set; } = new RawLogicalRequirements();

        public IList<RawRunway> Runways { get; set; } = new List<RawRunway>();

        public IList<RawCanLeaveCharged> CanLeaveCharged { get; set; } = new List<RawCanLeaveCharged>();

        public int? SpawnAt { get; set; }

        public IList<RawNodeLock> Locks { get; set; } = new List<RawNodeLock>();

        public ISet<UtilityEnum> Utility { get; set; } = new HashSet<UtilityEnum>();

        public IList<RawViewableNode> ViewableNodes { get; set; } = new List<RawViewableNode>();

        public ISet<string> Yields { get; set; } = new HashSet<string>();

        public IList<RawTwinDoorAddress> TwinDoorAddresses { get; set; } = new List<RawTwinDoorAddress>();
    }
}
