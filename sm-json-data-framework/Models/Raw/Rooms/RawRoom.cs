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

        public IEnumerable<RawRoomEnvironment> RoomEnvironments { get; set; } = Enumerable.Empty<RawRoomEnvironment>();

        public IEnumerable<RawRoomNode> Nodes { get; set; } = Enumerable.Empty<RawRoomNode>();

        public IEnumerable<RawLink> Links { get; set; } = Enumerable.Empty<RawLink>();

        public IEnumerable<RawRoomObstacle> Obstacles { get; set; } = Enumerable.Empty<RawRoomObstacle>();

        public IEnumerable<RawRoomEnemy> Enemies { get; set; } = Enumerable.Empty<RawRoomEnemy>();
    }
}
