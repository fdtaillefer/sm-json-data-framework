using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Raw.Requirements;
using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Options;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    /// <summary>
    /// Represents a portion of a room.
    /// </summary>
    public class RoomNode : AbstractModelElement<UnfinalizedRoomNode, RoomNode>
    {
        private UnfinalizedRoomNode InnerElement { get; set; }

        public RoomNode(UnfinalizedRoomNode innerElement, Action<RoomNode> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            NodeItem = InnerElement.NodeItem?.Finalize(mappings);
            DoorEnvironments = InnerElement.DoorEnvironments.Select(environment => environment.Finalize(mappings)).ToList().AsReadOnly();
            InteractionRequires = InnerElement.InteractionRequires.Finalize(mappings);
            Runways = InnerElement.Runways.Values.Select(runway => runway.Finalize(mappings)).ToDictionary(runway => runway.Name).AsReadOnly();
            CanLeaveCharged = InnerElement.CanLeaveCharged.Select(canLeaveCharged => canLeaveCharged.Finalize(mappings)).ToList().AsReadOnly();
            OverrideSpawnAtNode = InnerElement.OverrideSpawnAtNode?.Finalize(mappings);
            SpawnAtNode = InnerElement.SpawnAtNode.Finalize(mappings);
            Locks = InnerElement.Locks.Values.Select(nodeLock => nodeLock.Finalize(mappings)).ToDictionary(nodeLock => nodeLock.Name).AsReadOnly();
            Utility = InnerElement.Utility.AsReadOnly();
            ViewableNodes = InnerElement.ViewableNodes.Select(viewableNode => viewableNode.Finalize(mappings)).ToList().AsReadOnly();
            Yields = InnerElement.Yields.Select(flag => flag.Finalize(mappings)).ToDictionary(flag => flag.Name).AsReadOnly();
            Note = InnerElement.Note?.AsReadOnly();
            Room = InnerElement.Room.Finalize(mappings);
            OutConnection = InnerElement.OutConnection?.Finalize(mappings);
            OutNode = InnerElement.OutNode?.Finalize(mappings);
            LinksTo = InnerElement.LinksTo.Values.Select(linkTo => linkTo.Finalize(mappings)).ToDictionary(linkTo => linkTo.TargetNode.Id).AsReadOnly();
            TwinDoorAddresses = InnerElement.TwinDoorAddresses.Select(twinAddress => twinAddress.Finalize(mappings)).ToList().AsReadOnly();
        }

        /// <summary>
        /// An in-room ID to identify the node. This is unique ONLY within the room!
        /// </summary>
        public int Id => InnerElement.Id;

        /// <summary>
        /// A string that identifies this node, often used as a key in Dictionaries. This is unique across the entire model.
        /// </summary>
        public string IdentifyingString => InnerElement.IdentifyingString;

        /// <summary>
        /// A human-legible name that identifies the node. This is unique across the entire model, but is long and unwieldy.
        /// </summary>
        public string Name => InnerElement.Name;

        /// <summary>
        /// The type of node this is.
        /// </summary>
        public NodeTypeEnum NodeType => InnerElement.NodeType;

        /// <summary>
        /// The subtype of this node. Which Value that make sense depends on the node type.
        /// </summary>
        public NodeSubTypeEnum NodeSubType => InnerElement.NodeSubType;

        /// <summary>
        /// The item that can be obtained by interacting with this node (if any).
        /// </summary>
        public Item NodeItem { get; }

        /// <summary>
        /// The in-game address of this node.
        /// </summary>
        public string NodeAddress => InnerElement.NodeAddress;

        /// <summary>
        /// The possible environments for this node. 
        /// Often there is just one, but sometimes which environment is in effect depends on what node the room was entered by.
        /// </summary>
        public IReadOnlyList<DoorEnvironment> DoorEnvironments { get; }

        /// <summary>
        /// Logical requirements that must be fulfilled to interact with this node, beyond taking care of any lock.
        /// </summary>
        public LogicalRequirements InteractionRequires { get; }

        /// <summary>
        /// Runways that can be used to gain momentum at this node, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Runway> Runways { get; }

        /// <summary>
        /// Ways that Samus can leave the room through this node with a number of frames remaining on a charged shinespark, or with an active shinespark.
        /// </summary>
        public IReadOnlyList<CanLeaveCharged> CanLeaveCharged { get; }

        /// <summary>
        /// If this is not null, it means that when Samus enters the room through this node, she instead spawns at the node in this property.
        /// If this is null, then Samus spawns at this node, as normal.
        /// </summary>
        public RoomNode OverrideSpawnAtNode { get; }

        /// <summary>
        /// The node at which Samus actually spawns upon entering the room via this node. In most cases it will be this node, but not always.
        /// </summary>
        public RoomNode SpawnAtNode { get; }

        /// <summary>
        /// Whether entering a room at this node effectively spawns Samus at a different node.
        /// </summary>
        public bool SpawnsAtDifferentNode => InnerElement.SpawnsAtDifferentNode;

        /// <summary>
        /// The locks that may prevent interaction with this node, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, NodeLock> Locks { get; }

        /// <summary>
        /// The set of utilities that Samus can make use of by interacting with this node.
        /// </summary>
        public IReadOnlySet<UtilityEnum> Utility { get; }

        /// <summary>
        /// The list of ways Samus can view other nodes from this node.
        /// </summary>
        public IReadOnlyList<ViewableNode> ViewableNodes { get; }

        /// <summary>
        /// The game flags that are activated by interacting with this node, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, GameFlag> Yields { get; }

        /// <summary>
        /// General information notes about this node.
        /// </summary>
        public IReadOnlyList<string> Note { get; }

        /// <summary>
        /// The room in which this node is.
        /// </summary>
        public Room Room { get; }

        /// <summary>
        /// If this node is a way out of the room, this is the one-way connection that connects this node to its destination.
        /// </summary>
        public Connection OutConnection { get; }

        /// <summary>
        /// If this node is a way out of the room, this is the node that leaving the room via this node leads to.
        /// </summary>
        public RoomNode OutNode { get; }

        /// <summary>
        /// Contains all in-room links from this node to another, mapped by the destination node ID.
        /// </summary>
        public IReadOnlyDictionary<int, LinkTo> LinksTo { get; }

        /// <summary>
        /// The list of twin doors that this node has.
        /// </summary>
        public IReadOnlyList<TwinDoorAddress> TwinDoorAddresses { get; }

        /// <summary>
        /// Returns the enumeration of locks on this node that are active and locked, according to the provided inGameState.
        /// </summary>
        /// <param name="inGameState">InGameState to evaluate for active locks</param>
        /// <returns></returns>
        public IEnumerable<NodeLock> GetActiveLocks(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            //Should remove that .Name and pass just nodeLock again, once I've removed all Unfinalized everywhere
            // Return locks whose locking conditions have been met, and that haven't been opened
            return Locks.Values.WhereUseful().Where(nodeLock => nodeLock.Lock.Execute(model, inGameState) != null)
                .Where(nodeLock => !inGameState.OpenedLocks.ContainsLock(nodeLock.Name));
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
            if (result == null)
            {
                return null;
            }

            // Actually interact with the node now


            // Activate game flags
            foreach (GameFlag flag in Node.Yields.Values)
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

    public class UnfinalizedRoomNode : AbstractUnfinalizedModelElement<UnfinalizedRoomNode, RoomNode>, InitializablePostDeserializeInRoom
    {
        public int Id { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>A string that identifies this node, often used as a key in Dictionaries.</para>
        /// </summary>
        public string IdentifyingString { get => SuperMetroidUtils.BuildNodeIdentifyingString(Room.Name, Id); }

        public string Name { get; set; }

        public NodeTypeEnum NodeType { get; set; }

        public NodeSubTypeEnum NodeSubType { get; set; }

        public string NodeItemName { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The item that can be obtained by interacting with this node (if any).</para>
        /// </summary>
        public UnfinalizedItem NodeItem { get; set; }

        public string NodeAddress { get; set; }

        public IList<UnfinalizedDoorEnvironment> DoorEnvironments { get; set; } = new List<UnfinalizedDoorEnvironment>();

        public UnfinalizedLogicalRequirements InteractionRequires { get; set; } = new UnfinalizedLogicalRequirements();

        /// <summary>
        /// Runways that can be used to gain momentum at this node, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedRunway> Runways { get; set; } = new Dictionary<string, UnfinalizedRunway>();

        public IList<UnfinalizedCanLeaveCharged> CanLeaveCharged { get; set; } = new List<UnfinalizedCanLeaveCharged>();

        // Decide who should handle null here
        public int? OverrideSpawnAtNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The node referenced by the <see cref="OverrideSpawnAtNodeId"/> property, if any.</para>
        /// </summary>
        public UnfinalizedRoomNode OverrideSpawnAtNode { get; set; }

        /// <summary>
        /// <para>Not reliable before <see cref="Initialize(UnfinalizedSuperMetroidModel)"/> has been called.</para>
        /// <para>The node at which Samus actually spawns upon entering the room via this node. In most cases it will be this node, but not always.</para>
        /// </summary>
        public UnfinalizedRoomNode SpawnAtNode => OverrideSpawnAtNode ?? this;

        /// <summary>
        /// Whether entering a room at this node effectively spawns Samus at a different node.
        /// </summary>
        public bool SpawnsAtDifferentNode => SpawnAtNode != this;

        /// <summary>
        /// The locks that may prevent interaction with this node, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedNodeLock> Locks { get; set; } = new Dictionary<string, UnfinalizedNodeLock>();

        public ISet<UtilityEnum> Utility { get; set; } = new HashSet<UtilityEnum>();

        public IList<UnfinalizedViewableNode> ViewableNodes { get; set; } = new List<UnfinalizedViewableNode>();

        public ISet<string> YieldsStrings { get; set; } = new HashSet<string>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The game flags that are activated by interacting with this node.</para>
        /// </summary>
        public IList<UnfinalizedGameFlag> Yields { get; set; }

        public IList<string> Note { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The room in which this node is.</para>
        /// </summary>
        public UnfinalizedRoom Room { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>If this node is a way out of the room, this is the one-way connection that connects this node to its destination.</para>
        /// </summary>
        public UnfinalizedConnection OutConnection { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>If this node is a way out of the room, this is the node that leaving the room via this node leads to.</para>
        /// </summary>
        public UnfinalizedRoomNode OutNode { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>Contains all in-room links from this node to another, mapped by the destination node ID.</para>
        /// </summary>
        public IDictionary<int, UnfinalizedLinkTo> LinksTo { 
            get 
            {
                if (Room.Links.TryGetValue(Id, out UnfinalizedLink link))
                {
                    return link.To;
                }
                // There are nodes with no links from them at all, for example some sandpit exits.
                // So returning an empty dictionary is perfectly fine.
                else
                {
                    return ImmutableDictionary<int, UnfinalizedLinkTo>.Empty;
                }
            } 
        }
        
        public IList<UnfinalizedTwinDoorAddress> TwinDoorAddresses { get; set; } = new List<UnfinalizedTwinDoorAddress>();

        public UnfinalizedRoomNode()
        {

        }

        public UnfinalizedRoomNode(RawRoomNode rawNode, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            Id = rawNode.Id;
            Name = rawNode.Name;
            NodeType = rawNode.NodeType;
            NodeSubType = rawNode.NodeSubType;
            NodeItemName = rawNode.NodeItem;
            NodeAddress = rawNode.NodeAddress;
            DoorEnvironments = rawNode.DoorEnvironments.Select(environment => new UnfinalizedDoorEnvironment(environment)).ToList();
            InteractionRequires = rawNode.InteractionRequires.ToLogicalRequirements(knowledgeBase);
            Runways = rawNode.Runways.Select(runway => new UnfinalizedRunway(runway, knowledgeBase)).ToDictionary(runway => runway.Name);
            CanLeaveCharged = rawNode.CanLeaveCharged.Select(clc => new Nodes.UnfinalizedCanLeaveCharged(clc, knowledgeBase)).ToList();
            OverrideSpawnAtNodeId = rawNode.SpawnAt;
            Locks = rawNode.Locks.Select(nodeLock => new UnfinalizedNodeLock(nodeLock, knowledgeBase)).ToDictionary(nodeLock => nodeLock.Name);
            Utility = new HashSet<UtilityEnum>(rawNode.Utility);
            ViewableNodes = rawNode.ViewableNodes.Select(viewableNode => new UnfinalizedViewableNode(viewableNode, knowledgeBase)).ToList();
            YieldsStrings = new HashSet<string>(rawNode.Yields);
            TwinDoorAddresses = rawNode.TwinDoorAddresses.Select(twinAddress => new UnfinalizedTwinDoorAddress(twinAddress)).ToList();
        }

        protected override RoomNode CreateFinalizedElement(UnfinalizedRoomNode sourceElement, Action<RoomNode> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new RoomNode(sourceElement, mappingsInsertionCallback, mappings);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            InteractionRequires.ApplyLogicalOptions(logicalOptions);

            foreach (UnfinalizedCanLeaveCharged canLeaveCharged in CanLeaveCharged)
            {
                canLeaveCharged.ApplyLogicalOptions(logicalOptions);
            }

            foreach (UnfinalizedDoorEnvironment doorEnvironment in DoorEnvironments)
            {
                doorEnvironment.ApplyLogicalOptions(logicalOptions);
            }

            foreach (UnfinalizedViewableNode viewableNode in ViewableNodes)
            {
                viewableNode.ApplyLogicalOptions(logicalOptions);
            }

            foreach (UnfinalizedNodeLock nodeLock in Locks.Values)
            {
                nodeLock.ApplyLogicalOptions(logicalOptions);
            }

            foreach (UnfinalizedRunway runway in Runways.Values)
            {
                runway.ApplyLogicalOptions(logicalOptions);
            }

            foreach (UnfinalizedTwinDoorAddress twinDoorAddress in TwinDoorAddresses)
            {
                twinDoorAddress.ApplyLogicalOptions(logicalOptions);
            }

            // Links belong to rooms, not nodes, so we don't have to propagate to them if we don't the information.

            // A node never becomes useless
            return false;
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            Room = room;

            // Initialize OutConnection and OutNode
            if (NodeType == NodeTypeEnum.Exit || NodeType == NodeTypeEnum.Door)
            {
                if (model.Connections.TryGetValue(IdentifyingString, out UnfinalizedConnection connection))
                {
                    OutConnection = connection;
                    UnfinalizedConnectionNode otherNode = connection.ToNode;
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
            Yields = YieldsStrings.Select(s => model.GameFlags[s]).ToList();

            // Initialize item
            if (NodeItemName != null)
            {
                NodeItem = model.Items[NodeItemName];
            }

            // Initialize CanLeaveChargeds
            foreach (UnfinalizedCanLeaveCharged canLeaveCharged in CanLeaveCharged)
            {
                canLeaveCharged.InitializeProperties(model, room, this);
            }

            // Initialize DoorEnvironments
            foreach (UnfinalizedDoorEnvironment doorEnvironment in DoorEnvironments)
            {
                doorEnvironment.InitializeProperties(model, room, this);
            }

            // Initialize ViewableNodes
            foreach (UnfinalizedViewableNode viewableNode in ViewableNodes)
            {
                viewableNode.InitializeProperties(model, room, this);
            }

            // Initialize Locks
            foreach (UnfinalizedNodeLock nodeLock in Locks.Values)
            {
                nodeLock.InitializeProperties(model, room, this);
            }

            // Initialize Runways
            foreach (UnfinalizedRunway runway in Runways.Values)
            {
                runway.InitializeProperties(model, room, this);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(InteractionRequires.InitializeReferencedLogicalElementProperties(model, room));

            foreach(UnfinalizedRunway runway in Runways.Values)
            {
                unhandled.AddRange(runway.InitializeReferencedLogicalElementProperties(model, room, this));
            }

            foreach(UnfinalizedCanLeaveCharged canLeaveCharged in CanLeaveCharged)
            {
                unhandled.AddRange(canLeaveCharged.InitializeReferencedLogicalElementProperties(model, room, this));
            }

            foreach(UnfinalizedNodeLock nodeLock in Locks.Values)
            {
                unhandled.AddRange(nodeLock.InitializeReferencedLogicalElementProperties(model, room, this));
            }

            foreach(UnfinalizedViewableNode viewableNode in ViewableNodes)
            {
                unhandled.AddRange(viewableNode.InitializeReferencedLogicalElementProperties(model, room, this));
            }

            return unhandled.Distinct();
        }
    }
}
