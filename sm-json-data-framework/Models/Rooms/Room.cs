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

        /// <summary>
        /// The raw sequence of nodes in this room
        /// </summary>
        [JsonPropertyName("nodes")]
        public IEnumerable<RoomNode> NodesSequence { get; set; }

        private IDictionary<int, RoomNode> _nodesDictionary;
        /// <summary>
        /// The nodes in this room, mapped by id
        /// </summary>
        [JsonIgnore]
        public IDictionary<int, RoomNode> Nodes {
            get 
            {
                if (_nodesDictionary == null)
                {
                    _nodesDictionary = NodesSequence.ToDictionary(n => n.Id);
                }
                return _nodesDictionary;
            }
        }

        public IEnumerable<Link> Links { get; set; } = Enumerable.Empty<Link>();

        [JsonPropertyName("obstacles")]
        public IEnumerable<RoomObstacle> ObstaclesSequence { get; set; } = Enumerable.Empty<RoomObstacle>();

        private IDictionary<string, RoomObstacle> _obstaclesDictionary;
        /// <summary>
        /// The obstacles in this room, mapped by id
        /// </summary>
        [JsonIgnore]
        public IDictionary<string, RoomObstacle> Obstacles {

            get
            {
                if (_obstaclesDictionary == null)
                {
                    _obstaclesDictionary = ObstaclesSequence.ToDictionary(o => o.Id);
                }
                return _obstaclesDictionary;
            }
                
        }

        public IEnumerable<RoomEnemy> Enemies { get; set; } = Enumerable.Empty<RoomEnemy>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>The SuperMetroidModel that this room is a part of.</para>
        /// </summary>
        [JsonIgnore]
        public SuperMetroidModel SuperMetroidModel { get; set; }

        /// <summary>
        /// Initializes additional properties in this Room, which wouldn't be initialized by simply parsing a rooms json file.
        /// All such properties are identified in their own documentation and should not be read if this method isn't called.
        /// </summary>
        /// <param name="model">The model to use to initialize the additional properties</param>
        public void Initialize(SuperMetroidModel model)
        {
            SuperMetroidModel = model;

            foreach(RoomNode node in Nodes.Values)
            {
                node.Initialize(model, this);
            }

            foreach(RoomObstacle obstacle in Obstacles.Values)
            {
                obstacle.Initialize(model, this);
            }

            foreach (LinkTo linkTo in Links.SelectMany(l => l.To))
            {
                linkTo.Initialize(model, this);
            }

            foreach(RoomEnemy enemy in Enemies)
            {
                enemy.Initialize(model, this);
            }
        }
    }
}
