using sm_json_data_framework.Models.Raw.Connections;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Connections
{
    public class ConnectionNode : RawConnectionNode
    {
        public ConnectionNode()
        {

        }

        public ConnectionNode(RawConnectionNode rawNode)
        {
            Area = rawNode.Area;
            Subarea = rawNode.Subarea;
            Roomid = rawNode.Roomid;
            RoomName = rawNode.RoomName;
            Nodeid = rawNode.Nodeid;
            NodeName = rawNode.NodeName;
            Position = rawNode.Position;
    }

        /// <summary>
        /// <para>A string that identifies this node, often used as a key in Dictionaries.</para>
        /// </summary>
        [JsonIgnore]
        public string IdentifyingString { get => SuperMetroidUtils.BuildNodeIdentifyingString(RoomName, Nodeid); }
    }
}
