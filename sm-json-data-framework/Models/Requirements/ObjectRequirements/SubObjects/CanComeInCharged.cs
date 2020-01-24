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
    public class CanComeInCharged : AbstractObjectLogicalElement
    {
        [JsonPropertyName("fromNode")]
        public int FromNodeId { get; set; }

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The node that this element's FromNodeId references. </para>
        /// </summary>
        [JsonIgnore]
        public RoomNode FromNodeNode { get; set; }

        public int FramesRemaining { get; set; }

        public int ShinesparkFrames { get; set; }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            if (room.Nodes.TryGetValue(FromNodeId, out RoomNode node))
            {
                FromNodeNode = node;
                return Enumerable.Empty<string>();
            }
            else
            {
                return new[] { $"Node {FromNodeId} in room {room.Name}" };
            }
        }

        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, bool usePreviousRoom = false)
        {
            // STITCHME Do something
            throw new NotImplementedException();
        }
    }
}
