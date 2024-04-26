using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Rooms.Nodes
{
    public class RawInitiateRemotelyPathToDoorNode
    {
        public int DestinationNode { get; set; }

        public IEnumerable<string> Strats { get; set; } = Enumerable.Empty<string>();
    }
}
