using sm_json_data_framework.Models.Raw.Requirements;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Rooms
{
    public class RawFarmCycle
    {
        public string Name { get; set; }

        public int CycleFrames { get; set; }

        public RawLogicalRequirements Requires { get; set; }
    }
}
