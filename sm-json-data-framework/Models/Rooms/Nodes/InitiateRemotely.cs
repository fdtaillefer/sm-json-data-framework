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
    /// <summary>
    /// Contains info relating to a <see cref="CanLeaveCharged"/> being initiated remotely.
    /// This means it is initiated at a different node that the one by which the room will be exited.
    /// </summary>
    public class InitiateRemotely : AbstractModelElement, InitializablePostDeserializableInCanLeaveCharged
    {
        [JsonPropertyName("initiateAt")]
        public int InitiateAtNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room, RoomNode, CanLeaveCharged)"/> has been called.</para>
        /// <para>The node referenced by the <see cref="InitiateAtNodeId"/> property.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode InitiateAtNode { get; set; }

        /// <summary>
        /// Indicates whether the door at <see cref="ExitNode"/> needs to be opened before doing the remove initiation.
        /// For the door to be considered opened, it must have no active locks and its node must have been visited during the current room visit.
        /// </summary>
        public bool MustOpenDoorFirst { get; set; }

        [JsonPropertyName("pathToDoor")]
        public IList<InitiateRemotelyPathToDoorNode> PathToDoorNodes { get; set; } = new List<InitiateRemotelyPathToDoorNode>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room, RoomNode, CanLeaveCharged)"/> has been called.</para>
        /// <para>The path referenced by the <see cref="PathToDoorNodes"/> property, represented as links to follow and appropriate strats.</para>
        /// <para>This is the path that Samus must take through the room, from <see cref="InitiateAtNode"/> to <see cref="ExitNode"/>.</para>
        /// </summary>
        [JsonIgnore]
        public IList<(LinkTo link, IList<Strat> strats)> PathToDoor { get; set; } = new List<(LinkTo link, IList<Strat> strats)>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room, RoomNode, CanLeaveCharged)"/> has been called.</para>
        /// <para>The node through which this remote initiation ultimately exits the room charged.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode ExitNode { get; set; }

        public InitiateRemotely()
        {

        }

        public InitiateRemotely(RawInitiateRemotely rawInitiateRemotely)
        {
            InitiateAtNodeId = rawInitiateRemotely.InitiateAt;
            MustOpenDoorFirst = rawInitiateRemotely.MustOpenDoorFirst;
            PathToDoorNodes = rawInitiateRemotely.PathToDoor.Select(pathNode => new InitiateRemotelyPathToDoorNode(pathNode)).ToList();
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // If this contains strats, they belong to a LinkTo.
            // However, we need to apply the logical options to them to see if they become impossible
            bool anyNodeImpossible = false;
            for (int i = 0; i < PathToDoor.Count; i++)
            {
                var (_, strats) = PathToDoor[i];
                bool anyStratPossible = false;
                foreach(Strat strat in strats)
                {
                    strat.ApplyLogicalOptions(logicalOptions);
                    if (!strat.UselessByLogicalOptions)
                    {
                        anyStratPossible = true;
                    }
                }
                
                // This node becomes impossible if no possible strat remains
                if (!anyStratPossible) {
                    anyNodeImpossible = true;
                }
            }
               
            // If there is a node in the path that has no possible strats remaining, this InitiateRemotely becomes impossible
            return anyNodeImpossible;
        }

        public void InitializeProperties(SuperMetroidModel model, Room room, RoomNode node, CanLeaveCharged canLeaveCharged)
        {
            // Initialize the start and end nodes of the remote canLeaveCharged
            InitiateAtNode = room.Nodes[InitiateAtNodeId];
            ExitNode = canLeaveCharged.Node;

            // Initialize the path to follow
            List<(LinkTo link, IList<Strat> strats)> pathToDoor = new ();
            RoomNode currentNodeFrom = InitiateAtNode;
            foreach (var pathNode in PathToDoorNodes)
            {
                if(room.Nodes.TryGetValue(pathNode.DestinationNodeId, out RoomNode destination))
                {
                    LinkTo link = room.GetLinkBetween(currentNodeFrom.Id, pathNode.DestinationNodeId);
                    if (link != null)
                    {
                        List<Strat> strats = new ();
                        foreach (string stratName in pathNode.StratNames)
                        {
                            Strat strat = link.Strats.Values.SingleOrDefault(strat => strat.Name == stratName);
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

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node, CanLeaveCharged canLeaveCharged)
        {
            // All referenced nodes and links and strats belong to other objects, so nothing to do here
            return Enumerable.Empty<string>();
        }
    }
}
