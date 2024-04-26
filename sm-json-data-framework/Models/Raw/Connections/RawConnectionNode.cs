using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Connections
{
    public class RawConnectionNode
    {
        public string Area { get; set; }

        public string Subarea { get; set; }

        public int Roomid { get; set; }

        public string RoomName { get; set; }

        public int Nodeid { get; set; }

        public string NodeName { get; set; }

        public ConnectionNodePositionEnum Position { get; set; }
    }
}
