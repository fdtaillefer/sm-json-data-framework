using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Raw.Rooms.Nodes;

namespace sm_json_data_framework.Models.Raw.Rooms
{
    public class RawRoom
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Area { get; set; }

        public string Subarea { get; set; }

        public bool Playable { get; set; }

        public string RoomAddress { get; set; }

        public IList<RawRoomEnvironment> RoomEnvironments { get; set; } = new List<RawRoomEnvironment>();

        public IList<RawRoomNode> Nodes { get; set; } = new List<RawRoomNode>();

        public IList<RawLink> Links { get; set; } = new List<RawLink>();

        public IList<RawRoomObstacle> Obstacles { get; set; } = new List<RawRoomObstacle>();

        public IList<RawRoomEnemy> Enemies { get; set; } = new List<RawRoomEnemy>();
    }
}
