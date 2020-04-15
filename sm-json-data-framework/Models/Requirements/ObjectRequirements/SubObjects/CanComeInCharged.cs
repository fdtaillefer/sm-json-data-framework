using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Node;
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
        public RoomNode FromNode { get; set; }

        public IEnumerable<int> InRoomPath { get; set; } = Enumerable.Empty<int>();

        public int FramesRemaining { get; set; }

        public int ShinesparkFrames { get; set; }

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

        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            bool mustShinespark = ShinesparkFrames > 0;
            int energyNeededForShinespark = model.Rules.CalculateEnergyNeededForShinespark(ShinesparkFrames) * times;

            // Check simple preconditions before looking at runways
            if (!inGameState.HasSpeedBooster() || (mustShinespark && !model.CanShinespark()))
            {
                return false;
            }

            // Figure out if we can get charged in-room before looking at the adjacent room.
            // We can use the runway in either direction in that case.
            IEnumerable<(Runway runway, decimal length)> usableInRoomRunways = FromNode.Runways
                .Where(r => r.IsUsable(model, inGameState, false, times: times, usePreviousRoom: usePreviousRoom))
                .Select(r => (runway: r, length: Math.Max(model.Rules.CalculateEffectiveRunwayLength(r, model.LogicalOptions.TilesSavedWithStutter),
                                                         model.Rules.CalculateEffectiveReversedRunwayLength(r, model.LogicalOptions.TilesSavedWithStutter))));

            // If any runway is usable in-room, return true immediately
            if (usableInRoomRunways.Any(pair => pair.length >= model.LogicalOptions.TilesToShineCharge))
            {
                return true;
            }

            // All options we have left require evaluating the room prior to the one we're asked to evaluate
            // If we're being asked to evaluate the previous room, we have no way to obtain the state of the room before that so just return false
            if (usePreviousRoom)
            {
                return false;
            }

            // If no in-room path is specified, then player is expected to have entered at fromNode and not moved
            IEnumerable<int> inRoomPath = (InRoomPath == null || !InRoomPath.Any()) ? new[] { FromNodeId } : InRoomPath;

            // Now look for a usable adjacent runway that is long enough, when combined with a runway in this room
            decimal availableComingInTiles = usableInRoomRunways.Where(pair => pair.runway.UsableComingIn).Max(pair => pair.length);
            availableComingInTiles = Math.Max(0M, availableComingInTiles - model.Rules.RoomTransitionTilesLost);
            decimal requiredAdjacentRunwayLength = model.LogicalOptions.TilesToShineCharge - availableComingInTiles;

            // We can do this if we can find an adjacent runway we are able to use and that is long enough
            IEnumerable<Runway> adequateRunways = inGameState.GetRetroactiveRunways(inRoomPath, true)
                            .Where(r => model.Rules.CalculateEffectiveRunwayLength(r, model.LogicalOptions.TilesSavedWithStutter) >= requiredAdjacentRunwayLength)
                            .Where(r => r.IsUsable(model, inGameState, false, times: times, usePreviousRoom: true));
            if (adequateRunways.Any())
            {
                // Check energy for shinespark
                if (inGameState.IsResourceAvailable(ConsumableResourceEnum.ENERGY, energyNeededForShinespark))
                {
                    return true;
                }
            }

            // We can also do this if we can find a CanLeaveCharged we are able to use and that has enough FramesRemaining
            IEnumerable<CanLeaveCharged> adequateCanLeaveChargeds = inGameState.GetRetroactiveCanLeaveChargeds(inRoomPath, true)
                            .Where(clc => clc.FramesRemaining >= FramesRemaining)
                            .Where(clc => clc.IsUsable(model, inGameState, times: times, usePreviousRoom: true));
            if(adequateCanLeaveChargeds.Any())
            {
                // Check energy for shinespark
                if (inGameState.IsResourceAvailable(ConsumableResourceEnum.ENERGY, energyNeededForShinespark))
                {
                    return true;
                }
            }

            return false;

            // Note that there are no concerns here about unlocking the previous door, because unlocking a door to use it cannot be done retroactively.
            // It has to have already been done in order to use the door in the first place.
        }
    }
}
