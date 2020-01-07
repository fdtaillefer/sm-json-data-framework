using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_parser.Models.Connections
{
    public class ConnectionNode
    {
        public string Area { get; set; }

        public string Subarea { get; set; }

        public int Roomid { get; set; }

        public string RoomName { get; set; }

        public int Nodeid { get; set; }

        public ConnectionNodePositionEnum Position { get; set; }

        // STITCHME Note?
    }
}
