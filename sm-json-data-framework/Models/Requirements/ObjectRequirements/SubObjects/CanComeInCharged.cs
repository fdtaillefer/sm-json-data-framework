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
    /// <summary>
    /// A logical element which requires Samus to have been able to come into the room either with a shinespark charged or, possibly, while doing a shinespark
    /// (depending on the required number of remaining frames in the shinespark charge))
    /// </summary>
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

        public override InGameState AttemptFulfill(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            bool mustShinespark = ShinesparkFrames > 0;
            int energyNeededForShinespark = model.Rules.CalculateEnergyNeededForShinespark(ShinesparkFrames) * times;
            Predicate<InGameState> hasEnergyForShinespark = igs => igs.IsResourceAvailable(ConsumableResourceEnum.ENERGY, energyNeededForShinespark);
            Action<InGameState> consumeShinesparkEnergy = igs => igs.ConsumeResource(ConsumableResourceEnum.ENERGY, energyNeededForShinespark);

            // Check simple preconditions before looking at anything
            if (!inGameState.HasSpeedBooster() || (mustShinespark && !model.CanShinespark()))
            {
                return null;
            }

            InGameState bestOverallResult = null;

            // So we have to see if we can use an in-room runway (not coming in).
            // And we have to see if we can use an adjacent runway by itself.
            // And we have to see if we can use a canLeavecharged.
            // And we have to see if we can combine an adjacent runway with a an in-room runway (coming in).
            // And after that we have to still be able to execute the shinespark.

            // We'll start with looking at in-room runways (not coming in).
            // For all in-room runways we are able to use while still doing the shinespark after, figure out the resulting state and effective length.
            IEnumerable<(Runway runway, InGameState resultingState, decimal length)> usableInRoomRunways = FromNode.Runways
                .Select(r => (runway: r, resultingState: r.AttemptUse(model, inGameState, comingIn: false, times: times, usePreviousRoom: usePreviousRoom)))
                .Where(p => p.resultingState != null && hasEnergyForShinespark(p.resultingState))
                .Select(p => (p.runway, p.resultingState, length: 
                    Math.Max(model.Rules.CalculateEffectiveRunwayLength(p.runway, model.LogicalOptions.TilesSavedWithStutter),
                        model.Rules.CalculateEffectiveReversedRunwayLength(p.runway, model.LogicalOptions.TilesSavedWithStutter))));

            // Find the best resulting state among all runways whose length is enough to fulfill this CanComeInCharged in-room
            InGameState bestInRoomResult = usableInRoomRunways.Where(t => t.length >= model.LogicalOptions.TilesToShineCharge)
                .Select(t => t.resultingState)
                .OrderByDescending(igs => igs, model.GetInGameStateComparer())
                .FirstOrDefault();

            // If using this in-room runway cost nothing, spend the shinespark and return
            if(model.CompareInGameStates(inGameState, bestInRoomResult) == 0)
            {
                consumeShinesparkEnergy(bestInRoomResult);
                return bestInRoomResult;
            }

            // If the best in-room runway we found is an improvement over the previous best solution, replace it
            if (model.CompareInGameStates(bestInRoomResult, bestOverallResult) > 0)
            {
                bestOverallResult = bestInRoomResult;
            }

            // Next Step: all adjacent runways with their resulting state.

            // If no in-room path is specified, then player is expected to have entered at fromNode and not moved
            IEnumerable<int> inRoomPath = (InRoomPath == null || !InRoomPath.Any()) ? new[] { FromNodeId } : InRoomPath;
            // Obtain info about all adjacent runways that can be used retroactively without dropping below the energy needed to shinespark
            IEnumerable<(Runway runway, InGameState resultingState, decimal length)> usableAdjacentRunways =
                inGameState.GetRetroactiveRunways(inRoomPath, usePreviousRoom: true)
                    .Select(r => (runway: r, resultingState: r.AttemptUse(model, inGameState, comingIn: false, times: times, usePreviousRoom: true)))
                    .Where(p => p.resultingState != null && hasEnergyForShinespark(p.resultingState))
                    .Select(p => (p.runway, p.resultingState, length: model.Rules.CalculateEffectiveRunwayLength(p.runway, model.LogicalOptions.TilesSavedWithStutter)));

            // Find the best resulting state among all reatroactive runways whose length is enough to fulfill this CanComeInCharged
            InGameState bestAdjacentRunwayResult = usableAdjacentRunways.Where(t => t.length >= model.LogicalOptions.TilesToShineCharge)
                .Select(t => t.resultingState)
                .OrderByDescending(igs => igs, model.GetInGameStateComparer())
                .FirstOrDefault();

            // If using this adjacent runway cost nothing, spend the shinespark and return
            if (model.CompareInGameStates(inGameState, bestAdjacentRunwayResult) == 0)
            {
                consumeShinesparkEnergy(bestAdjacentRunwayResult);
                return bestAdjacentRunwayResult;
            }

            // If the best adjacent runway we found is an improvement over the previous best solution, replace it
            if (model.CompareInGameStates(bestAdjacentRunwayResult, bestOverallResult) > 0)
            {
                bestOverallResult = bestAdjacentRunwayResult;
            }

            // Next step: Find the best retroactive CanLeaveCharged that has enough frames remaining and leaves Samus with enough energy for the shinespark
            IEnumerable<CanLeaveCharged> usableCanLeaveChargeds = inGameState.GetRetroactiveCanLeaveChargeds(inRoomPath, usePreviousRoom: true)
                .Where(clc => clc.FramesRemaining >= FramesRemaining);
            InGameState bestCanLeaveChargedState = model.ApplyOr(inGameState, usableCanLeaveChargeds,
                (clc, igs) => clc.AttemptUse(model, igs, times: times, usePreviousRoom: true),
                hasEnergyForShinespark);

            // If using this CanLeaveCharged cost nothing, spend the shinespark and return
            if (model.CompareInGameStates(inGameState, bestCanLeaveChargedState) == 0)
            {
                consumeShinesparkEnergy(bestCanLeaveChargedState);
                return bestCanLeaveChargedState;
            }

            // If the best CanLeaveCharged we found is an improvement over the previous best solution, replace it
            if (model.CompareInGameStates(bestCanLeaveChargedState, bestOverallResult) > 0)
            {
                bestOverallResult = bestCanLeaveChargedState;
            }

            // Next step: Find the best combination of adjacent and in-room runway
            // We can re-use the state from using the adjacent runways that we calculated, but not the in-room ones (because executions, not results, must be applied on top of each other)
            // Iterate over usable adjacent runways that actually offer a gain over the number of tiles lost by the room transation,
            // and match each of those against each in-room runway that is usable coming in and combines for a long enough runway
            foreach((Runway runway, InGameState resultingState, decimal length) currentAdjacentRunway in usableAdjacentRunways
                .Where(t => t.length > model.Rules.RoomTransitionTilesLost))
            {
                decimal requiredInRoomLength = model.LogicalOptions.TilesToShineCharge + model.Rules.RoomTransitionTilesLost - currentAdjacentRunway.length;

                // Determine which runways we may attempt to use. Limit to the ones we processed earlier because there's no point re-evaluating those we couldn't execute then,
                // but we'll re-attempt to use the runways using the resulting state of the current adjacent runway.
                // We'll also ignore in-room runways that are not long enough to combine with our current adjacent runway.
                IEnumerable<Runway> adequateRunways = usableInRoomRunways
                    .Where(t => t.length >= requiredInRoomLength)
                    .Select(t => t.runway);
                InGameState bestCurrentCombination = model.ApplyOr(currentAdjacentRunway.resultingState, adequateRunways,
                    (r, igs) => r.AttemptUse(model, igs, comingIn: true, times: times, usePreviousRoom: usePreviousRoom),
                    hasEnergyForShinespark);

                // If the best combination we found is free, spend energy for the shinespark and return it
                if (model.CompareInGameStates(inGameState, bestCurrentCombination) == 0)
                {
                    consumeShinesparkEnergy(bestCurrentCombination);
                    return bestCurrentCombination;
                }

                // If the best combination we found is an improvement over the previous best solution, replace it
                if (model.CompareInGameStates(bestCurrentCombination, bestOverallResult) > 0)
                {
                    bestOverallResult = bestCurrentCombination;
                }
            }
            
            // If we have found no solution at all that we can execute and that leaves us with enough energy for the shinespark, we cannot do this
            if (bestOverallResult == null)
            {
                return null;
            }
            // Apply shinespark on the best solution we've found and return it
            else
            {
                consumeShinesparkEnergy(bestOverallResult);
                return bestOverallResult;
            }
        }
    }
}
