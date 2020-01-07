using sm_json_data_parser.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_parser.Models.Rooms.Nodes
{
    public class NodeLock
    {
        public LockTypeEnum LockType { get; set; }

        public LogicalRequirements Lock { get; set; } = new LogicalRequirements();

        public IEnumerable<Strat> UnlockStrats { get; set; } = Enumerable.Empty<Strat>();

        public IEnumerable<Strat> BypassStrats { get; set; } = Enumerable.Empty<Strat>();

        // STITCHME Note
    }
}
