using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
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
        public InitiateRemotely(UnfinalizedInitiateRemotely sourceElement, Action<InitiateRemotely> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            MustOpenDoorFirst = sourceElement.MustOpenDoorFirst;
            InitiateAtNode = sourceElement.InitiateAtNode.Finalize(mappings);
            PathToDoor = sourceElement.PathToDoor
                .Select(node => (node.link.Finalize(mappings), (IReadOnlyDictionary<string, Strat>)node.strats.Select(strat => strat.Finalize(mappings)).ToDictionary(strat => strat.Name).AsReadOnly()))
                .ToList().AsReadOnly();
            ExitNode = sourceElement.ExitNode.Finalize(mappings);
        }

        /// <summary>
        /// The node referenced by the <see cref="InitiateAtNodeId"/> property.
        /// </summary>
        public RoomNode InitiateAtNode { get; }

        /// <summary>
        /// Indicates whether the door at <see cref="ExitNode"/> needs to be opened before doing the remove initiation.
        /// For the door to be considered opened, it must have no active locks and its node must have been visited during the current room visit.
        /// </summary>
        public bool MustOpenDoorFirst { get; }

        /// <summary>
        /// <para>A path that must be followed by Samus to execute the remote CanLeaveCharged, represented as links to follow and appropriate strats(that are mapped by name).</para>
        /// <para>This is the path that Samus must take through the room, from <see cref="InitiateAtNode"/> to <see cref="ExitNode"/>.</para>
        /// </summary>
        public IReadOnlyList<(LinkTo linkTo, IReadOnlyDictionary<string, Strat> strats)> PathToDoor { get; }

        /// <summary>
        /// The node through which this remote initiation ultimately exits the room charged.
        /// </summary>
        public RoomNode ExitNode { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // If this contains strats, they belong to a LinkTo.
            // However, we need to apply the logical options to them to calculate if they become impossible
            for (int i = 0; i < PathToDoor.Count; i++)
            {
                var (_, strats) = PathToDoor[i];
                foreach (Strat strat in strats.Values)
                {
                    strat.ApplyLogicalOptions(logicalOptions, rules);
                }

            }
        }

        protected override void UpdateLogicalProperties(SuperMetroidRules rules)
        {
            base.UpdateLogicalProperties(rules);
            LogicallyNever = CalculateLogicallyNever(rules);
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidRules rules)
        {
            // An InitiateRemotely is logically relevant even if impossible, because its existence indicates that its CanLeaveCharged is impossible
            return true;
        }

        /// <summary>
        /// If true, then this remote initiation is impossible to execute given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            // If there is any node in the path that has no possible strat, then it's impossible to execute this InitiateRemotely
            return PathToDoor.Any(pathNode => !pathNode.strats.Values.WhereLogicallyRelevant().Any());
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
