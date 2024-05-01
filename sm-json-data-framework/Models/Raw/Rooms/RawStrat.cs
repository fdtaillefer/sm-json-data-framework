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

        public IList<RawStratObstacle> Obstacles { get; set; } = new List<RawStratObstacle>();

        public IList<RawStratFailure> Failures { get; set; } = new List<RawStratFailure>();

        public ISet<string> StratProperties { get; set; } = new HashSet<string>();
    }
}
