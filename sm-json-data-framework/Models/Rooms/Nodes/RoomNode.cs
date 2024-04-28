using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Raw.Requirements;
using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        [JsonIgnore]
        public bool SpawnsAtDifferentNode { get { return SpawnAtNode != this; } }

        /// <summary>
        /// The locks that may prevent interaction with this node, mapped by name.
        /// </summary>
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
        /// <para>Contains all in-room links from this node to another, mapped by the destination node ID.</para>
        /// </summary>
        [JsonIgnore]
        public IDictionary<int, LinkTo> Links { 
            get 
            {
                if (Room.Links.TryGetValue(Id, out Link link))
                {
                    return link.To;
                }
                // There are nodes with no links from them at all, for example some sandpit exits.
                // So returning an empty dictionary is perfectly fine.
                else
                {
                    return ImmutableDictionary<int, LinkTo>.Empty;
                }
            } 
        }
        
        public IEnumerable<TwinDoorAddress> TwinDoorAddresses { get; set; } = Enumerable.Empty<TwinDoorAddress>();

        public RoomNode()
        {

        }

        public RoomNode(RawRoomNode node, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            Id = node.Id;
            Name = node.Name;
            NodeType = node.NodeType;
            NodeSubType = node.NodeSubType;
            NodeItemName = node.NodeItem;
            NodeAddress = node.NodeAddress;
            DoorEnvironments = node.DoorEnvironments.Select(environment => new DoorEnvironment(environment));
            InteractionRequires = node.InteractionRequires.ToLogicalRequirements(knowledgeBase);
            Runways = node.Runways.Select(runway => new Runway(runway, knowledgeBase));
            CanLeaveCharged = node.CanLeaveCharged.Select(clc => new Nodes.CanLeaveCharged(clc, knowledgeBase));
            OverrideSpawnAtNodeId = node.SpawnAt;
            Locks = node.Locks.Select(nodeLock => new NodeLock(nodeLock, knowledgeBase)).ToDictionary(nodeLock => nodeLock.Name);
            Utility = new HashSet<UtilityEnum>(node.Utility);
            ViewableNodes = node.ViewableNodes.Select(viewableNode => new ViewableNode(viewableNode, knowledgeBase));
            YieldsStrings = new HashSet<string>(node.Yields);
            TwinDoorAddresses = node.TwinDoorAddresses.Select(twinAddress => new TwinDoorAddress(twinAddress));
        }

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

        public void InitializeProperties(SuperMetroidModel model, Room room)
        {
            Room = room;

            // Initialize OutConnection and OutNode
            if (NodeType == NodeTypeEnum.Exit || NodeType == NodeTypeEnum.Door)
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
                        throw new Exception($"A connection thought to be going to node {OutNode.Name} claims to be going to a node named {connection.ToNode.NodeName}");
                    }
                }
                // It's probably not worth an exception if there's no connection going from this node? Or maybe it is? The node type does imply there should be one...
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

            // Initialize CanLeaveChargeds
            foreach (CanLeaveCharged canLeaveCharged in CanLeaveCharged)
            {
                canLeaveCharged.InitializeProperties(model, room, this);
            }

            // Initialize DoorEnvironments
            foreach (DoorEnvironment doorEnvironment in DoorEnvironments)
            {
                doorEnvironment.InitializeProperties(model, room, this);
            }

            // Initialize ViewableNodes
            foreach (ViewableNode viewableNode in ViewableNodes)
            {
                viewableNode.InitializeProperties(model, room, this);
            }

            // Initialize Locks
            foreach (NodeLock nodeLock in Locks.Values)
            {
                nodeLock.InitializeProperties(model, room, this);
            }

            // Initialize Runways
            foreach (Runway runway in Runways)
            {
                runway.InitializeProperties(model, room, this);
            }
        }

        public bool CleanUpUselessValues(SuperMetroidModel model, Room room)
        {
            // Cleanup CanLeaveChargeds
            CanLeaveCharged = CanLeaveCharged.Where(clc => clc.CleanUpUselessValues(model, room, this));

            // Cleanup DoorEnvironments
            DoorEnvironments = DoorEnvironments.Where(environment => environment.CleanUpUselessValues(model, room, this));

            // Cleanup ViewableNodes
            ViewableNodes = ViewableNodes.Where(viewableNode => viewableNode.CleanUpUselessValues(model, room, this));

            // Cleanup Locks
            Locks = Locks.Where(kvp => kvp.Value.CleanUpUselessValues(model, room, this)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Cleanup Runways
            Runways = Runways.Where(runway => runway.CleanUpUselessValues(model, room, this));

            // A node never becomes useless
            return true;
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
