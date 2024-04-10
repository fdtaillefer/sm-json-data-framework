using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
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

        public IEnumerable<DoorEnvironment> DoorEnvironments { get; set; } = Enumerable.Empty<DoorEnvironment>();

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

        /// <summary>
        /// Whether entering a room at this node effectively spawns Samus at a different node.
        /// </summary>
        public bool SpawnsAtDifferentNode { get { return SpawnAtNode != this; } }

        public IDictionary<string, NodeLock> Locks { get; set; } = new Dictionary<string, NodeLock>();

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

        public IEnumerable<TwinDoorAddress> TwinDoorAddresses { get; set; } = Enumerable.Empty<TwinDoorAddress>();

        /// <summary>
        /// Returns the enumeration of locks on this node that are active and locked, according to the provided inGameState.
        /// </summary>
        /// <param name="inGameState">InGameState to evaluate for active locks</param>
        /// <returns></returns>
        public IEnumerable<NodeLock> GetActiveLocks(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            // Return locks whose locking conditions have been met, and that haven't been opened
            return Locks.Values.Where(nodeLock => nodeLock.Lock.Execute(model, inGameState) != null)
                .Where(nodeLock => !inGameState.OpenedLocks.ContainsLock(nodeLock))
                .ToList();
        }

        public IEnumerable<Action> Initialize(SuperMetroidModel model, Room room)
        {
            Room = room;
            List<Action> initializedRoomCallbacks = new List<Action>();

            // Initialize OutConnection and OutNode
            if(NodeType == NodeTypeEnum.Exit || NodeType == NodeTypeEnum.Door)
            {
                if (model.Connections.TryGetValue(IdentifyingString, out Connection connection))
                {
                    OutConnection = connection;
                    ConnectionNode otherNode = connection.ToNode;
                    OutNode = model.Rooms[otherNode.RoomName].Nodes[otherNode.Nodeid];

                    // Sanity check for node names and IDs. Take this out if nodeIds get taken out of connections
                    if (connection.FromNode.NodeName != null && connection.FromNode.NodeName != Name)
                    {
                        throw new Exception($"A connection thought to be going from node {Name} claims to be going from a node named {connection.FromNode.NodeName}");
                    }
                    if (connection.ToNode.NodeName != null && connection.ToNode.NodeName != OutNode.Name)
                    {
                        throw new Exception($"A connection thought to be going from node {OutNode.Name} claims to be going from a node named {connection.ToNode.NodeName}");
                    }

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

            // We can't initialize CanLeaveChargeds now because they need the entire room to be loaded first.
            // So we'll do it in a callback that we'll return, to be executed after the rest of the room is initialized.
            initializedRoomCallbacks.Add(() => {
                foreach(CanLeaveCharged canLeaveCharged in CanLeaveCharged)
                {
                    canLeaveCharged.Initialize(model, room, this);
                }
                // We are now able to eliminate any CanLeaveCharged that is initiated remotely and whose path is impossible to follow.
                // (The PathToDoor is expected to be left empty if it's impossible to follow due to strats not being possible given the applied settings)
                CanLeaveCharged = CanLeaveCharged.Where(clc => clc.InitiateRemotely == null || clc.InitiateRemotely.PathToDoor.Any());
            });

            // We can't initialize DoorEnvironments now because they reference other nodes.
            // So we'll do it in a callback that we'll return, to be executed after the rest of the room is initialized.
            initializedRoomCallbacks.Add(() => {
                foreach (DoorEnvironment doorEnvironment in DoorEnvironments)
                {
                    doorEnvironment.Initialize(model, room, this);
                }
            });

            // Initialize ViewableNodes
            foreach (ViewableNode viewableNode in ViewableNodes)
            {
                viewableNode.Initialize(model, room, this);
            }

            // Initialize Locks
            foreach(NodeLock nodeLock in Locks.Values)
            {
                nodeLock.Initialize(model, room, this);
            }

            // Initialize Runways
            foreach(Runway runway in Runways)
            {
                runway.Initialize(model, room, this);
            }

            return initializedRoomCallbacks;
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

            foreach(NodeLock nodeLock in Locks.Values)
            {
                unhandled.AddRange(nodeLock.InitializeReferencedLogicalElementProperties(model, room, this));
            }

            foreach(ViewableNode viewableNode in ViewableNodes)
            {
                unhandled.AddRange(viewableNode.InitializeReferencedLogicalElementProperties(model, room, this));
            }

            return unhandled.Distinct();
        }

        IExecutable _interactExecution = null;
        /// <summary>
        /// An IExecutable that corresponds to interacting with this node.
        /// </summary>
        public IExecutable InteractExecution
        {
            get
            {
                if (_interactExecution == null)
                {
                    _interactExecution = new InteractExecution(this);
                }
                return _interactExecution;
            }
        }
    }
    /// <summary>
    /// A class that encloses the opening of a NodeLock in an IExecutable interface. 
    /// Note that if this picks up a resource item, it does not apply any change to current resources, 
    /// in an attempt to avoid applying non-repeatable changes that could have logical implications.
    /// </summary>
    internal class InteractExecution : IExecutable
    {
        private RoomNode Node { get; set; }

        public InteractExecution(RoomNode node)
        {
            Node = node;
        }

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // First thing is making sure no locks prevent interaction
            IEnumerable<NodeLock> bypassedLocks = inGameState.GetBypassedExitLocks(previousRoomCount);
            IEnumerable<NodeLock> unhandledLocks = Node.GetActiveLocks(model, inGameState)
                .Where(activeLock => !bypassedLocks.Contains(activeLock, ObjectReferenceEqualityComparer<NodeLock>.Default));

            // Can't interact with the node if there's active locks that haven't been opened or bypassed
            if (unhandledLocks.Any())
            {
                return null;
            }

            // Locks are ok, let's try to actually interact with the node

            // Start by executing the node's interaction requirements
            ExecutionResult result = Node.InteractionRequires.Execute(model, inGameState, times, previousRoomCount);

            // Give up if interaction requirements couldn't be met
            if(result == null)
            {
                return null;
            }

            // Actually interact with the node now


            // Activate game flags
            foreach (GameFlag flag in Node.Yields)
            {
                result.ApplyActivatedGameFlag(flag);
            }

            // Take item at location
            if (Node.NodeItem != null && !inGameState.TakenItemLocations.ContainsNode(Node))
            {
                result.ResultingState.ApplyTakeLocation(Node);
                result.ResultingState.ApplyAddItem(Node.NodeItem);
            }

            // Use any refill utility
            foreach (UtilityEnum utility in Node.Utility)
            {
                switch (utility)
                {
                    case UtilityEnum.Energy:
                        result.ResultingState.ApplyRefillResource(RechargeableResourceEnum.RegularEnergy);
                        break;
                    case UtilityEnum.Reserve:
                        result.ResultingState.ApplyRefillResource(RechargeableResourceEnum.ReserveEnergy);
                        break;
                    case UtilityEnum.Missile:
                        result.ResultingState.ApplyRefillResource(RechargeableResourceEnum.Missile);
                        break;
                    case UtilityEnum.Super:
                        result.ResultingState.ApplyRefillResource(RechargeableResourceEnum.Super);
                        break;
                    case UtilityEnum.PowerBomb:
                        result.ResultingState.ApplyRefillResource(RechargeableResourceEnum.PowerBomb);
                        break;
                    // Other utilities don't do anything for us
                    default:
                        break;
                }
            }

            // Use node to exit the room
            if (Node.OutNode != null)
            {
                result.ResultingState.ApplyEnterRoom(Node.OutNode);
            }

            return result;
        }
    }
}
