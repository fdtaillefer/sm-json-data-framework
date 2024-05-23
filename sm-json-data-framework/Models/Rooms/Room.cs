using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    /// <summary>
    /// A contiguous portion of the game world that can be navigated without going through a door transition.
    /// </summary>
    public class Room : AbstractModelElement<UnfinalizedRoom, Room>
    {
        public Room(UnfinalizedRoom sourceElement, Action<Room> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Id = sourceElement.Id;
            Name = sourceElement.Name;
            Area = sourceElement.Area;
            Subarea = sourceElement.Subarea;
            Playable = sourceElement.Playable;
            RoomAddress = sourceElement.RoomAddress;
            RoomEnvironments = sourceElement.RoomEnvironments.Select(environment => environment.Finalize(mappings)).ToList().AsReadOnly();
            Nodes = sourceElement.Nodes.Values.Select(node => node.Finalize(mappings)).ToDictionary(node => node.Id).AsReadOnly();
            Links = sourceElement.Links.Values.Select(link => link.Finalize(mappings)).ToDictionary(link => link.FromNode.Id).AsReadOnly();
            Obstacles = sourceElement.Obstacles.Values.Select(obstacle => obstacle.Finalize(mappings)).ToDictionary(obstacle => obstacle.Id).AsReadOnly();
            Enemies = sourceElement.Enemies.Values.Select(roomEnemy => roomEnemy.Finalize(mappings)).ToDictionary(roomEnemy => roomEnemy.Id).AsReadOnly();
        }

        /// <summary>
        /// An arbitrary, numerical ID that can be used to identify this Room.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// A human-legible name that uniquely identifies this Room. 
        /// Room names typically come from the community and not any official source.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The name of the in-game area this Room is in, e.g. Brinstar.
        /// </summary>
        public string Area { get; }

        /// <summary>
        /// The name of the sub-area (within the <see cref="Area"/>) that this Room is in. 
        /// Sub-areas are not officially defined in-game.
        /// </summary>
        public string Subarea { get; }

        /// <summary>
        /// Whether player inputs work while in this room. 
        /// </summary>
        public bool Playable { get; }

        /// <summary>
        /// The in-game address of this Room.
        /// </summary>
        public string RoomAddress { get; }

        /// <summary>
        /// The list of RoomEnvironments that can affect this Room.
        /// Typically there is only one, but sometimes there are several and the one that is active depends on through which node the room was entered.
        /// </summary>
        public IReadOnlyList<RoomEnvironment> RoomEnvironments { get; }

        /// <summary>
        /// The nodes in this room, mapped by in-room numerical id
        /// </summary>
        public IReadOnlyDictionary<int, RoomNode> Nodes { get; }

        /// <summary>
        /// The links in this room mapped by their origin node ID.
        /// </summary>
        public IReadOnlyDictionary<int, Link> Links { get; }

        /// <summary>
        /// The obstacles that are in this room, mapped by Id.
        /// </summary>
        public IReadOnlyDictionary<string, RoomObstacle> Obstacles { get; }

        /// <summary>
        /// The groups of enemies in this room, mapped their by in-room id
        /// </summary>
        public IReadOnlyDictionary<string, RoomEnemy> Enemies { get; }

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

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            foreach (RoomEnvironment roomEnvironment in RoomEnvironments)
            {
                roomEnvironment.ApplyLogicalOptions(logicalOptions, rules);
            }

            foreach (RoomNode node in Nodes.Values)
            {
                node.ApplyLogicalOptions(logicalOptions, rules);
            }

            foreach (RoomObstacle obstacle in Obstacles.Values)
            {
                obstacle.ApplyLogicalOptions(logicalOptions, rules);
            }

            foreach (Link link in Links.Values)
            {
                link.ApplyLogicalOptions(logicalOptions, rules);
            }

            foreach (RoomEnemy enemy in Enemies.Values)
            {
                enemy.ApplyLogicalOptions(logicalOptions, rules);
            }
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidRules rules)
        {
            // A room always has relevance
            return true;
        }
    }

    public class UnfinalizedRoom : AbstractUnfinalizedModelElement<UnfinalizedRoom, Room>, InitializablePostDeserializeOutOfRoom
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Area { get; set; }

        public string Subarea { get; set; }

        public bool Playable { get; set; }

        public string RoomAddress { get; set; }

        public IList<UnfinalizedRoomEnvironment> RoomEnvironments { get; set; } = new List<UnfinalizedRoomEnvironment>();

        /// <summary>
        /// The nodes in this room, mapped by in-room numerical id
        /// </summary>
        public IDictionary<int, UnfinalizedRoomNode> Nodes { get; set; } = new Dictionary<int, UnfinalizedRoomNode>();

        /// <summary>
        /// The links in this room mapped by their origin node ID.
        /// </summary>
        public IDictionary<int, UnfinalizedLink> Links { get; set; } = new Dictionary<int, UnfinalizedLink>();

        /// <summary>
        /// The obstacles that are in this room, mapped by Id.
        /// </summary>
        public IDictionary<string, UnfinalizedRoomObstacle> Obstacles { get; set; } = new Dictionary<string, UnfinalizedRoomObstacle>();

        /// <summary>
        /// The groups of enemies in this room, mapped their by in-room id
        /// </summary>
        public IDictionary<string, UnfinalizedRoomEnemy> Enemies { get; set; } = new Dictionary<string, UnfinalizedRoomEnemy>();

        // Is this really needed? Should probably remove this later
        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel)"/> has been called.</para>
        /// <para>The SuperMetroidModel that this room is a part of.</para>
        /// </summary>
        public UnfinalizedSuperMetroidModel SuperMetroidModel { get; set; }

        public UnfinalizedRoom()
        {

        }

        public UnfinalizedRoom(RawRoom rawRoom, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            Id = rawRoom.Id;
            Name = rawRoom.Name;
            Area = rawRoom.Area;
            Subarea = rawRoom.Subarea;
            Playable = rawRoom.Playable;
            RoomAddress = rawRoom.RoomAddress;
            RoomEnvironments = rawRoom.RoomEnvironments.Select(rawEnvironment => new UnfinalizedRoomEnvironment(rawEnvironment)).ToList();
            Nodes = rawRoom.Nodes.Select(rawNode => new UnfinalizedRoomNode(rawNode, knowledgeBase)).ToDictionary(node => node.Id);
            Links = rawRoom.Links.Select(rawLink => new UnfinalizedLink(rawLink, knowledgeBase)).ToDictionary(link => link.FromNodeId);
            Obstacles = rawRoom.Obstacles.Select(rawObstacle => new UnfinalizedRoomObstacle(rawObstacle, knowledgeBase)).ToDictionary(obstacle => obstacle.Id);
            Enemies = rawRoom.Enemies.Select(rawRoomEnemy => new UnfinalizedRoomEnemy(rawRoomEnemy, knowledgeBase)).ToDictionary(roomEnemy =>  roomEnemy.Id);
        }

        protected override Room CreateFinalizedElement(UnfinalizedRoom sourceElement, Action<Room> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new Room(sourceElement, mappingsInsertionCallback, mappings);
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model)
        {
            SuperMetroidModel = model;

            foreach (UnfinalizedRoomEnvironment roomEnvironment in RoomEnvironments)
            {
                roomEnvironment.InitializeProperties(model, this);
            }

            foreach (UnfinalizedRoomNode node in Nodes.Values)
            {
                node.InitializeProperties(model, this);
            }

            foreach (UnfinalizedRoomObstacle obstacle in Obstacles.Values)
            {
                obstacle.InitializeProperties(model, this);
            }

            foreach (UnfinalizedLink link in Links.Values)
            {
                link.InitializeProperties(model, this);
            }

            foreach (UnfinalizedRoomEnemy enemy in Enemies.Values)
            {
                enemy.InitializeProperties(model, this);
            }
        }

        /// <summary>
        /// Returns the LinkTo that describes navigation from fromNodeId to toNodeId, if it exists.
        /// </summary>
        /// <param name="fromNodeId">ID of the origin node</param>
        /// <param name="toNodeId">ID of the destination node</param>
        /// <returns>The LinkTo, or null if not found.</returns>
        public UnfinalizedLinkTo GetLinkBetween(int fromNodeId, int toNodeId)
        {
            if (Links.TryGetValue(fromNodeId, out UnfinalizedLink link))
            {
                if (link.To.TryGetValue(toNodeId, out UnfinalizedLinkTo linkTo))
                {
                    return linkTo;
                }
            }
            return null;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model)
        {
            List<string> unhandled = new List<string>();

            foreach (UnfinalizedRoomEnvironment roomEnvironment in RoomEnvironments)
            {
                unhandled.AddRange(roomEnvironment.InitializeReferencedLogicalElementProperties(model, this));
            }

            foreach (UnfinalizedRoomNode node in Nodes.Values)
            {
                unhandled.AddRange(node.InitializeReferencedLogicalElementProperties(model, this));
            }

            foreach(UnfinalizedLink link in Links.Values)
            {
                unhandled.AddRange(link.InitializeReferencedLogicalElementProperties(model, this));
            }

            foreach (UnfinalizedRoomEnemy enemy in Enemies.Values)
            {
                unhandled.AddRange(enemy.InitializeReferencedLogicalElementProperties(model, this));
            }

            foreach(UnfinalizedRoomObstacle obstacle in Obstacles.Values)
            {
                unhandled.AddRange(obstacle.InitializeReferencedLogicalElementProperties(model, this));
            }

            return unhandled.Distinct();
        }
    }
}
