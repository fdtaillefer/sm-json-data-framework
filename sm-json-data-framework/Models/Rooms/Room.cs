using sm_json_data_framework.Models.Rooms.Nodes;
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

        public IEnumerable<RoomEnvironment> RoomEnvironments { get; set; } = Enumerable.Empty<RoomEnvironment>();

        /// <summary>
        /// The nodes in this room, mapped by in-room numerical id
        /// </summary>
        public IDictionary<int, RoomNode> Nodes { get; set; } = new Dictionary<int, RoomNode>();

        /// <summary>
        /// The links in this room mapped by their origin node ID.
        /// </summary>
        public IDictionary<int, Link> Links { get; set; } = new Dictionary<int, Link>();

        public IDictionary<string, RoomObstacle> Obstacles { get; set; } = new Dictionary<string, RoomObstacle>();

        /// <summary>
        /// The groups of enemies in this room, mapped by id
        /// </summary>
        public IDictionary<string, RoomEnemy> Enemies { get; set; } = new Dictionary<string, RoomEnemy>();

        // Is this really needed? Should probably remove this later
        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>The SuperMetroidModel that this room is a part of.</para>
        /// </summary>
        [JsonIgnore]
        public SuperMetroidModel SuperMetroidModel { get; set; }

        /// <summary>
        /// Returns the LinkTo that describes navigation from fromNodeId to toNodeId, if it exists.
        /// </summary>
        /// <param name="fromNodeId">ID of the origin node</param>
        /// <param name="toNodeId">ID of the destination node</param>
        /// <returns>The LinkTo, or null if not found.</returns>
        public LinkTo GetLinkBetween(int fromNodeId, int toNodeId)
        {
            if (Links.TryGetValue(fromNodeId, out Link link))
            {
                if (link.To.TryGetValue(toNodeId, out LinkTo linkTo))
                {
                    return linkTo;
                }
            }
            return null;
        }

        public void InitializeProperties(SuperMetroidModel model)
        {
            SuperMetroidModel = model;

            foreach (RoomEnvironment roomEnvironment in RoomEnvironments)
            {
                roomEnvironment.InitializeProperties(model, this);
            }

            foreach (RoomNode node in Nodes.Values)
            {
                node.InitializeProperties(model, this);
            }

            foreach (RoomObstacle obstacle in Obstacles.Values)
            {
                obstacle.InitializeProperties(model, this);
            }

            foreach (Link link in Links.Values)
            {
                link.InitializeProperties(model, this);
            }

            foreach (RoomEnemy enemy in Enemies.Values)
            {
                enemy.InitializeProperties(model, this);
            }
        }

        public bool CleanUpUselessValues(SuperMetroidModel model)
        {
            RoomEnvironments = RoomEnvironments.Where(roomEnvironment => roomEnvironment.CleanUpUselessValues(model, this));

            Nodes = Nodes.Where(kvp => kvp.Value.CleanUpUselessValues(model, this)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            Obstacles = Obstacles.Where(kvp => kvp.Value.CleanUpUselessValues(model, this)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            Links = Links.Where(kvp => kvp.Value.CleanUpUselessValues(model, this)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            Enemies = Enemies.Where(kvp => kvp.Value.CleanUpUselessValues(model, this)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // A room is never useless
            return true;
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

            foreach(Link link in Links.Values)
            {
                unhandled.AddRange(link.InitializeReferencedLogicalElementProperties(model, this));
            }

            foreach (RoomEnemy enemy in Enemies.Values)
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
