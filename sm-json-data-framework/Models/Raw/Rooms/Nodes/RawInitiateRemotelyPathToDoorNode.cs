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

        public ISet<string> Strats { get; set; } = new HashSet<string>();
    }
}
