using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
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
        public IEnumerable<(LinkTo link, IEnumerable<Strat> strats)> PathToDoor { get; set; } = Enumerable.Empty<(LinkTo link, IEnumerable<Strat> strats)>();

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
                        Strat strat = link.Strats.SingleOrDefault(strat => strat.Name == stratName);
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
            // Strats can be remove if they're declared out of logic, and a link with no strat would make sense to remove,
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

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node, CanLeaveCharged canLeaveCharged)
        {
            // All referenced nodes and links and strats belong to other objects, so nothing to do here
            return Enumerable.Empty<string>();
        }
    }
}
