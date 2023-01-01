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
    /// <summary>
    /// A logical element which requires Samus to have been able to come into the room while gathering momentum
    /// </summary>
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

        public decimal UsedTiles { get; set; }

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

        public override ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // If no in-room path is specified, then player is expected to have entered at fromNode and not moved
            IEnumerable<int> requiredInRoomPath = (InRoomPath == null || !InRoomPath.Any()) ? new[] { FromNodeId } : InRoomPath;

            // Find all runways from the previous room that can be retroactively attempted and are long enough.
            // We're calculating runway length to account for open ends, but using 0 for tilesSavedWithStutter because no charging is involved.
            IEnumerable<Runway> retroactiveRunways = inGameState.GetRetroactiveRunways(requiredInRoomPath, previousRoomCount)
                .Where(r => model.Rules.CalculateEffectiveRunwayLength(r, tilesSavedWithStutter: 0) >= UsedTiles);

            (_, var executionResult) = model.ExecuteBest(retroactiveRunways.Select(runway => runway.AsExecutable(comingIn: false)),
                inGameState, times: times, previousRoomCount: previousRoomCount);

            return executionResult;

            // Note that there are no concerns here about unlocking the previous door, because unlocking a door to use it cannot be done retroactively.
            // It has to have already been done in order to use the door in the first place.
        }
    }
}
