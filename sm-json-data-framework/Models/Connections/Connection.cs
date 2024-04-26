using sm_json_data_framework.Models.Raw.Connections;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Connections
{
    /// <summary>
    /// Description of a one-way game transition from an origin node to a destination node.
    /// Contains the nodes as they are described in the json model.
    /// </summary>
    public class Connection
    {
        public ConnectionTypeEnum ConnectionType { get; set; }

        public ConnectionNode FromNode { get; set; }

        public ConnectionNode ToNode { get; set; }

        public Connection() { 

        }

        public Connection(RawConnection rawConnection, RawConnectionNode fromNode, RawConnectionNode toNode)
        {
            ConnectionType = rawConnection.ConnectionType;
            FromNode = new ConnectionNode(fromNode);
            ToNode = new ConnectionNode(toNode);
        }
    }
}
