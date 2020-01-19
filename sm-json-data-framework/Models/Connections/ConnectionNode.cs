using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Connections
{
    public class ConnectionNode
    {
        public string Area { get; set; }

        public string Subarea { get; set; }

        public int Roomid { get; set; }

        public string RoomName { get; set; }

        /// <summary>
        /// <para>A string that identifies this node, often used as a key in Dictionaries.</para>
        /// </summary>
        [JsonIgnore]
        public string IdentifyingString { get => SuperMetroidUtils.BuildNodeIdentifyingString(RoomName, Nodeid); }

        public int Nodeid { get; set; }

        public ConnectionNodePositionEnum Position { get; set; }
    }
}
