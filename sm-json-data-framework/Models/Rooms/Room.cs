﻿using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class Room : InitializablePostDeserializeOutOfRoom
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Area { get; set; }

        public string Subarea { get; set; }

        public bool Playable { get; set; }

        public string RoomAddress { get; set; }

        /// <summary>
        /// The raw sequence of nodes in this room
        /// </summary>
        [JsonPropertyName("nodes")]
        public IEnumerable<RoomNode> NodesSequence { get; set; }

        public IEnumerable<RoomEnvironment> RoomEnvironments { get; set; } = Enumerable.Empty<RoomEnvironment>();

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

        [JsonPropertyName("enemies")]
        public IEnumerable<RoomEnemy> EnemiesSequence { get; set; } = Enumerable.Empty<RoomEnemy>();

        private IDictionary<string, RoomEnemy> _enemiesDictionary;
        /// <summary>
        /// The groups of enemies in this room, mapped by id
        /// </summary>
        [JsonIgnore]
        public IDictionary<string, RoomEnemy> Enemies
        {
            get
            {
                if (_enemiesDictionary == null)
                {
                    _enemiesDictionary = EnemiesSequence.ToDictionary(e => e.Id);
                }
                return _enemiesDictionary;
            }
        }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>The SuperMetroidModel that this room is a part of.</para>
        /// </summary>
        [JsonIgnore]
        public SuperMetroidModel SuperMetroidModel { get; set; }

        public void Initialize(SuperMetroidModel model)
        {
            SuperMetroidModel = model;

            List<Action> postInitializationCallbacks = new List<Action>();

            foreach( RoomEnvironment roomEnvironment in RoomEnvironments)
            {
                postInitializationCallbacks.AddRange(roomEnvironment.Initialize(model, this));
            }

            foreach(RoomNode node in Nodes.Values)
            {
                postInitializationCallbacks.AddRange(node.Initialize(model, this));
            }

            foreach(RoomObstacle obstacle in Obstacles.Values)
            {
                postInitializationCallbacks.AddRange(obstacle.Initialize(model, this));
            }

            foreach (Link link in Links)
            {
                postInitializationCallbacks.AddRange(link.Initialize(model, this));
            }

            foreach(RoomEnemy enemy in EnemiesSequence)
            {
                postInitializationCallbacks.AddRange(enemy.Initialize(model, this));
            }

            // If we received any callbacks to execute after the room is done, execute them now
            postInitializationCallbacks.ForEach(action => action.Invoke());
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model)
        {
            List<string> unhandled = new List<string>();

            foreach (RoomEnvironment roomEnvironment in RoomEnvironments)
            {
                unhandled.AddRange(roomEnvironment.InitializeReferencedLogicalElementProperties(model, this));
            }

            foreach (RoomNode node in Nodes.Values)
            {
                unhandled.AddRange(node.InitializeReferencedLogicalElementProperties(model, this));
            }

            foreach(Link link in Links)
            {
                unhandled.AddRange(link.InitializeReferencedLogicalElementProperties(model, this));
            }

            foreach (RoomEnemy enemy in EnemiesSequence)
            {
                unhandled.AddRange(enemy.InitializeReferencedLogicalElementProperties(model, this));
            }

            foreach(RoomObstacle obstacle in Obstacles.Values)
            {
                unhandled.AddRange(obstacle.InitializeReferencedLogicalElementProperties(model, this));
            }

            return unhandled.Distinct();
        }
    }
}
