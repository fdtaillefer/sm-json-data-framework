using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Rooms.Nodes
{
    public class RawDoorEnvironment
    {
        public PhysicsEnum Physics { get; set; }

        public IEnumerable<int> EntranceNodes { get; set; }
    }
}
