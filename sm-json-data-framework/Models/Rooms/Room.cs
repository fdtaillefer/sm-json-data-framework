using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class Room
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Area { get; set; }

        public string Subarea { get; set; }

        // STITCHME Note is string or array - how would we want to handle this? It's not important for now anyway

        public string RoomAddress { get; set; }

        [JsonPropertyName("nodes")]
        public IEnumerable<RoomNode> NodesSequence { get; set; }

        private IDictionary<int, RoomNode> _nodesDictionary;
        /// <summary>
        /// The nodes in this room, mapped by id
        /// </summary>
        [JsonIgnore]
        public IDictionary<int, RoomNode> Nodes { get 
            {
                if (_nodesDictionary == null)
                {
                    _nodesDictionary = NodesSequence.ToDictionary(n => n.Id);
                }
                return _nodesDictionary;
            }
        }

        public IEnumerable<Link> Links { get; set; } = Enumerable.Empty<Link>();

        public IEnumerable<RoomObstacle> Obstacles { get; set; } = Enumerable.Empty<RoomObstacle>();

        public IEnumerable<RoomEnemy> Enemies { get; set; } = Enumerable.Empty<RoomEnemy>();
    }
}
