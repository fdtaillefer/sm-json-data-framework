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

        public IEnumerable<int> InRoomPath { get; set; } = Enumerable.Empty<int>();

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
            // If no in-room path is specified, then player is expected to have entered at fromNode and not moved
            IEnumerable<int> inRoomPath = (InRoomPath == null || !InRoomPath.Any()) ?  new[] { FromNodeId } : InRoomPath;

            // This is fulfilled if there is a retroactive runway that the player is in a state to retroactively use, and which has a strat
            // the player can execute
            return inGameState.GetRetroactiveRunways(inRoomPath, usePreviousRoom).Any(r => r.Length >= UsedTiles && r.IsUsable(model, inGameState, false, true));

            // Note that there are no concerns here about unlocking the previous door, because unlocking a door to use it cannot be done retroactively.
            // It has to have already been done in order to use the door in the first place.
        }
    }
}
