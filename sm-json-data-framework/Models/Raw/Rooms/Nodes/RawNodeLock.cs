using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Raw.Requirements;

namespace sm_json_data_framework.Models.Raw.Rooms.Nodes
{
    public class RawNodeLock
    {
        public LockTypeEnum LockType { get; set; }

        public RawLogicalRequirements Lock { get; set; } = new RawLogicalRequirements();

        public string Name { get; set; }

        public IList<RawStrat> UnlockStrats { get; set; } = new List<RawStrat>();

        public IList<RawStrat> BypassStrats { get; set; } = new List<RawStrat>();

        public ISet<string> Yields { get; set; } = new HashSet<string>();
    }
}
