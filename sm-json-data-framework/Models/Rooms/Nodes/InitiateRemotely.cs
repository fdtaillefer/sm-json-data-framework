using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    /// <summary>
    /// Contains info relating to a <see cref="CanLeaveCharged"/> being initiated remotely.
    /// This means it is initiated at a different node that the one by which the room will be exited.
    /// </summary>
    public class InitiateRemotely : InitializablePostDeserializableInCanLeaveCharged
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
        public IEnumerable<InitiateRemotelyPathToDoorNode> PathToDoorNodes { get; set; } = Enumerable.Empty<InitiateRemotelyPathToDoorNode>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room, RoomNode, CanLeaveCharged)"/> has been called.</para>
        /// <para>The path referenced by the <see cref="PathToDoorNodes"/> property, represented as links to follow and appropriate strats.</para>
        /// <para>This is the path that Samus must take through the room, from <see cref="InitiateAtNode"/> to <see cref="ExitNode"/>.</para>
        /// </summary>
        [JsonIgnore]
        public IList<(LinkTo link, IEnumerable<Strat> strats)> PathToDoor { get; set; } = new List<(LinkTo link, IEnumerable<Strat> strats)>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room, RoomNode, CanLeaveCharged)"/> has been called.</para>
        /// <para>The node through which this remote initiation ultimately exits the room charged.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode ExitNode { get; set; }

        public void Initialize(SuperMetroidModel model, Room room, RoomNode node, CanLeaveCharged canLeaveCharged)
        {
            // Initialize the start and end nodes of the remote canLeaveCharged
            InitiateAtNode = room.Nodes[InitiateAtNodeId];
            ExitNode = canLeaveCharged.Node;

            // Initialize the path to follow
            List<(LinkTo link, IEnumerable<Strat> strats)> pathToDoor = new List<(LinkTo, IEnumerable<Strat>)>();
            RoomNode currentNodeFrom = InitiateAtNode;
            bool pathPossible = true;
            foreach(var pathNode in PathToDoorNodes.TakeWhile(_ => pathPossible))
            {
                RoomNode destination = room.Nodes[pathNode.DestinationNodeId];
                
                if (currentNodeFrom.Links.TryGetValue(pathNode.DestinationNodeId, out LinkTo link))
                {

                    List<Strat> strats = new List<Strat>();
                    foreach(string stratName in pathNode.StratNames)
                    {
                        Strat strat = link.Strats.Values.SingleOrDefault(strat => strat.Name == stratName);
                        if (strat == null)
                        {
                            pathPossible = false;
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
                    pathPossible = false;
                }
            }

            // If the path had a node with no link or no strat, leave the path empty to represent that this cannot be done.
            // Strats can be removed if they're declared out of logic, and a link with no strat would make sense to remove,
            // so we won't just crash on this.
            // The CanLeaveCharged should probably just be taken out itself later by higher-level checks
            if(pathPossible)
            {
                // Validate that the path makes sense before we return

                // The path must end at the exit node
                if(pathToDoor.Last().link.TargetNode != node)
                {
                    throw new Exception($"PathToNode on a CanLeaveCharged of node {node.Name} does not end at that node.");
                }

                PathToDoor = pathToDoor;
            }
        }

        public void InitializeForeignProperties(SuperMetroidModel model, Room room, RoomNode node, CanLeaveCharged canLeaveCharged)
        {
            // Initialize the start and end nodes of the remote canLeaveCharged
            InitiateAtNode = room.Nodes[InitiateAtNodeId];
            ExitNode = canLeaveCharged.Node;

            // Initialize the path to follow
            List<(LinkTo link, IEnumerable<Strat> strats)> pathToDoor = new ();
            RoomNode currentNodeFrom = InitiateAtNode;
            foreach (var pathNode in PathToDoorNodes)
            {
                if(room.Nodes.TryGetValue(pathNode.DestinationNodeId, out RoomNode destination))
                {
                    if (currentNodeFrom.Links.TryGetValue(pathNode.DestinationNodeId, out LinkTo link))
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
            if (pathToDoor.Last().link.TargetNode != node)
            {
                throw new Exception($"PathToNode on a CanLeaveCharged of node {node.Name} does not end at that node.");
            }
            PathToDoor = pathToDoor;
        }

        public void InitializeOtherProperties(SuperMetroidModel model, Room room, RoomNode node, CanLeaveCharged canLeaveCharged)
        {
            // Nothing relevant to initialize
        }

        public bool CleanUpUselessValues(SuperMetroidModel model, Room room, RoomNode node, CanLeaveCharged canLeaveCharged)
        {
            // If this contains strats, they belong to a LinkTo. There's nothing fundamentally wrong with cleaning up those strats anyway though.

            for (int i = 0; i < PathToDoor.Count; i++)
            {
                var pathNode = PathToDoor[i];
                // Remove unusable strats
                pathNode.strats = pathNode.strats.Where(strat => strat.CleanUpUselessValues(model, room));
            }

            // If any node in the path has no strats remaining, it means the PathToNode is impossible to follow.
            // This makes the InitiateRemotely itself impossible to do.
            return PathToDoor.All(node => !node.strats.Any());
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node, CanLeaveCharged canLeaveCharged)
        {
            // All referenced nodes and links and strats belong to other objects, so nothing to do here
            return Enumerable.Empty<string>();
        }
    }
}
