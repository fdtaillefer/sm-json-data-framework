using sm_json_data_framework.Models.Raw.Connections;
using sm_json_data_framework.Options;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Connections
{
    public class ConnectionNode: AbstractModelElement
    {
        public string Area { get; set; }

        public string Subarea { get; set; }

        public int Roomid { get; set; }

        public string RoomName { get; set; }

        public int Nodeid { get; set; }

        public string NodeName { get; set; }

        public ConnectionNodePositionEnum Position { get; set; }

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

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Logical Options have no power here
            return false;
        }

        /// <summary>
        /// <para>A string that identifies this node, often used as a key in Dictionaries.</para>
        /// </summary>
        [JsonIgnore]
        public string IdentifyingString { get => SuperMetroidUtils.BuildNodeIdentifyingString(RoomName, Nodeid); }
    }
}
