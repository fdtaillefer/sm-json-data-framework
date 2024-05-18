using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class InitiateRemotely : AbstractModelElement<UnfinalizedInitiateRemotely, InitiateRemotely>
    {
        private UnfinalizedInitiateRemotely InnerElement { get; set; }

        public InitiateRemotely(UnfinalizedInitiateRemotely innerElement, Action<InitiateRemotely> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            InitiateAtNode = InnerElement.InitiateAtNode.Finalize(mappings);
            PathToDoor = InnerElement.PathToDoor
                .Select(node => (node.link.Finalize(mappings), (IReadOnlyDictionary<string, Strat>)node.strats.Select(strat => strat.Finalize(mappings)).ToDictionary(strat => strat.Name).AsReadOnly()))
                .ToList().AsReadOnly();
            ExitNode = InnerElement.ExitNode.Finalize(mappings);
        }

        /// <summary>
        /// The node referenced by the <see cref="InitiateAtNodeId"/> property.
        /// </summary>
        public RoomNode InitiateAtNode { get; }

        /// <summary>
        /// Indicates whether the door at <see cref="ExitNode"/> needs to be opened before doing the remove initiation.
        /// For the door to be considered opened, it must have no active locks and its node must have been visited during the current room visit.
        /// </summary>
        public bool MustOpenDoorFirst => InnerElement.MustOpenDoorFirst;

        /// <summary>
        /// <para>A path that must be followed by Samus to execute the remote CanLeaveCharged, represented as links to follow and appropriate strats(that are mapped by name).</para>
        /// <para>This is the path that Samus must take through the room, from <see cref="InitiateAtNode"/> to <see cref="ExitNode"/>.</para>
        /// </summary>
        public IReadOnlyList<(LinkTo linkTo, IReadOnlyDictionary<string, Strat> strats)> PathToDoor { get; }

        /// <summary>
        /// The node through which this remote initiation ultimately exits the room charged.
        /// </summary>
        public RoomNode ExitNode { get; }

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // If this contains strats, they belong to a LinkTo.
            // However, we need to apply the logical options to them to see if they become impossible
            bool anyNodeImpossible = false;
            for (int i = 0; i < PathToDoor.Count; i++)
            {
                var (_, strats) = PathToDoor[i];
                bool anyStratPossible = false;
                foreach (Strat strat in strats.Values)
                {
                    strat.ApplyLogicalOptions(logicalOptions);
                    if (!strat.UselessByLogicalOptions)
                    {
                        anyStratPossible = true;
                    }
                }

                // This node becomes impossible if no possible strat remains
                if (!anyStratPossible)
                {
                    anyNodeImpossible = true;
                }
            }

            // If there is a node in the path that has no possible strats remaining, this InitiateRemotely becomes impossible
            return anyNodeImpossible;
        }
    }

    /// <summary>
    /// Contains info relating to a <see cref="UnfinalizedCanLeaveCharged"/> being initiated remotely.
    /// This means it is initiated at a different node that the one by which the room will be exited.
    /// </summary>
    public class UnfinalizedInitiateRemotely : AbstractUnfinalizedModelElement<UnfinalizedInitiateRemotely, InitiateRemotely>, InitializablePostDeserializableInCanLeaveCharged
    {
        public int InitiateAtNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom, UnfinalizedRoomNode, UnfinalizedCanLeaveCharged)"/> has been called.</para>
        /// <para>The node referenced by the <see cref="InitiateAtNodeId"/> property.</para>
        /// </summary>
        public UnfinalizedRoomNode InitiateAtNode { get; set; }

        /// <summary>
        /// Indicates whether the door at <see cref="ExitNode"/> needs to be opened before doing the remove initiation.
        /// For the door to be considered opened, it must have no active locks and its node must have been visited during the current room visit.
        /// </summary>
        public bool MustOpenDoorFirst { get; set; }

        public IList<InitiateRemotelyPathToDoorNode> PathToDoorNodes { get; set; } = new List<InitiateRemotelyPathToDoorNode>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom, UnfinalizedRoomNode, UnfinalizedCanLeaveCharged)"/> has been called.</para>
        /// <para>The path referenced by the <see cref="PathToDoorNodes"/> property, represented as links to follow and appropriate strats.</para>
        /// <para>This is the path that Samus must take through the room, from <see cref="InitiateAtNode"/> to <see cref="ExitNode"/>.</para>
        /// </summary>
        public IList<(UnfinalizedLinkTo link, IList<UnfinalizedStrat> strats)> PathToDoor { get; set; } = new List<(UnfinalizedLinkTo link, IList<UnfinalizedStrat> strats)>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom, UnfinalizedRoomNode, UnfinalizedCanLeaveCharged)"/> has been called.</para>
        /// <para>The node through which this remote initiation ultimately exits the room charged.</para>
        /// </summary>
        public UnfinalizedRoomNode ExitNode { get; set; }

        public UnfinalizedInitiateRemotely()
        {

        }

        public UnfinalizedInitiateRemotely(RawInitiateRemotely rawInitiateRemotely)
        {
            InitiateAtNodeId = rawInitiateRemotely.InitiateAt;
            MustOpenDoorFirst = rawInitiateRemotely.MustOpenDoorFirst;
            PathToDoorNodes = rawInitiateRemotely.PathToDoor.Select(pathNode => new InitiateRemotelyPathToDoorNode(pathNode)).ToList();
        }

        protected override InitiateRemotely CreateFinalizedElement(UnfinalizedInitiateRemotely sourceElement, Action<InitiateRemotely> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new InitiateRemotely(sourceElement, mappingsInsertionCallback, mappings);
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room, UnfinalizedRoomNode node, UnfinalizedCanLeaveCharged canLeaveCharged)
        {
            // Initialize the start and end nodes of the remote canLeaveCharged
            InitiateAtNode = room.Nodes[InitiateAtNodeId];
            ExitNode = canLeaveCharged.Node;

            // Initialize the path to follow
            List<(UnfinalizedLinkTo link, IList<UnfinalizedStrat> strats)> pathToDoor = new ();
            UnfinalizedRoomNode currentNodeFrom = InitiateAtNode;
            foreach (var pathNode in PathToDoorNodes)
            {
                if(room.Nodes.TryGetValue(pathNode.DestinationNodeId, out UnfinalizedRoomNode destination))
                {
                    UnfinalizedLinkTo link = room.GetLinkBetween(currentNodeFrom.Id, pathNode.DestinationNodeId);
                    if (link != null)
                    {
                        List<UnfinalizedStrat> strats = new ();
                        foreach (string stratName in pathNode.StratNames)
                        {
                            UnfinalizedStrat strat = link.Strats.Values.SingleOrDefault(strat => strat.Name == stratName);
                            if (strat == null)
                            {
                                throw new Exception($"Strat {stratName} not found on link from node {currentNodeFrom.Id} to node {pathNode.DestinationNodeId}" +
                                    $"in room '{room.Name}'");
                            }
                            else
                            {
                                strats.Add(strat);
                            }
                        }
                        // Next node will start at current node's destination
                        currentNodeFrom = destination;
                        pathToDoor.Add((link, strats));
                    }
                    else
                    {
                        // Link doesn't exist, throw an exception with a helpful message.
                        // Links can be removed by clean-up, but not before all model properties have been initialized.
                        throw new LinkNotFoundException(room, currentNodeFrom, destination.Id);
                    }
                }
                else
                {
                    // Node doesn't exist, throw an exception with a helpful message
                    throw new NodeNotInRoomException(room, pathNode.DestinationNodeId);
                }
            }
            // Validate that the path makes sense before we assign PathToDoor
            // The path must end at the exit node
            if (pathToDoor.Last().link.TargetNodeId != node.Id)
            {
                string nodesPath = String.Join(", ", PathToDoorNodes.Select(node => node.DestinationNodeId.ToString()));
                throw new Exception($"PathToNode on a CanLeaveCharged of node {node.Id} in room '{node.Room.Name}' does not end at that node.\n" +
                    $"The nodes in the path are {{{nodesPath}}}");
            }
            PathToDoor = pathToDoor;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room, UnfinalizedRoomNode node, UnfinalizedCanLeaveCharged canLeaveCharged)
        {
            // All referenced nodes and links and strats belong to other objects, so nothing to do here
            return Enumerable.Empty<string>();
        }
    }
}
