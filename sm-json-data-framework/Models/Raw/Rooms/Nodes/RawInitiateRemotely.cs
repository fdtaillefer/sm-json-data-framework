using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Rooms.Nodes
{
    public class RawInitiateRemotely
    {
        public int InitiateAt { get; set; }

        public bool MustOpenDoorFirst { get; set; }

        [JsonPropertyName("pathToDoor")]
        public IList<RawInitiateRemotelyPathToDoorNode> PathToDoor { get; set; } = new List<RawInitiateRemotelyPathToDoorNode>();
    }
}
