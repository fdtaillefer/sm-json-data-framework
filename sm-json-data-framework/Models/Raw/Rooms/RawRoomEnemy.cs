using sm_json_data_framework.Models.Enemies;
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

namespace sm_json_data_framework.Models.Raw.Rooms
{
    public class RawRoomEnemy
    {
        public string Id { get; set; }

        public string GroupName { get; set; }

        public string EnemyName { get; set; }

        public int Quantity { get; set; }

        public ISet<int> HomeNodes { get; set; } = new HashSet<int>();

        public ISet<int> BetweenNodes { get; set; } = new HashSet<int>();

        public RawLogicalRequirements Spawn { get; set; } = new RawLogicalRequirements();

        public RawLogicalRequirements StopSpawn { get; set; }

        public RawLogicalRequirements DropRequires { get; set; } = new RawLogicalRequirements();

        public IList<RawFarmCycle> FarmCycles { get; set; } = new List<RawFarmCycle>();
    }
}
