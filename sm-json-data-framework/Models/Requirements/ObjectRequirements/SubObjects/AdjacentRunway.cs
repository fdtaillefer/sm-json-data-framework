using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    public class AdjacentRunway : AbstractObjectLogicalElement
    {
        [JsonPropertyName("fromNode")]
        public int FromNodeId { get; set; }

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The node that this element's FromNodeId references. </para>
        /// </summary>
        [JsonIgnore]
        public RoomNode FromNode {get;set;}

        public int UsedTiles { get; set; }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            if (room.Nodes.TryGetValue(FromNodeId, out RoomNode node))
            {
                FromNode = node;
                return Enumerable.Empty<string>();
            }
            else
            {
                return new[] { $"Node {FromNodeId} in room {room.Name}" };
            }
        }

        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, bool usePreviousRoom = false)
        {
            // We already have to look at the room prior to the one we're asked to evaluate
            // If we're being asked to evaluate the previous room, we have no way to obtain the state of the room before that so just return false
            if (usePreviousRoom)
            {
                return false;
            }

            // If we didn't exit the previous room through a node that leads to this one, we cannot use an adjacent runway
            RoomNode lastRoomExitNode = inGameState.GetCurrentNode(true);
            if (lastRoomExitNode?.OutNode != FromNode)
            {
                return false;
            }

            // If we aren't just entering the room at this node, we cannot use an adjacent runway
            if(inGameState.GetCurrentNode() != FromNode || inGameState.GetVisitedNodeIds().Count() > 1)
            {
                return false;
            }

            // At this point we know we just exited the previous room through a node that led to FromNode, and that we haven't moved since
            // We can use the adjacent runway so long as we can execute at least one strat of a long enough runway
            return lastRoomExitNode.Runways.Any(r => r.Length >= UsedTiles && r.Strats.Any(s => s.IsFulfilled(model, inGameState, true)));
        }
    }
}
