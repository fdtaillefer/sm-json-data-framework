using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Node.NodeSparking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class RoomNode
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public NodeTypeEnum NodeType { get; set; }

        public NodeSubTypeEnum NodeSubType { get; set; }

        public string NodeItem { get; set; }

        public string NodeAddress { get; set; }

        public LogicalRequirements InteractionRequires { get; set; }

        public IEnumerable<Runway> Runways { get; set; } = Enumerable.Empty<Runway>();

        public IEnumerable<CanLeaveCharged> CanLeaveCharged { get; set; } = Enumerable.Empty<CanLeaveCharged>();

        // Decide who should handle null here
        [JsonPropertyName("spawnAt")]
        public int? OverrideSpawnAtNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>The node referenced by the <see cref="OverrideSpawnAtNodeId"/> property, if any.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode OverrideSpawnAtNode { get; set; }

        /// <summary>
        /// <para>Not reliable before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>The node at which Samus actually spawns upon entering the room via this node. In most cases it will be this node, but not always.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode SpawnAtNode { get { return OverrideSpawnAtNode ?? this; } }

        public IEnumerable<NodeLock> Locks { get; set; } = Enumerable.Empty<NodeLock>();

        public IEnumerable<UtilityEnum> Utility { get; set; } = Enumerable.Empty<UtilityEnum>();

        public IEnumerable<ViewableNode> ViewableNodes { get; set; } = Enumerable.Empty<ViewableNode>();

        [JsonPropertyName("yields")]
        public IEnumerable<string> YieldsStrings { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The game flags that are activated by interacting with this node.</para>
        /// </summary>
        [JsonIgnore]
        public IEnumerable<GameFlag> Yields { get; set; }

        // STITCHME Note?

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>The room in which this node is.</para>
        /// </summary>
        [JsonIgnore]
        public Room Room { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>If this node is a way out of the room, this is the connection that connects this node to its destination.</para>
        /// </summary>
        [JsonIgnore]
        public Connection OutConnection { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>If this node is a way out of the room, this is the node that leaving the room via this node leads to.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode OutNode { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>Contains all in-room links from this node to another, mapped by the destination node ID</para>
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, LinkTo> Links { get; set; }

        /// <summary>
        /// Initializes additional properties in this RoomNode, which wouldn't be initialized by simply parsing a rooms json file.
        /// All such properties are identified in their own documentation and should not be read if this method isn't called.
        /// </summary>
        /// <param name="model">The model to use to initialize the additional properties</param>
        /// <param name="room">The room in which this node is</param>
        public void Initialize(SuperMetroidModel model, Room room)
        {
            Room = room;

            // Initialize OutConnection and OutNode
            if(NodeType == NodeTypeEnum.Exit || NodeType == NodeTypeEnum.Door)
            {
                if (model.Connections.TryGetValue($"{Room.Name}_{Id}", out Connection connection))
                {
                    OutConnection = connection;
                    ConnectionNode otherNode = connection.Nodes.Where(n => n.RoomName == Room.Name && n.Nodeid == Id).Single();
                    OutNode = model.Rooms[otherNode.RoomName].Nodes[otherNode.Nodeid];
                }
            }

            // Initialize OverrideSpawnAtNode
            if (OverrideSpawnAtNodeId != null)
            {
                OverrideSpawnAtNode = Room.Nodes[(int)OverrideSpawnAtNodeId];
            }

            // Initialize CanLeaveCharged objects
            foreach (CanLeaveCharged canLeaveCharged in CanLeaveCharged)
            {
                canLeaveCharged.Initialize(model, this);
            }

            // Initialize Yielded game flags
            Yields = YieldsStrings.Select(s => model.GameFlags[s]);

            // Initialize Links
            IEnumerable<Link> linksFromHere = room.Links.Where(l => l.FromNodeId == Id);
            // One Link object contains all links from a given node. It's possible not to find one for this node, but there should never be more than one.
            if (linksFromHere.Any())
            {
                Links = linksFromHere.Single().To.ToDictionary(l => l.TargetNodeId);
            }
        }
    }
}
