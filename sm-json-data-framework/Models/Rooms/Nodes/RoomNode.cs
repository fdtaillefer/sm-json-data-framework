using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Node;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class RoomNode : InitializablePostDeserializeInRoom
    {
        public int Id { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>A string that identifies this node, often used as a key in Dictionaries.</para>
        /// </summary>
        [JsonIgnore]
        public string IdentifyingString { get => SuperMetroidUtils.BuildNodeIdentifyingString(Room.Name, Id); }

        public string Name { get; set; }

        public NodeTypeEnum NodeType { get; set; }

        public NodeSubTypeEnum NodeSubType { get; set; }

        [JsonPropertyName("nodeItem")]
        public string NodeItemName { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The item that can be obtained by interacting with this node (if any).</para>
        /// </summary>
        [JsonIgnore]
        public Item NodeItem { get; set; }

        public string NodeAddress { get; set; }

        public LogicalRequirements InteractionRequires { get; set; } = new LogicalRequirements();

        public IEnumerable<Runway> Runways { get; set; } = Enumerable.Empty<Runway>();

        public IEnumerable<CanLeaveCharged> CanLeaveCharged { get; set; } = Enumerable.Empty<CanLeaveCharged>();

        // Decide who should handle null here
        [JsonPropertyName("spawnAt")]
        public int? OverrideSpawnAtNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
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

        [JsonIgnore]
        public IEnumerable<string> Note { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The room in which this node is.</para>
        /// </summary>
        [JsonIgnore]
        public Room Room { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>If this node is a way out of the room, this is the one-way connection that connects this node to its destination.</para>
        /// </summary>
        [JsonIgnore]
        public Connection OutConnection { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>If this node is a way out of the room, this is the node that leaving the room via this node leads to.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode OutNode { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>Contains all in-room links from this node to another, mapped by the destination node ID</para>
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, LinkTo> Links { get; set; }

        public void Initialize(SuperMetroidModel model, Room room)
        {
            Room = room;

            // Initialize OutConnection and OutNode
            if(NodeType == NodeTypeEnum.Exit || NodeType == NodeTypeEnum.Door)
            {
                if (model.Connections.TryGetValue(IdentifyingString, out Connection connection))
                {
                    OutConnection = connection;
                    ConnectionNode otherNode = connection.ToNode;
                    OutNode = model.Rooms[otherNode.RoomName].Nodes[otherNode.Nodeid];
                }
            }

            // Initialize OverrideSpawnAtNode
            if (OverrideSpawnAtNodeId != null)
            {
                OverrideSpawnAtNode = Room.Nodes[(int)OverrideSpawnAtNodeId];
            }

            // Initialize Yielded game flags
            Yields = YieldsStrings.Select(s => model.GameFlags[s]);

            // Initialize item
            if (NodeItemName != null)
            {
                NodeItem = model.Items[NodeItemName];
            }

            // Initialize Links
            IEnumerable<Link> linksFromHere = room.Links.Where(l => l.FromNodeId == Id);
            // One Link object contains all links from a given node. It's possible not to find one for this node, but there should never be more than one.
            if (linksFromHere.Any())
            {
                Links = linksFromHere.Single().To.ToDictionary(l => l.TargetNodeId);
            }

            // Initialize CanLeaveCharged objects
            foreach (CanLeaveCharged canLeaveCharged in CanLeaveCharged)
            {
                canLeaveCharged.Initialize(model, room, this);
            }

            // Initialize ViewableNodes
            foreach(ViewableNode viewableNode in ViewableNodes)
            {
                viewableNode.Initialize(model, room, this);
            }

            // Initialize Locks
            foreach(NodeLock nodeLock in Locks)
            {
                nodeLock.Initialize(model, room, this);
            }

            // Initialize Runways
            foreach(Runway runway in Runways)
            {
                runway.Initialize(model, room, this);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(InteractionRequires.InitializeReferencedLogicalElementProperties(model, room));

            foreach(Runway runway in Runways)
            {
                unhandled.AddRange(runway.InitializeReferencedLogicalElementProperties(model, room, this));
            }

            foreach(CanLeaveCharged canLeaveCharged in CanLeaveCharged)
            {
                unhandled.AddRange(canLeaveCharged.InitializeReferencedLogicalElementProperties(model, room, this));
            }

            foreach(NodeLock nodeLock in Locks)
            {
                unhandled.AddRange(nodeLock.InitializeReferencedLogicalElementProperties(model, room, this));
            }

            foreach(ViewableNode viewableNode in ViewableNodes)
            {
                unhandled.AddRange(viewableNode.InitializeReferencedLogicalElementProperties(model, room, this));
            }

            return unhandled.Distinct();
        }
    }
}
