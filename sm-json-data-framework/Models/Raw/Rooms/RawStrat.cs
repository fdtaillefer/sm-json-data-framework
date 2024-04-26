using sm_json_data_framework.Models.Raw.Requirements;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Rooms
{
    public class RawStrat
    {
        public string Name { get; set; }

        public bool Notable { get; set; }

        public RawLogicalRequirements Requires { get; set; }

        public IEnumerable<RawStratObstacle> Obstacles { get; set; } = Enumerable.Empty<RawStratObstacle>();

        public IEnumerable<RawStratFailure> Failures { get; set; } = Enumerable.Empty<RawStratFailure>();

        public IEnumerable<string> StratProperties { get; set; } = Enumerable.Empty<string>();
    }
}
